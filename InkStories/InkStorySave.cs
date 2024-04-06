using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkStories
{
    public class InkStorySave
    {
        public List<InkStorySaveData> Data { get; set; } = new List<InkStorySaveData>();
        public Dictionary<string, List<string>> InksForNextDay { get; set; }
    }
}
