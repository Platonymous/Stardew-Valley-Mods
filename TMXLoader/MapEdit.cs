using xTile;

namespace TMXLoader
{
    internal class MapEdit
    {
        public string name { get; set; } = "none";
        public string file { get; set; } = "none";
        public int[] sourceArea { get; set; } = new int[] { };
        public int[] position { get; set; } = new int[] { 0, 0 };
        public string[] addWarps { get; set; } = new string[] { };
        public string[] removeWarps { get; set; } = new string[] { };
        public bool retainWarps { get; set; } = false;
        public bool removeEmpty { get; set; } = true;
        public string info { get; set; } = "";

        public bool isGreenhouse { get; set; } = false;
        public string type { get; set; } = "Location";

        internal Map _map = null;
    }
}
