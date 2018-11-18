namespace TMXLoader
{
    internal class NPCPlacement
    {
        public string name { get; set; } = "none";
        public string map { get; set; } = "none";
        public int[] position { get; set; } = new int[] { 0, 0 };
        public int direction { get; set; } = 0;
    }
}
