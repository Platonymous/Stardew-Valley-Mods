using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFarming
{
    public class CustomCraftingPage : IClickableMenu
    {
        private string descriptionText = "";
        private string hoverText = "";
        private List<Dictionary<ClickableTextureComponent, CustomRecipe>> pagesOfCraftingRecipes = new List<Dictionary<ClickableTextureComponent, CustomRecipe>>();
        private string hoverTitle = "";
        public const int howManyRecipesFitOnPage = 40;
        private Item hoverItem;
        private Item lastCookingHover;
        private InventoryMenu inventory;
        private Item heldItem;
        private int currentCraftingPage;
        private CustomRecipe hoverRecipe;
        private ClickableTextureComponent upButton;
        private ClickableTextureComponent downButton;
        private bool cooking;
        public ClickableTextureComponent trashCan;
        public float trashCanLidRotation;

        public CustomCraftingPage(int x, int y, int width, int height, List<CustomRecipe> recipes, bool cooking = false)
          : base(x, y, width, height, false)
        {
            this.cooking = cooking;
            this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Game1.tileSize * 5 - Game1.tileSize / 4, false, (List<Item>)null, (InventoryMenu.highlightThisItem)null, -1, 3, 0, 0, true);
            this.inventory.showGrayedOutSlots = true;
            int num1 = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth - Game1.tileSize / 4;
            int y1 = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - Game1.tileSize / 4;
            int tileSize = Game1.tileSize;
            int num2 = 8;
            int num3 = 10;
            int num4 = -1;
            if (cooking)
                this.initializeUpperRightCloseButton();
            
            this.trashCan = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 4, this.yPositionOnScreen + height - Game1.tileSize * 3 - Game1.tileSize / 2 - IClickableMenu.borderWidth - 104, Game1.tileSize, 104), Game1.mouseCursors, new Rectangle(669, 261, 16, 26), (float)Game1.pixelZoom, false);
            List<string> stringList = new List<string>();
            
            int index1 = 0;
            foreach (CustomRecipe r in recipes)
            {
                CustomRecipe craftingRecipe;
                int index2;
                ClickableTextureComponent key1;
                bool flag;
                do
                {
                    ++num4;
                    if (num4 % 40 == 0)
                        this.pagesOfCraftingRecipes.Add(new Dictionary<ClickableTextureComponent, CustomRecipe>());
                    int num5 = num4 / num3 % (40 / num3);
                    craftingRecipe = recipes[index1];
                    int count = recipes.Count;
                    while (num5 == 40 / num3 - 1 && count > 0)
                    {
                        index1 = (index1 + 1) % recipes.Count;

                        craftingRecipe = recipes[index1];
                        if (count == 0)
                        {
                            num4 += 40 - num4 % 40;
                            num5 = num4 / num3 % (40 / num3);
                            this.pagesOfCraftingRecipes.Add(new Dictionary<ClickableTextureComponent, CustomRecipe>());
                        }
                    }
                    index2 = num4 / 40;
                    key1 = new ClickableTextureComponent("", new Rectangle(num1 + num4 % num3 * (Game1.tileSize + num2), y1 + num5 * (Game1.tileSize + 8), Game1.tileSize, Game1.tileSize * 2), (string)null, (craftingRecipe.item as Item).Name, craftingRecipe.item.Texture, craftingRecipe.item.SourceRectangle, (float)Game1.pixelZoom, false);
                    flag = false;
                    foreach (ClickableComponent key2 in this.pagesOfCraftingRecipes[index2].Keys)
                    {
                        if (key2.bounds.Intersects(key1.bounds))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                while (flag);
                this.pagesOfCraftingRecipes[index2].Add(key1, craftingRecipe);
                index1 = 0;
            }
            if (this.pagesOfCraftingRecipes.Count <= 1)
                return;
            this.upButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 12 + Game1.tileSize / 2, y1, Game1.tileSize, Game1.tileSize), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12, -1, -1), 0.8f, false);
            this.downButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 12 + Game1.tileSize / 2, y1 + Game1.tileSize * 3 + Game1.tileSize / 2, Game1.tileSize, Game1.tileSize), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11, -1, -1), 0.8f, false);
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (!key.Equals((object)Keys.Delete) || this.heldItem == null || !this.heldItem.canBeTrashed())
                return;
            if (this.heldItem is StardewValley.Object && Game1.player.specialItems.Contains((this.heldItem as StardewValley.Object).parentSheetIndex))
                Game1.player.specialItems.Remove((this.heldItem as StardewValley.Object).parentSheetIndex);
            this.heldItem = (Item)null;
            Game1.playSound("trashcan");
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentCraftingPage > 0)
            {
                this.currentCraftingPage = this.currentCraftingPage - 1;
                Game1.playSound("shwip");
            }
            else
            {
                if (direction >= 0 || this.currentCraftingPage >= this.pagesOfCraftingRecipes.Count - 1)
                    return;
                this.currentCraftingPage = this.currentCraftingPage + 1;
                Game1.playSound("shwip");
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, true);
            this.heldItem = this.inventory.leftClick(x, y, this.heldItem, true);
            if (this.upButton != null && this.upButton.containsPoint(x, y))
            {
                if (this.currentCraftingPage > 0)
                    Game1.playSound("coin");
                this.currentCraftingPage = Math.Max(0, this.currentCraftingPage - 1);
                this.upButton.scale = this.upButton.baseScale;
            }
            if (this.downButton != null && this.downButton.containsPoint(x, y))
            {
                if (this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
                    Game1.playSound("coin");
                this.currentCraftingPage = Math.Min(this.pagesOfCraftingRecipes.Count - 1, this.currentCraftingPage + 1);
                this.downButton.scale = this.downButton.baseScale;
            }
            foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                int num = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? 5 : 1;
                for (int index = 0; index < num; ++index)
                {
                    if (key.containsPoint(x, y) && this.pagesOfCraftingRecipes[this.currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(this.cooking ? Utility.getHomeOfFarmer(Game1.player).fridge.items : (List<Item>)null))
                        this.clickCraftingRecipe(key, index == 0);
                }
            }
            if (this.trashCan != null && this.trashCan.containsPoint(x, y) && (this.heldItem != null && this.heldItem.canBeTrashed()))
            {
                if (this.heldItem is StardewValley.Object && Game1.player.specialItems.Contains((this.heldItem as StardewValley.Object).parentSheetIndex))
                    Game1.player.specialItems.Remove((this.heldItem as StardewValley.Object).parentSheetIndex);
                this.heldItem = (Item)null;
                Game1.playSound("trashcan");
            }
            else
            {
                if (this.heldItem == null || this.isWithinBounds(x, y) || !this.heldItem.canBeTrashed())
                    return;
                Game1.playSound("throwDownITem");
                Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, (GameLocation)null);
                this.heldItem = (Item)null;
            }
        }

        private void clickCraftingRecipe(ClickableTextureComponent c, bool playSound = true)
        {
            Item obj = (Item) this.pagesOfCraftingRecipes[this.currentCraftingPage][c].item;
            Game1.player.checkForQuestComplete((NPC)null, -1, -1, obj, (string)null, 2, -1);
            if (this.heldItem == null)
            {
                this.pagesOfCraftingRecipes[this.currentCraftingPage][c].consumeIngredients();
                this.heldItem = obj;
                if (playSound)
                    Game1.playSound("coin");
            }
            else if (this.heldItem.Name.Equals(obj.Name) && this.heldItem.Stack + this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft - 1 < this.heldItem.maximumStackSize())
            {
                this.heldItem.Stack += this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft;
                this.pagesOfCraftingRecipes[this.currentCraftingPage][c].consumeIngredients();
                if (playSound)
                    Game1.playSound("coin");
            }
            if (!this.cooking && Game1.player.craftingRecipes.ContainsKey((this.pagesOfCraftingRecipes[this.currentCraftingPage][c].item as Item).Name))
            {
                SerializableDictionary<string, int> craftingRecipes = Game1.player.craftingRecipes;
                string name = (this.pagesOfCraftingRecipes[this.currentCraftingPage][c].item as Item).Name;
                craftingRecipes[name] = craftingRecipes[name] + this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft;
            }
            if (this.cooking)
                Game1.player.cookedRecipe(this.heldItem.parentSheetIndex);
            if (!this.cooking)
                Game1.stats.checkForCraftingAchievements();
            else
                Game1.stats.checkForCookingAchievements();
            if (!Game1.options.gamepadControls || this.heldItem == null || !Game1.player.couldInventoryAcceptThisItem(this.heldItem))
                return;
            Game1.player.addItemToInventoryBool(this.heldItem, false);
            this.heldItem = (Item)null;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.heldItem = this.inventory.rightClick(x, y, this.heldItem, true);
            foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                if (key.containsPoint(x, y)&& this.pagesOfCraftingRecipes[this.currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(this.cooking ? Utility.getHomeOfFarmer(Game1.player).fridge.items : (List<Item>)null))
                    this.clickCraftingRecipe(key, true);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.hoverTitle = "";
            this.descriptionText = "";
            this.hoverText = "";
            this.hoverRecipe = (CustomRecipe)null;
            this.hoverItem = this.inventory.hover(x, y, this.hoverItem);
            if (this.hoverItem != null)
            {
                this.hoverTitle = this.inventory.hoverTitle;
                this.hoverText = this.inventory.hoverText;
            }
            foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                if (key.containsPoint(x, y))
                {
                    
                        this.hoverRecipe = this.pagesOfCraftingRecipes[this.currentCraftingPage][key];
                        if (this.lastCookingHover == null || !this.lastCookingHover.Name.Equals((this.hoverRecipe.item as Item).Name))
                            this.lastCookingHover = (Item) this.hoverRecipe.item;
                        key.scale = Math.Min(key.scale + 0.02f, key.baseScale + 0.1f);
                }
                else
                    key.scale = Math.Max(key.scale - 0.02f, key.baseScale);
            }
            if (this.upButton != null)
            {
                if (this.upButton.containsPoint(x, y))
                    this.upButton.scale = Math.Min(this.upButton.scale + 0.02f, this.upButton.baseScale + 0.1f);
                else
                    this.upButton.scale = Math.Max(this.upButton.scale - 0.02f, this.upButton.baseScale);
            }
            if (this.downButton != null)
            {
                if (this.downButton.containsPoint(x, y))
                    this.downButton.scale = Math.Min(this.downButton.scale + 0.02f, this.downButton.baseScale + 0.1f);
                else
                    this.downButton.scale = Math.Max(this.downButton.scale - 0.02f, this.downButton.baseScale);
            }
            if (this.trashCan == null)
                return;
            if (this.trashCan.containsPoint(x, y))
            {
                if ((double)this.trashCanLidRotation <= 0.0)
                    Game1.playSound("trashcanlid");
                this.trashCanLidRotation = Math.Min(this.trashCanLidRotation + (float)Math.PI / 48f, 1.570796f);
            }
            else
                this.trashCanLidRotation = Math.Max(this.trashCanLidRotation - (float)Math.PI / 48f, 0.0f);
        }

        public override bool readyToClose()
        {
            return this.heldItem == null;
        }

        public override void draw(SpriteBatch b)
        {
            if (this.cooking)
                Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, (string)null, false);
            this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 * Game1.tileSize, false);
            this.inventory.draw(b);
            if (this.trashCan != null)
            {
                this.trashCan.draw(b);
                b.Draw(Game1.mouseCursors, new Vector2((float)(this.trashCan.bounds.X + 60), (float)(this.trashCan.bounds.Y + 40)), new Rectangle?(new Rectangle(686, 256, 18, 10)), Color.White, this.trashCanLidRotation, new Vector2(16f, 10f), (float)Game1.pixelZoom, SpriteEffects.None, 0.86f);
            }
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                if (key.hoverText.Equals("ghosted"))
                    key.draw(b, Color.Black * 0.35f, 0.89f);
                else if (!this.pagesOfCraftingRecipes[this.currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(this.cooking ? Utility.getHomeOfFarmer(Game1.player).fridge.items : (List<Item>)null))
                {
                    key.draw(b, Color.LightGray * 0.4f, 0.89f);
                }
                else
                {
                    key.draw(b);
                    if (this.pagesOfCraftingRecipes[this.currentCraftingPage][key].numberProducedPerCraft > 1)
                        NumberSprite.draw(this.pagesOfCraftingRecipes[this.currentCraftingPage][key].numberProducedPerCraft, b, new Vector2((float)(key.bounds.X + Game1.tileSize - 2), (float)(key.bounds.Y + Game1.tileSize - 2)), Color.Red, (float)(0.5 * ((double)key.scale / (double)Game1.pixelZoom)), 0.97f, 1f, 0, 0);
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            if (this.hoverItem != null)
                IClickableMenu.drawToolTip(b, this.hoverText, this.hoverTitle, this.hoverItem, this.heldItem != null, -1, 0, -1, -1, (CraftingRecipe)null, -1);
            else if (this.hoverText != null)
                IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, this.heldItem != null ? Game1.tileSize : 0, this.heldItem != null ? Game1.tileSize : 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
            if (this.heldItem != null)
                this.heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + Game1.tileSize / 4), (float)(Game1.getOldMouseY() + Game1.tileSize / 4)), 1f);
            base.draw(b);
            if (this.downButton != null && this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
                this.downButton.draw(b);
            if (this.upButton != null && this.currentCraftingPage > 0)
                this.upButton.draw(b);
            if (this.cooking)
                this.drawMouse(b);
            if (this.hoverRecipe == null)
                return;
            SpriteBatch b1 = b;
            string text = (this.hoverRecipe.item as Item).getDescription();
            SpriteFont smallFont = Game1.smallFont;
            int xOffset = this.heldItem != null ? Game1.tileSize * 3 / 4 : 0;
            int yOffset = this.heldItem != null ? Game1.tileSize * 3 / 4 : 0;
            int moneyAmountToDisplayAtBottom = -1;
            string name = (this.hoverRecipe.item as Item).Name;
            int healAmountToDisplay = -1;
            string[] buffIconsToDisplay = (string[])null;

            Item lastCookingHover = this.lastCookingHover;
            int currencySymbol = 0;
            int extraItemToShowIndex = -1;
            int extraItemToShowAmount = -1;
            int overrideX = -1;
            int overrideY = -1;
            double num = 1.0;
            CustomRecipe hoverRecipe = this.hoverRecipe;
            IClickableMenu.drawHoverText(b1, text, smallFont, xOffset, yOffset, moneyAmountToDisplayAtBottom, name, healAmountToDisplay, buffIconsToDisplay, lastCookingHover, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, overrideX, overrideY, (float)num);
        }
    }
}
