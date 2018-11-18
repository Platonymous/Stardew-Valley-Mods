using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notes
{
    class NoteMenu : NamingMenu
    {
        public string Title { get; set; }

        public NoteMenu(doneNamingBehavior b, string title, string defaultName = null)
            :base(b, title,defaultName)
        {
            Title = title;
            textBox.Width = 800;
            textBox.X = (Game1.viewport.Width - textBox.Width) / 2;
            doneNamingButton.bounds = new Rectangle(textBox.X + textBox.Width + 32, doneNamingButton.bounds.Y, doneNamingButton.bounds.Width, doneNamingButton.bounds.Height);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, Title, Game1.viewport.Width / 2, Game1.viewport.Height / 2 - 128, Title, 1f, -1, 0, 0.88f, false);
            this.textBox.Draw(b, true);
            this.doneNamingButton.draw(b);
            this.drawMouse(b);
        }
    }
}
