using PyTK.Types;
using StardewValley;

namespace HarpOfYobaRedux
{
    internal class Letter : Mail
    {
        public Item item;

        public Letter()
        {

        }

        public Letter(string id, string text, Item item = null)
            :base(id,text,388)
        {
            if (item == null)
                item = new SheetMusic(id);

            this.id = "hoy_" + id;
            this.item = item;
            injectIntoMail();
        }

    }
}
