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

            foreach(string file in files)
            {
                var manifestFile = Helper.ReadJsonFile<Manifest>(file);

                if(manifestFile is Manifest manifest && manifest.ContentPackFor is ManifestContentPackFor m && m.UniqueID == "Pathoschild.ContentPatcher")
                {
                    string contentFile = Path.Combine(Path.GetDirectoryName(file), "content.json");
                    if (!File.Exists(contentFile))
                        continue;

                    var content = Helper.ReadJsonFile<Content>(contentFile);

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
