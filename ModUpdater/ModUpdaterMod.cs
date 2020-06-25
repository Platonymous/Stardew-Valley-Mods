using StardewModdingAPI;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Octokit;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;

namespace ModUpdater
{
    public class ModUpdaterMod : Mod
    {
        internal static Config config;
        internal static List<ModUpdateManifest> updated;
        internal static GitHubClient github;
        internal static bool loggedNextUpdateCheck = false;
        internal static Dictionary<string, IReadOnlyList<RepositoryContent>> repoContents;
        internal static int update = ModUpdate();
        internal static bool shouldUpdate = false;

        public static int ModUpdate()
        {
            updated = new List<ModUpdateManifest>();
            repoContents = new Dictionary<string, IReadOnlyList<RepositoryContent>>();
            string modsPath = (string)typeof(Constants).GetProperty("ModsPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
            string configFile = Path.Combine(modsPath, "ModUpdater", "config.json");

            github = github ?? new GitHubClient(new ProductHeaderValue("Platonymous.ModUpdater", "1.0.0"));

            if (!File.Exists(configFile))
                File.WriteAllText(configFile,Newtonsoft.Json.JsonConvert.SerializeObject(new Config()));
            
            config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile));

            if (config.GitHubUser != "")
            {
                var basicAuth = new Credentials(config.GitHubUser, config.GitHubPassword);
                github.Credentials = basicAuth;
            }

                if ((DateTime.Now - config.LastUpdateCheck).TotalMinutes >= config.Interval)
            {
                config.LastUpdateCheck = DateTime.Now;
                shouldUpdate = true;
            }

            if (!shouldUpdate)
                return 0;

            File.WriteAllText(configFile, Newtonsoft.Json.JsonConvert.SerializeObject(config));


            int result = 0;
            foreach (var manifestFile in Directory.GetFiles(modsPath, "manifest.json", SearchOption.AllDirectories)) {

                string dir = Path.GetDirectoryName(manifestFile);

                if (dir.StartsWith("."))
                    continue;

                var parent = Directory.GetParent(dir);

                if (parent.Name.StartsWith("."))
                    continue;

                ModUpdateManifest mod = Newtonsoft.Json.JsonConvert.DeserializeObject<ModUpdateManifest>(File.ReadAllText(manifestFile));
   
                try
                {

                    if (config.Exclude.Contains(mod.UniqueID))
                        continue;

                    if (mod.ModUpdater.ModFolder == "" && !string.IsNullOrEmpty(mod.EntryDll))
                        mod.ModUpdater.ModFolder = mod.EntryDll.Substring(0, mod.EntryDll.Length - 4);

                    if (mod.ModUpdater.Repository != "")
                        result += CheckMod(modsPath, parent, mod);
                    else if (mod.Author == "Platonymous" && !string.IsNullOrEmpty(mod.EntryDll))
                    {
                        mod.ModUpdater = new PyModUpdateInformation(mod.EntryDll.Substring(0, mod.EntryDll.Length - 4));
                        result += CheckMod(modsPath, parent, mod);
                    }
                }
                catch (RateLimitExceededException e)
                {
                    Console.WriteLine("[ModUpdater] [" + mod.UniqueID + "] Updater failed: " + "API Rate Limit exceeded. Please try again later.");
                    continue;
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ModUpdater] [" + mod.UniqueID + "] Updater failed. Please try again later.");
                    continue;
                }
            }

       
            if (result > 0)
                patchModLoad();

            string tempFolder = Path.Combine(modsPath, "ModUpdater", "Temp");

