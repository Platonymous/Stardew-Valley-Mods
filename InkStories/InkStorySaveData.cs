using System.Collections.Generic;

namespace InkStories
{
    public class InkStorySaveData
    {
        public string Id { get; set; }
        public string LastState { get; set; }
        public SharedStoryData SharedData { get; set; } = new SharedStoryData();
    }
}
