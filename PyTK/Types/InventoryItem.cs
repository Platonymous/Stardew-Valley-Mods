using StardewValley;

namespace PyTK.Types
{
    /// <summary>An object that contains the price and stock of an item.</summary>
    public class InventoryItem
    {
        public Item item;
        public int price;
        public int stock;

        public InventoryItem(Item item, int price, int stock = int.MaxValue)
        {
            this.item = item;
            this.price = price;
            this.stock = stock;
        }

    }
}
