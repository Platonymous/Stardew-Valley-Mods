using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Sprinkles
{
    public class SprinklesMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (e.IsActionButton && Game1.currentLocation is GameLocation gl)
            {
                int tilesize = Game1.tileSize * Game1.pixelZoom;
                Vector2 p = new Vector2((int)(Game1.getOldMouseX() + Game1.viewport.X) / Game1.tileSize, (int)(Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize);
                if (gl.objects.ContainsKey(p) && gl.objects[p].name.Contains("Sprinkler"))
                    if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                        gl.objects[p].DayUpdate(Game1.currentLocation);
                    else
                        foreach (SObject v in gl.objects.Values)
                            if (v.name.Contains("Sprinkler"))
                                v.DayUpdate(gl);
            }
        }
    }
}
