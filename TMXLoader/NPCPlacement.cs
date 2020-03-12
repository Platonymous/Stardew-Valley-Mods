namespace TMXLoader
{
    public class NPCPlacement
    {
        public string name { get; set; } = "none";
        public string map { get; set; } = "none";
        public int[] position { get; set; } = new int[] { 0, 0 };
        public int[] position2 { get; set; } = new int[] { -1, -1 };
        public int direction { get; set; } = 0;
        public int direction2 { get; set; } = 0;
        public bool datable { get; set; } = false;
        public string conditions { get; set; } = "";
    }
}
