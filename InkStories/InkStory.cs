using Ink;
using Ink.Runtime;
using StardewValley;
using System.Linq;

namespace InkStories
{
    public enum DataType
    {
        JSON,
        TEXT
    }

    public class InkStory
    {
        public string Id { get; }

        public NPC CurrentNPC { get; set; }
        public Story Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (Source == null)
                    {
                        Source = InkPatches.LoadPatched(Asset);
                        if(Source.StartsWith("{\"inkVersion"))
                            Type = DataType.JSON;
                    }
                    if (Type == DataType.TEXT)
                    {
                        var compiler = new Compiler(Source);
                        Source = compiler.Compile().ToJson();
                        Type = DataType.JSON;
                    }

                    _instance = new Story(Source);

                    if (!string.IsNullOrEmpty(LastState))
                        _instance.state.LoadJson(LastState);

                    InkExternals.AddExternalFunctions(this,_instance);
                }
                return _instance;
            }
        }

        public DataType Type { get; private set; } = DataType.JSON;

        private Story _instance;

        public bool Loaded => _instance != null;

        public string LastState { get; private set; }

        public string Source { get; private set; }

        public string Asset { get; private set; }

        public SharedStoryData SharedData { get; private set; }

        public InkStory(string id, string source, DataType type)
        {
            Id= id;
            Source = source;
            Type = type;
            SharedData = new SharedStoryData() { Id = id };
        }

        public InkStory(string id, string asset, string type)
        {
            Id = id;
            Source = null;
            Asset = asset;
            Type = type.ToUpper() == "JSON" ? DataType.JSON : DataType.TEXT;
            SharedData = new SharedStoryData() { Id = id };
        }

        public InkStorySaveData GetSaveData()
        {
            LastState = _instance?.state.ToJson();
            return new InkStorySaveData() { Id = Id, LastState = LastState, SharedData = SharedData };
        }

        public void LoadSaveData(InkStorySaveData saveData)
        {
            if (saveData.Id == Id)
            {
                LastState = saveData.LastState;
                SharedData = saveData.SharedData;
            }
        }

        public void Reset(bool total = false)
        {
            _instance?.ResetState();
            _instance = null;
            var newShared = new SharedStoryData() { Id = Id };
            if (!total)
            {
                newShared.Data.AddRange(SharedData.Data.Where(d => d.IsFixed));
                newShared.Numbers.AddRange(SharedData.Numbers.Where(d => d.IsFixed));
            }
            SharedData = newShared;
            LastState = null;
        }
    }
}
