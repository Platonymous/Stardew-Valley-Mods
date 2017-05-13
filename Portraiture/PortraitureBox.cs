using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace Portraiture
{
    class PortraitureBox : DialogueBox
    {

        private Dialogue characterDialogue;
        public static float displayAlpha;

        public PortraitureBox (Dialogue dialogue)
            : base(dialogue)
        {
            displayAlpha = 0;
            characterDialogue = dialogue;
        }

        private Texture2D getTexture(string name)
        {
            return TextureLoader.getPortrait(name);
        }

        private void drawPortraiture(SpriteBatch b)
        {
            if(width < 107 * Game1.pixelZoom * 3 / 2 || PortraitureMod.helper.Reflection.GetPrivateValue<bool>(this, "transitioning") || PortraitureMod.helper.Reflection.GetPrivateValue<bool>(this, "isQuestion"))
            {
                return;
            }

            Texture2D texture = getTexture(characterDialogue.speaker.name);

            int textureSize = Math.Max(texture.Width / 2, 64);
         
            int x = PortraitureMod.helper.Reflection.GetPrivateValue<int>(this, "x");
            int y = PortraitureMod.helper.Reflection.GetPrivateValue<int>(this, "y");

            int num1 = x + width - 112 * Game1.pixelZoom + Game1.pixelZoom;
            int num2 = x + width - num1;
            int num3 = num1 + Game1.pixelZoom * 19;
            int num4 = y + height / 2 - 74 * Game1.pixelZoom / 2 - 18 * Game1.pixelZoom / 2;
            
            int num5 = PortraitureMod.helper.Reflection.GetPrivateMethod(this, "shouldPortraitShake").Invoke<bool>(new object[] { characterDialogue }) ? Game1.random.Next(-1, 2) : 0;

            Rectangle rectangle = Game1.getSourceRectForStandardTileSheet(texture, characterDialogue.getPortraitIndex(), textureSize, textureSize);

            if (!texture.Bounds.Contains(rectangle))
            {
                rectangle = new Rectangle(0, 0, textureSize, textureSize);
            }


            b.Draw(texture, new Rectangle(num3 + 4 * Game1.pixelZoom + num5, num4 + 6 * Game1.pixelZoom, 64 * Game1.pixelZoom, 64 * Game1.pixelZoom), new Rectangle?(rectangle), Color.White);

       
            string activeFolderText = TextureLoader.getFolderName();

            activeFolderText = activeFolderText.Replace('_', ' ');

            if(textureSize > 64)
            {
                activeFolderText = "= " + activeFolderText;
            }
         
            int textlength = (int)Game1.smallFont.MeasureString(activeFolderText).X;
            int textheight = (int)Game1.smallFont.MeasureString(activeFolderText).Y;
            int padding = Game1.pixelZoom * 12;
            int displayBoxWidth = (int)textlength + padding;
            int displayBoxHeight = (int)textheight + padding / 2;
          
            Vector2 boxPos = new Vector2(x, y);
            Vector2 displayBoxPos = new Vector2(boxPos.X, boxPos.Y - (displayBoxHeight + padding));

            drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), (int)displayBoxPos.X, (int)displayBoxPos.Y, displayBoxWidth, displayBoxHeight, Color.White * displayAlpha, 1f, false);
            if (displayAlpha >= 0.8)
            {
                Utility.drawTextWithShadow(b, activeFolderText, Game1.smallFont, new Vector2(displayBoxPos.X + ((displayBoxWidth - textlength) / 2), Game1.pixelZoom + displayBoxPos.Y + ((displayBoxHeight - textheight) / 2)), Game1.textColor);
            }
          
          }

        public override void draw(SpriteBatch b)
        {
            characterDialogue.speaker.Portrait = TextureLoader.getEmptyPortrait();
            
            base.draw(b);
            
            drawPortraiture(b);

        }

    }
}
