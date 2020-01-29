using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMusic
{
    public class GMCMOption
    {
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public int ActiveIndex { get; set; } = 0;

        public int DefaultIndex { get; set; } = 0;

        public List<string> Choices { get; set; } = new List<string>();

        public GMCMOption(string name, List<string> choices, string description = "", int activeIndex = 0, int defaultIndex = 0)
        {
            Name = name;
            Description = description;
            ActiveIndex = activeIndex;
            DefaultIndex = defaultIndex;
            Choices = choices;
        }

    }

    public class GMCMLabel : GMCMOption
    {
        public GMCMLabel(string name, string description = "")
            : base(name,null,description)
        {
            Name = name;
            Description = description;
        }
    }
}
