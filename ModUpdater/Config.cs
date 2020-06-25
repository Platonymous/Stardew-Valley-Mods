using System;
using System.Collections.Generic;

namespace ModUpdater
{
    public class Config
    {
        public bool AutoRestart { get; set; } = false;

        public string ExecutionArgs { get; set; } = "";

        public DateTime LastUpdateCheck { get; set; } = new DateTime();

        public int Interval { get; set; } = 60;

        public bool LoadPrereleases { get; set; } = false;

        public string GitHubUser { get; set; } = "";

        public string GitHubPassword { get; set; } = "";

        public List<string> Exclude { get; set; } = new List<string>();
     }
}
