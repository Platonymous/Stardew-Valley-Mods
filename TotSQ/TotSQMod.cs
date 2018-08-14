using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using PyTK.Extensions;
using PyTK.CustomElementHandler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using PyTK.Types;

namespace TotSQ
{
    public class TotSQMod : Mod
    {
        internal Config config;
        internal ITranslationHelper i18n => Helper.Translation;
        internal static IModHelper _helper;
        internal static IMonitor _monitor;

        public override void Entry(IModHelper helper)
        {
            _monitor = Monitor;
            _helper = helper;
            var spider = Helper.Content.Load<Texture2D>(@"assets/moving.png");
            var spider16 = Helper.Content.Load<Texture2D>(@"assets/moving16.png");
            var spider32 = Helper.Content.Load<Texture2D>(@"assets/moving32.png");
            var spiderScaled = ScaledTexture2D.FromTexture(spider16, spider, 6);
            Spider.Texture = spiderScaled;
            spider16.inject(@"Characters/Monsters/Spider");

            new Dictionary<string, string>() { { "Spider", "24/5/0/0/false/1000/766 .75 766 .05 153 .1 66 .015 92 .15 96 .005 99 .001/1/.01/4/2/.00/true/3" } }.injectInto(@"Data/Monsters");

            Keys.L.onPressed(() =>
            {
                if (!Context.IsWorldReady)
                    return;
                Vector2 pos = Game1.player.getTileLocation() + new Vector2(-2, 0);
                Game1.currentLocation.addCharacterAtRandomLocation(new Spider());

            });
        }
    }
}
