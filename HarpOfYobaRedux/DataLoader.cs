using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using StardewModdingAPI;
using PyTK.Extensions;
using PyTK.CustomElementHandler;
using Microsoft.Xna.Framework;

namespace HarpOfYobaRedux
{
    internal class DataLoader
    {
        internal static Dictionary<string, Letter> letters;

        public static void load(IModHelper helper, bool cm)
        {
            Instrument.allInstruments = new Dictionary<string, Instrument>();
            SheetMusic.allSheets = new Dictionary<string, SheetMusic>();
            Texture2D texture = loadTexture(helper, "tilesheet.png");
            List<SheetMusic> sheets = new List<SheetMusic>();

            var harpOfYoba = new Instrument("harp", texture, "Harp of Yoba", "Add Sheet Music to play.",new HarpAnimation());
            sheets.Add(new SheetMusic("thunder", texture, "Serenade of Thunder", "Rain on me", Microsoft.Xna.Framework.Color.Blue, !cm ? "AbigailFluteDuet" : "cm:HOY-SerenadeOfThunder:AbigailFluteDuet", 15000, new RainMagic()));
            sheets.Add(new SheetMusic("birthday", texture, "Birthday Sonata", "Popular on birthdays", Microsoft.Xna.Framework.Color.DarkBlue, !cm ? "shimmeringbastion" : "cm:HOY-BirthdaySonata:shimmeringbastion", 11000, new BirthdayMagic()));
            sheets.Add(new SheetMusic("wanderer", texture, "Ballad of the Wanderer", "Wander off and return", Microsoft.Xna.Framework.Color.Orange, !cm ? "honkytonky" : "cm:HOY-BalladOfTheWanderer:honkytonky", 12000, new TeleportMagic()));
            sheets.Add(new SheetMusic("yoba", texture, "Prelude to Yoba", "Can you hear the trees sing along", Microsoft.Xna.Framework.Color.ForestGreen, !cm ? "wedding" : "cm:HOY-PreludeToYoba:wedding", 14000, new TreeMagic()));
            sheets.Add(new SheetMusic("fisher", texture, "The Fisherman's Lament", "The old mariners lucky melody", Microsoft.Xna.Framework.Color.DarkMagenta, !cm ? "poppy" : "cm:HOY-FishermentsLament:poppy", 10000, new FisherMagic()));
            sheets.Add(new SheetMusic("dark", texture, "Ode to the Dark", "All monsters are created equal", Microsoft.Xna.Framework.Color.Red, !cm ? "tribal" : "cm:HOY-OdeToTheDark:tribal", 10000, new MonsterMagic()));
            sheets.Add(new SheetMusic("animals", texture, "Animals' Aria", "Beloved by Farmanimals", Microsoft.Xna.Framework.Color.Brown, !cm ? "tinymusicbox" : "cm:HOY-AnimalsAria:tinymusicbox", 11000, new AnimalMagic()));
            sheets.Add(new SheetMusic("adventure", texture, "Adventurer's Allegro", "An energizing tune", Microsoft.Xna.Framework.Color.LightCoral, !cm ? "aerobics" : "cm:HOY-AdventurersAllegro:aerobics", 11000, new BoosterMagic()));
            sheets.Add(new SheetMusic("granpa", texture, "Farmer's Lullaby", "Stand on fertile ground", Microsoft.Xna.Framework.Color.Magenta, !cm ? "grandpas_theme" : "cm:HOY-FarmersLullaby:grandpas_theme", 12000, new SeedMagic()));
            sheets.Add(new SheetMusic("time", texture, "Rondo of Time", "Play ahead to pass the time", Microsoft.Xna.Framework.Color.LightCyan, !cm ? "50s" : "cm:HOY-RondoOfTime:50s", 30000, new TimeMagic()));
            sheets.Add(new SheetMusic("lua", texture, "Lua Crescendo", "Play lua", Microsoft.Xna.Framework.Color.BlueViolet, HarpOfYobaReduxMod.config.luamusic, 10000, new LuaMagic(helper)));

            Texture2D sheetTexture = texture.getArea(new Rectangle(0, 0, 16, 16));
            Texture2D harpTexture = texture.getArea(new Rectangle(32, 0, 16, 16));

            CustomObjectData.newObject("Platonymous.HarpOfYoba." + harpOfYoba.instrumentID, harpTexture, Color.White, "Harp of Yoba", "Add Sheet Music to play.",customType:typeof(Instrument));

            foreach(SheetMusic sheet in sheets)
                CustomObjectData.newObject("Platonymous.HarpOfYoba." + sheet.sheetMusicID, sheetTexture, Color.White, sheet.Name, sheet.getDescription(), customType: typeof(SheetMusic));

            loadLetters();
        }

        public static Dictionary<string, Letter> loadLetters()
        {
            letters = new Dictionary<string, Letter>();
            Instrument harp = new Instrument("harp");
            harp.attach(new SheetMusic("birthday"));
            letters.Add("hoy_birthday", new Letter("birthday", "Dear @,^  I hope you are doing well. Your Grandpa would have wanted me to give you his old Harp. Maybe you can play for him from time to time. I didn't get to play it much, since you left.^  Love, Dad  ^  P.S. I wrote the notes to your favorite birthday tune on the back.", harp));
            letters.Add("hoy_dark", new Letter("dark", "Greetings, young adept.^I have enclosed in this package an item of arcane significance. Use it wisely.   ^   -M. Rasmodius, Wizard"));
            letters.Add("hoy_yoba", new Letter("yoba", "Dear @,^  Congratulations to your wedding. I wish we could have been there, but you two have to visit us soon.^  Love, Dad  ^  P.S. Did you play our family wedding song during the ceremony?"));
            letters.Add("hoy_thunder", new Letter("thunder", "Hey @,^ I loved playing with you in the rain. We should do that again some time. I wrote the notes to our song on the back of this letter. See you soon!   ^   -Abigail"));
            letters.Add("hoy_wanderer", new Letter("wanderer", "Dear @,^Thank you for rebuilding our community center and for becoming such a valuable part of our little town! ^   -Mayor Lewis  ^  P.S. We found this inside the community vault, is it one of your songs?"));
            letters.Add("hoy_fisher", new Letter("fisher", "Thank you @ for playing all those melodies to an old fisherman.   ^   "));
            letters.Add("hoy_animals", new Letter("animals", "@,^ I wrote a song for your animals. I hope they like it.  ^   -Haley"));
            letters.Add("hoy_adventure", new Letter("adventure", "You killed more than 100 Monsters, well done! Here's the song of our guild. Play it with pride.  ^   -Marlon"));
            letters.Add("hoy_granpa", new Letter("granpa", "It's an empty letter with notes scribbled on the back.  ^   "));
            letters.Add("hoy_time", new Letter("time", "Dear @,^Thank you for listening to an old fool like me. I found the melody of one of the songs we used to sing in the mines to pass the time. Sadly I can't play it anymore.   ^   -George  ^  "));
            letters.Add("hoy_lua", new Letter("lua", "Huh, an empty piece of sheetpaper, could be used to write music for the harp.  ^   "));
            return letters;
        }

        public static Letter getLetter(string id)
        {
            return letters[id];
        }

        private static Texture2D loadTexture(IModHelper helper, string file)
        {
            return helper.Content.Load<Texture2D>($"assets/{file}");
        }

    }
}
