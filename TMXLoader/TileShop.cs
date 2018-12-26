using System.Collections.Generic;

namespace TMXLoader
{
    public class TileShop
    {
        public string id { get; set; }
        public List<TileShopItem> inventory { get; set; } = new List<TileShopItem>();
    }
}
