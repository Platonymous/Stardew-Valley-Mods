
using Microsoft.Xna.Framework.Input;

namespace RingOfFire
{
    class ROFConfig
    {

        public Keys actionKey { get; set; }
        public int price { get; set; }

        public ROFConfig()
        {
            actionKey = Keys.Space;
            price = 50000;
        }

    }
}
