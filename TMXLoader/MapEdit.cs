using StardewModdingAPI;
using xTile;

namespace TMXLoader
{
    public class MapEdit
    {
        public string name { get; set; } = "none";

        public bool addLocation { get; set; } = true;
        public string file { get; set; } = "none";
        public int[] sourceArea { get; set; } = new int[] { };
        public int[] position { get; set; } = new int[] { 0, 0 };
        public string[] addWarps { get; set; } = new string[] { };
        public string[] removeWarps { get; set; } = new string[] { };
        public bool retainWarps { get; set; } = false;
        public bool removeEmpty { get; set; } = true;
        public string info { get; set; } = "";
        public string type { get; set; } = "Location";

        public string conditions { get; set; } = "";

        internal Map _map = null;
        internal IContentPack _pack = null;

        public override bool Equals(object obj)
        {
            return obj is MapEdit me && me.file == file && name == "name" && conditions == me.conditions && position == me.position;
        }

        public override int GetHashCode()
        {
            return (file + ":" + name + ":" + conditions + ":" + position[0] + ":" +position[1]).GetHashCode();
        }
    }
}
