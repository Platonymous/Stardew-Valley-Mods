using System;
using StardewModdingAPI;
using PyTK.Extensions;
using System.IO;
using Harmony;

namespace ScaleUp
{
    public class ScaleUpMod : Mod
    {
        public static bool shouldBeTrue = false;

        public override void Entry(IModHelper helper)
        {
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.ScaleUp");
            instance.Patch(Type.GetType("StardewModdingAPI.Toolkit.Utilities.PathUtilities, StardewModdingAPI.Toolkit").GetMethod("IsSafeRelativePath"), null, new HarmonyMethod(GetType().GetMethod("IsSafeRelativePath")));

            loadContentPacks();
            helper.Content.AssetEditors.Add(new Scaler(helper));
        }

        public static void IsSafeRelativePath(string path, ref bool __result)
        {
            __result = __result || shouldBeTrue;
            shouldBeTrue = false;
        }

        private void loadContentPacks()
        {
            var directory = new DirectoryInfo(Helper.DirectoryPath).Parent;
            string[] files = Directory.GetFiles(directory.FullName, "manifest.json", SearchOption.AllDirectories);

            foreach(string fullPath in files)
            {
                FileInfo manifestFile = new FileInfo(fullPath);
                shouldBeTrue = true;
                Manifest manifest = this.Helper.Data.ReadJsonFile<Manifest>(Path.Combine("..", manifestFile.Directory.Name, manifestFile.Name));

                if (manifest != null && manifest.ContentPackFor is ManifestContentPackFor m && m.UniqueID == "Pathoschild.ContentPatcher")
                {
                    shouldBeTrue = true;
                    Content content = Helper.Data.ReadJsonFile<Content>(Path.Combine("..", manifestFile.Directory.Name, "content.json"));
                    if (content == null)
                        continue;

                    foreach(Changes change in content.Changes)
                    {
                        if (change.Action != "Load" || !change.ScaleUp || change.OriginalWidth == -1 || change.Target == "")
                            continue;

                        Monitor.Log("Mark for scaling: " + change.Target);

                        Scaler.Assets.AddOrReplace(change.Target, change.OriginalWidth);
                    }

                }
            }


        }

    }
}
