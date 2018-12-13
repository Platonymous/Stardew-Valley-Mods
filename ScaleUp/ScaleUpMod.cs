using System;
using StardewModdingAPI;
using PyTK.Extensions;
using System.IO;

namespace ScaleUp
{
    public class ScaleUpMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            loadContentPacks();
            helper.Content.AssetEditors.Add(new Scaler(helper));
        }

        private void loadContentPacks()
        {
            var directory = new DirectoryInfo(Helper.DirectoryPath).Parent;
            string[] files = Directory.GetFiles(directory.FullName, "manifest.json", SearchOption.AllDirectories);

            foreach(string fullPath in files)
            {
                FileInfo manifestFile = new FileInfo(fullPath);
                IContentPack contentPack = this.Helper.CreateTemporaryContentPack(manifestFile.Directory.FullName, Guid.NewGuid().ToString("N"), "temp pack", null, null, new SemanticVersion(1, 0, 0));
                Manifest manifest = contentPack.ReadJsonFile<Manifest>(manifestFile.Name);

                if (manifest != null && manifest.ContentPackFor is ManifestContentPackFor m && m.UniqueID == "Pathoschild.ContentPatcher")
                {
                    Content content = contentPack.ReadJsonFile<Content>("content.json");
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
