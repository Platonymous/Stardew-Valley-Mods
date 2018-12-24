namespace CustomitemTemplate
{
    public class Data
    {
        public string Id { get; set; }
        public string DataString { get; set; }
        public string Texture { get; set; }
        public int TileIndex { get; set; } = 0;
        public bool ScaleUp { get; set; } = false;
        public int OriginalWidth { get; set; } = 16;
        public bool BigCraftable { get; set; } = false;
        public string CraftingRecipe{ get; set; } = null;
        public string SoldBy { get; set; } = null;
        public int Price { get; set; } = 100;
    }
}
