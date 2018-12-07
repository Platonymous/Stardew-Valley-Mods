using Microsoft.Xna.Framework.Input;

namespace ATM
{
    class Config
    {
        public string Map { get; set; } = "Town";
        public int[] Position { get; set; } = new int[] { 32, 55 };
        public bool Credit { get; set; } = true;
        public string CreditLine { get; set; } = "value / 2";
        public float CreditInterest { get; set; } = 0.05f;
        public float GainInterest { get; set; } = 0.01f;
    }
}
