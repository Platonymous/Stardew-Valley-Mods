using StardewValley;

namespace SplitMoney
{
    public class SplitMoneyAPI
    {
        public void SendMoney(Farmer farmer, int amount)
        {
            SplitMoneyMod.sendMoney(farmer, amount);
        }
    }
}