            if (Directory.Exists(tempFolder))
                new DirectoryInfo(tempFolder).Delete(true);
            return result;
        }

        public static int CheckMod(string modsPath, DirectoryInfo parent, ModUpdateManifest mod)
        {
            if (!shouldUpdate && !mod.ModUpdater.Install)
            {
                if (!loggedNextUpdateCheck)
                {
                    Console.WriteLine("[ModUpdater] Next update check: " + (config.LastUpdateCheck.AddMinutes(config.Interval).ToString("s")));
                    loggedNextUpdateCheck = true;
                }
                    return 0;
            }

            string tempFolder = Path.Combine(modsPath, "ModUpdater", "Temp");
            var currentVersion = mod.Version;


            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            var rkey = mod.ModUpdater.User + ">" + mod.ModUpdater.Repository + ">" + mod.ModUpdater.ModFolder;
            IReadOnlyList<RepositoryContent> rContent = null;
            if (repoContents.ContainsKey(rkey))
                rContent = repoContents[rkey];

            Repository repo = null;

            if (rContent == null)
            {
                var repoRequest = github.Repository.Get(mod.ModUpdater.User, mod.ModUpdater.Repository);
                repoRequest.Wait();
                repo = repoRequest.Result;
            }

            Console.WriteLine("[ModUpdater] Checking for updates: " + mod.Name);
            Console.WriteLine("[ModUpdater] Current version: " + currentVersion);

            
            if (rContent != null || repo is Repository)
            {
                if (rContent == null)
                {
                    var fileRequest = github.Repository.Content.GetAllContents(repo.Id, mod.ModUpdater.Directory);
                    fileRequest.Wait();
                    var files = fileRequest.Result;
                    rContent = files;
                    repoContents.Add(rkey, rContent);
                }

                var selector = mod.ModUpdater.FileSelector.Replace("{ModFolder}", mod.ModUpdater.ModFolder);
                Regex findFile = new Regex(selector, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                var filesFound = rContent.Where(f =>
                {
                   var m = findFile.Match(Path.GetFileNameWithoutExtension(f.Path));
                    return m.Success && m.Groups.Count == 2;
                });

                if (filesFound.Count() == 0)
                    return 0;

                foreach (RepositoryContent file in filesFound)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file.Path);
                    var match = findFile.Match(fileName);
                    string newVersion = match.Groups[1].Value;

                    if (SemanticVersion.TryParse(newVersion, out ISemanticVersion version) 
                        && (mod.ModUpdater.Install || (shouldUpdate && SemanticVersion.TryParse(currentVersion, out ISemanticVersion current) && version.IsNewerThan(current))))
                    {
                        if (version.IsPrerelease() && !config.LoadPrereleases)
                            continue;

                        var url = file.DownloadUrl;
                        var tempFile = Path.Combine(tempFolder, Path.GetFileName(file.Path));

                        using (WebClient client = new WebClient())
                            client.DownloadFile(url, tempFile);

                        ModUpdateManifest updateManifest = null;

                        using (ZipArchive zip1 = ZipFile.OpenRead(tempFile))
                        {
                            if (zip1.Entries.FirstOrDefault(entry => entry.Name.Equals("manifest.json", StringComparison.InvariantCultureIgnoreCase)) is ZipArchiveEntry manifestEntry)
                            {
                                using (StreamReader sr = new StreamReader(manifestEntry.Open(), System.Text.Encoding.UTF8))
                                {
                                    if (Newtonsoft.Json.JsonConvert.DeserializeObject<ModUpdateManifest>(sr.ReadToEnd()) is ModUpdateManifest um)
                                        updateManifest = um;

                                    if (updateManifest is ModUpdateManifest
                                        && SemanticVersion.TryParse(updateManifest.MinimumApiVersion, out ISemanticVersion updateApiVersion)
                                        && Constants.ApiVersion.IsOlderThan(updateApiVersion))
                                    {
                                        Console.WriteLine("[ModUpdater] [" + updateManifest.UniqueID + "]" + "Could not update to version" + updateManifest.Version + ". Need at least SMAPI " + updateManifest.MinimumApiVersion);
                                        continue;
                                    }
                                }
                            }else
                                continue;

                            foreach (ZipArchiveEntry e in zip1.Entries)
                            {
                                var tPath = Path.Combine(modsPath, e.FullName);
                                var tDirectory = Path.Combine(modsPath, Path.GetDirectoryName(e.FullName));
                                if (!Directory.Exists(tDirectory))
                                    Directory.CreateDirectory(tDirectory);

                                Console.WriteLine("[ModUpdater] " + " [" + mod.UniqueID + "] " + "Updating file: " + tPath);
                                e.ExtractToFile(tPath, true);
                            }
                        }

                        Console.WriteLine("[ModUpdater]  [" + mod.UniqueID + "] " + mod.Name + " was successfully updated to version " + version);

                        if (updateManifest is ModUpdateManifest)
                            updated.Add(updateManifest);

                        return 1;
                    }
                }
            }

            return 0;
        }

        public static void patchModLoad()
        {
            Harmony.HarmonyInstance harmony = Harmony.HarmonyInstance.Create("Platonymous.ModUpdater");
            harmony.Patch(
                original:Type.GetType("StardewModdingAPI.Framework.SCore, StardewModdingAPI").GetMethod("CheckForUpdatesAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                prefix: new Harmony.HarmonyMethod(Harmony.AccessTools.DeclaredMethod(typeof(ModUpdaterMod),nameof(CheckForUpdatesAsync)))
                );

            harmony.Patch(
                original: Type.GetType("StardewModdingAPI.Framework.SCore, StardewModdingAPI").GetMethod("TryLoadMod", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                prefix: new Harmony.HarmonyMethod(Harmony.AccessTools.DeclaredMethod(typeof(ModUpdaterMod), nameof(TryLoadMod)))
                );
        }
        
        public static bool CheckForUpdatesAsync(object __instance, ref IModInfo[] mods)
        {
            if (update > 0 && config.AutoRestart && Constants.TargetPlatform == GamePlatform.Windows)
            {
                Process.Start(Path.Combine(Constants.ExecutionPath, "StardewModdingAPI.exe"), string.IsNullOrEmpty(config.ExecutionArgs) ? null : config.ExecutionArgs);
                Environment.Exit(-1);
                return false;
            }

            return true;
        }

        public static void TryLoadMod(ref IModInfo mod)
        {
            if (update > 0)
                foreach (var u in updated)
                    if (SemanticVersion.TryParse(u.Version, out ISemanticVersion version) && u.UniqueID == mod.Manifest.UniqueID)
                        mod.Manifest.GetType().GetProperty("Version").SetValue(mod.Manifest, version);
        }

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            helper.WriteConfig<Config>(config);

            helper.Events.GameLoop.GameLaunched += (s,e) =>
            {
                if(update > 0)
                    Monitor.Log(update + " Mod" + (update == 1 ? " was" : "s were") + " updated. A restart is recommended.", LogLevel.Warn);
            };
        }
    }
}
