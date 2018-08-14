using PyTK.CustomElementHandler;
using StardewValley;
using System.Collections.Generic;

namespace SplitMoney
{
    class GoldItem : PySObject
    {
        public GoldItem()
        {

        }

        public GoldItem(CustomObjectData data)
            : base(data)
        {
            this.data = data;
        }

        public override int salePrice()
        {
            return Stack;
        }

        public override Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> savData = new Dictionary<string, string>();
            savData.Add("id", CustomObjectData.collection["Platonymous.G"].id);
            savData.Add("stack", Stack.ToString());
            return savData;
        }

        public override string DisplayName { get => Stack + base.DisplayName.ToLower(); set => base.DisplayName = value; }

        public override Item getOne()
        {
            GoldItem item = new GoldItem(data);
            return item;
        }

        public override void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            data = CustomObjectData.collection[additionalSaveData["id"]];
            int s = Stack;
            if(additionalSaveData.ContainsKey("stack"))
            int.TryParse(additionalSaveData["stack"], out s);
            Stack = s;
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];
            return new GoldItem(CustomObjectData.collection[additionalSaveData["id"]]);
        }


        public override bool canStackWith(Item other)
        {
            bool result = base.canStackWith(other) && this.Name == other.Name;
            return result;
        }
    }
}
