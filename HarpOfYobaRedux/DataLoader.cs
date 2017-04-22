using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using StardewModdingAPI;
using StardewValley;


namespace HarpOfYobaRedux
{
    internal class DataLoader
    {

        private static IModHelper Helper;

        public DataLoader(IModHelper h)
        {
            Helper = h;
        }

        public static void load()
        {
            Texture2D texture = loadTexture("tilesheet.png");
            new Instrument("harp", texture, "Harp of Yoba", "Add Sheet Music to play.",new HarpAnimation());

            new SheetMusic("thunder", texture, "Serenade of Thunder", "Rain on me", Microsoft.Xna.Framework.Color.Blue, "AbigailFluteDuet", 10000, new RainMagic());
            new SheetMusic("birthday", texture, "Birthday Sonata", "Popular on birthdays", Microsoft.Xna.Framework.Color.DarkBlue, "shimmeringbastion", 10000, new BirthdayMagic());
            new SheetMusic("wanderer", texture, "Ballad of the Wanderer", "Wander off and return", Microsoft.Xna.Framework.Color.Orange, "honkytonky", 10000, new TeleportMagic());
            new SheetMusic("yoba", texture, "Prelude to Yoba", "Can you hear the trees sing along", Microsoft.Xna.Framework.Color.ForestGreen, "wedding", 10000, new TreeMagic());
            new SheetMusic("fisher", texture, "The Fisherman's Lament", "The old mariners lucky melody", Microsoft.Xna.Framework.Color.DarkMagenta, "poppy", 10000, new FisherMagic());
            new SheetMusic("dark", texture, "Ode to the Dark", "All monsters are created equal", Microsoft.Xna.Framework.Color.Red, "tribal", 10000, new MonsterMagic());
            new SheetMusic("animals", texture, "Animals' Aria", "Beloved by Farmanimals", Microsoft.Xna.Framework.Color.Brown, "tinymusicbox", 10000, new AnimalMagic());
            new SheetMusic("adventure", texture, "Adventurer's Allegro", "An energizing tune", Microsoft.Xna.Framework.Color.LightCoral, "aerobics", 10000, new BoosterMagic());
            new SheetMusic("granpa", texture, "Farmer's Lullaby", "Stand on fertile ground", Microsoft.Xna.Framework.Color.Magenta, "grandpas_theme", 10000, new SeedMagic());
            new SheetMusic("time", texture, "Rondo of Time", "Play ahead to pass the time", Microsoft.Xna.Framework.Color.LightCyan, "50s", 10000, new TimeMagic());

        }

        public static string getLetter(string id)
        {

            Dictionary<string,string> letters = new Dictionary<string, string>();
            letters.Add("birthday", "Dear " + Game1.player.name + ",^  I hope you are doing well. Your Grandpa would have wanted me to give you his old Harp. Maybe you can play for him from time to time. I didn't get to play it much, since you left.^  Love, Dad  ^  P.S. I wrote the notes to your favorite birthday tune on the back.");
            letters.Add("dark", "Greetings, young adept.^I have enclosed in this package an item of arcane significance. Use it wisely.   ^   -M. Rasmodius, Wizard");
            letters.Add("yoba", "Dear " + Game1.player.name + ",^  Congratulations to your wedding. I wish we could have been there, but you and " + Game1.player.spouse + " have to visit us soon.^  Love, Dad  ^  P.S. Did you play our family wedding song during the ceremony?");
            letters.Add("thunder","Hey " + Game1.player.name + ",^ I loved playing with you in the rain. We should do that again some time. I wrote the notes to our song on the back of this letter. See you soon!   ^   -Abigail");
            letters.Add("wanderer","Dear " + Game1.player.name + ",^Thank you for rebuilding our community center and for becoming such a valuable part of our little town! ^   -Mayor Lewis  ^  P.S. We found this inside the community vault, is it one of your songs?");
            letters.Add("fisher", "Thank you " + Game1.player.name + " for playing all those melodies to an old fisherman.   ^   ");
            letters.Add("animals", Game1.player.name + ",^ I wrote a song for your animals. I hope they like it.  ^   -Haley");
            letters.Add("adventure", "You killed more than 100 Monsters, well done! Here's the song of our guild. Play it with pride.  ^   -Marlon");
            letters.Add("granpa", "It's an empty letter with notes scribbled on the back.  ^   ");
            letters.Add("time", "Dear " + Game1.player.name + ",^Thank you for listening to an old fool like me. I found the melody of one of the songs we used to sing in the mines to pass the time. Sadly I can't play it anymore.   ^   -George  ^  ");

            return letters[id];
        }

        private static Texture2D loadTexture(string file)
        {
            string path = Path.Combine(Helper.DirectoryPath,"Assets",file);
            Image textureImage = Image.FromFile(path);
            Texture2D texture = Bitmap2Texture(new Bitmap(textureImage));
            return texture;
        }
        
        private static Texture2D Bitmap2Texture(Bitmap bmp)
        {
            MemoryStream s = new MemoryStream();

            bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            s.Seek(0, SeekOrigin.Begin);
            Texture2D tx = Texture2D.FromStream(Game1.graphics.GraphicsDevice, s);

            return tx;

        }

    }
}
