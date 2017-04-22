using StardewValley;
using System.Collections.Generic;


namespace HarpOfYobaRedux
{
    internal class Letter
    {
        public string text;
        public List<Item> items;
        public string letterID;


        public Letter()
        {

        }

        public Letter(string text, List<Item> items)
        {
            this.letterID = items[0].Name;
            this.text = text;
            this.items = items;
        }

    }
}
