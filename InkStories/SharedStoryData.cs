using System.Collections.Generic;

namespace InkStories
{
    public class SharedStoryData
    {
        public string Id { get; set; }
        public List<SharedStoryDataEntry> Data { get; set; } = new List<SharedStoryDataEntry>();
        public List<SharedStoryNumberEntry> Numbers { get; set; } = new List<SharedStoryNumberEntry>();
    }
}
