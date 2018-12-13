using StardewModdingAPI;

namespace RingOfFire
{
    class ROFConfig
    {

        public SButton actionKey { get; set; }
        public int price { get; set; }

        public ROFConfig()
        {
            actionKey = SButton.Space;
            price = 50000;
        }
    }
}
