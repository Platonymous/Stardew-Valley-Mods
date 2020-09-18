using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Toolkit.Utilities;
using StardewValley;
using StardewXnbHack.Framework;
using StardewXnbHack.Framework.Writers;

namespace StardewXnbHackMod
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.ConsoleCommands.Add("xnbhack", "Unpacks all assets in 'Content' to 'Content (unpacked)", this.Unpack);
        }

        public void Unpack(string call, string[] parameter)
        {
            IAssetWriter[] assetWriters = {
            new MapWriter(),
            new SpriteFontWriter(),
            new TextureWriter(),
            new XmlSourceWriter(),
            new DataWriter() // check last due to more expensive CanWrite
            };

            Platform platform = EnvironmentUtility.DetectPlatform();
            string gamePath = Constants.ExecutionPath;

            this.Monitor.Log($"Found game folder: {gamePath}.", LogLevel.Info);
            this.Monitor.Log("");

            // get import/export paths
            string contentPath = Path.Combine(gamePath, "Content");
            string exportPath = Path.Combine(gamePath, "Content (unpacked)");

            // symlink files on Linux/Mac
            if (platform == Platform.Linux || platform == Platform.Mac)
            {
                Process.Start("ln", $"-sf \"{Path.Combine(gamePath, "Content")}\"");
                Process.Start("ln", $"-sf \"{Path.Combine(gamePath, "lib")}\"");
                Process.Start("ln", $"-sf \"{Path.Combine(gamePath, "lib64")}\"");
            }

            ConsoleProgressBar progressBar;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Game1 game = Game1.game1;

            this.Monitor.Log("");
            this.Monitor.Log("Unpacking files...", LogLevel.Info);

            // collect files
            DirectoryInfo contentDir = new DirectoryInfo(contentPath);
            FileInfo[] files = contentDir.EnumerateFiles("*.xnb", SearchOption.AllDirectories).ToArray();
            progressBar = new ModConsoleProgressBar(this.Monitor, files.Length, Console.Title);

            // write assets
            foreach (FileInfo file in files)
            {
                // prepare paths
                string assetName = file.FullName.Substring(contentPath.Length + 1, file.FullName.Length - contentPath.Length - 5); // remove root path + .xnb extension
                string fileExportPath = Path.Combine(exportPath, assetName);
                Directory.CreateDirectory(Path.GetDirectoryName(fileExportPath));

                // show progress bar
                progressBar.Increment();
                progressBar.Print(assetName);

                // read asset
                object asset = null;
                try
                {
                    asset = game.Content.Load<object>(assetName);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{assetName} => read error: {ex.Message}", LogLevel.Error);
                    continue;
                }

                // write asset
                try
                {
                    // get writer
                    IAssetWriter writer = assetWriters.FirstOrDefault(p => p.CanWrite(asset));

                    // write file
                    if (writer == null)
                    {
                        this.Monitor.Log($"{assetName}.xnb ({asset.GetType().Name}) isn't a supported asset type.", LogLevel.Warn);
                        File.Copy(file.FullName, $"{fileExportPath}.xnb", overwrite: true);
                    }
                    else if (!writer.TryWriteFile(asset, fileExportPath, assetName, platform, out string writeError))
                    {
                        this.Monitor.Log($"{assetName}.xnb ({asset.GetType().Name}) could not be saved: {writeError}.", LogLevel.Warn);
                        File.Copy(file.FullName, $"{fileExportPath}.xnb", overwrite: true);
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{assetName} => export error: {ex.Message}", LogLevel.Error);
                }
                finally
                {
                    game.Content.Unload();
                }
            }

            this.Monitor.Log($"Done! Unpacked files to {exportPath}.", LogLevel.Info);
        }
    }
}
