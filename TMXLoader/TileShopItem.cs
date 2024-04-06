namespace TMXLoader
{
    public class TileShopItem
    {
        public string index { get; set; } = "-1";
        public string type { get; set; } = "Object";
        public string name { get; set; } = "none";
        public int stock { get; set; } = int.MaxValue;

        public int stack { get => stock; set => stock = value; }
        public int price { get; set; } = -1;
        public string conditions { get; set; } = "";
    }
}
