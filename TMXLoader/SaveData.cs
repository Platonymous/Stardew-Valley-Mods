using System.Collections.Generic;

namespace TMXLoader
{
    public class SaveData
    {
        public List<SaveLocation> Locations = new List<SaveLocation>();
        public List<SaveBuildable> Buildables = new List<SaveBuildable>();
        public List<PersistentData> Data = new List<PersistentData>();
    }
}
