using System.Collections.Generic;

namespace ModUpdater
{
    public class ModUpdateManifest
    {
        public string Name { get; set; } = "";
        public string Author { get; set; } = "";
        public string UniqueID { get; set; } = "";
        public string Version { get; set; } = "0.0.0";
        public string MinimumApiVersion { get; set; } = "0.0.0";

        public string EntryDll { get; set; } = "";

        public ModUpdateInformation ModUpdater { get; set; } = new ModUpdateInformation();
    }

    public class ModUpdateInformation
    {
        public string Repository { get; set; } = "";
        public string User { get; set; } = "";
        public string Directory { get; set; } = "";
        public string FileSelector { get; set; } = "{ModFolder} ([0-9a-zA-Z.-]+)";
        public string ModFolder { get; set; } = "";
        public bool Install { get; set; } = false;

        public string Branch { get; set; } = "";

        public List<string> DoNotReplace { get; set; } = new List<string>() { "config.json", "console.lua" };

        public List<string> DeleteFolders { get; set; } = new List<string>();

        public List<string> DeleteFiles { get; set; } = new List<string>();
    }

    public class PyModUpdateInformation : ModUpdateInformation
    {
        public PyModUpdateInformation(string modFolder)
        {
            Repository = "Stardew-Valley-Mods";
            User = "Platonymous";
            Directory = "_releases";
            ModFolder = modFolder;
        }
    }
}
