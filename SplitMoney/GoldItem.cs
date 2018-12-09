using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using StardewValley;
using System;
using System.Collections.Generic;

namespace SplitMoney
{
    class GoldItem : PySObject
    {
        public bool forSaving
        {
            get
            {
                return Flipped;
            }
            set
            {
                Flipped = value;
            }
        }

        public GoldItem()
        {
        }

        public GoldItem(CustomObjectData data)
            : base(data)
        {
            this.data = data;
            this.Flipped = false;
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
            savData.Add("forSaving", forSaving.ToString().ToLower());
            return savData;
        }

        public override string DisplayName { get => (forSaving ? Stack - 1 : Stack) + base.DisplayName.ToLower() + (forSaving ? " (saved)" : ""); set => base.DisplayName = value; }

        public override Item getOne()
        {
            GoldItem item = new GoldItem(data);
            item.forSaving = forSaving;
            return item;
        }
        
        public override void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            data = CustomObjectData.collection[additionalSaveData["id"]];
            int s = Stack;
            if(additionalSaveData.ContainsKey("stack"))
            int.TryParse(additionalSaveData["stack"], out s);
            if (additionalSaveData.ContainsKey("forSaving"))
                forSaving = additionalSaveData["forSaving"] == "true";
            Stack = s;

            SaveHandler.FinishedRebuilding += removeAfterRebuild;
        }

        public void removeAfterRebuild(object sender, EventArgs e)
        {
            if (forSaving && Game1.player.items.Contains(this))
            {
                if (SplitMoneyMod.myMoney == -1)
                    SplitMoneyMod.myMoney = Stack - 1;
                Game1.player.items.Remove(this);
            }

            SaveHandler.FinishedRebuilding -= removeAfterRebuild;
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];
            return new GoldItem(CustomObjectData.collection[additionalSaveData["id"]]);
        }


        public override bool canStackWith(Item other)
        {
            if (forSaving || (other is GoldItem g && g.forSaving))
                return false;

            bool result = base.canStackWith(other) && this.Name == other.Name;
            return result;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            if (forSaving && Game1.player.items.Contains(this))
            {
                if (SplitMoneyMod.myMoney == -1)
                    SplitMoneyMod.myMoney = Stack - 1;
                Game1.player.items.Remove(this);
            }

            base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
        }
    }
}
