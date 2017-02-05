using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

using Microsoft.Xna.Framework.Graphics;


namespace TheHarpOfYoba
{
    class LoadData
    {
        public string name = "HarpOfYoba_";
        public string tmp;

        public LoadData()
        {

        }


        public bool doesSavFileExist(ulong GID, string PN)
        {
            this.tmp = this.name+PN + "_" + GID + ".sav";
            string str = PN;
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c))
                    str = str.Replace(c.ToString() ?? "", "");
            }
            string path2 = Path.Combine(str + "_" + (object)GID, this.tmp);
            FileInfo fileInfo1 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), path2));
            if (!fileInfo1.Exists)
            {
                return false;
            }else
            {
                return true;
            }

           

        }

        public string loadSavStringFromFile(ulong GID, string PN)
        {
            this.tmp = this.name + PN + "_" + GID + ".sav";
            FileInfo fi = ensureFolderStructureExists(PN, GID, this.tmp);

                using (StreamReader sr = fi.OpenText())
                {
                    return sr.ReadToEnd();

                }
            
           
        }

        public string saveSavStringToFile(string savstring, ulong GID, string PN)
        {
            this.tmp = this.name + PN + "_" + GID + ".sav";
            FileInfo fi = ensureFolderStructureExists(PN, GID, this.tmp);

            
                using (StreamWriter sw = fi.CreateText())
                {
                    sw.WriteLine(savstring);
                }
          


            return savstring;

        }


        public static FileInfo ensureFolderStructureExists(string PN, ulong GID, string tmpString)
        {
            string str = PN;
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c))
                    str = str.Replace(c.ToString() ?? "", "");
            }
            string path2 = Path.Combine(str + "_" + (object)GID,tmpString);
            FileInfo fileInfo1 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), path2));
            if (!fileInfo1.Directory.Exists)
                fileInfo1.Directory.Create();

            return fileInfo1;
        }


        public Texture2D getSprite(int i)
        {
            String src = "";
            if (i == 0)
            {
                src = "iVBORw0KGgoAAAANSUhEUgAAAFAAAAAQCAYAAACBSfjBAAAB00lEQVR42u2Xv0oDQRDGA4IgBF/DV7AUESF5B0sbC1GUxBewshDstEidxkrIM+QFBDtBEBEbEcRyvW+5L0w2583O7kUUbuCYRe/LzPz2z812Oq219mfsoLft5GPVXw6OnXz+m36zu+LkkwRwsrfrPi/63oeJaInhf9PzU/c1HnofToQ2MaE+N75VD2ijjTX3sb/ufQhSBYviAI+FIgmZCMd1BSB5vo9JwMPf4zhWnxvfqgccwCMoQJQgOVYBYjy5uZoLjMSYnFYAxtO7Wx9MrmpOTqw+N75VT4AYj84O58Dh74T7I8DrwcksgffXl4VEtASq9JgIrkANYJPx354ezfp+d3UG8PnhfgFkFEDMlixAFhJTQKhnIQAZAzBHP+zt+NUuAVjyB0CsNqmXILMAwmtnyDL0BAivnaF1AGPi1wGEV89AeV5UASjsKPYr2JReAixsqw6gPG+ps8SX510VQL6j9lIhAF9YuQW4Qn5LDxAeTLmFucK0D6EEb4nPs06C82DLLcwVqkJEQN/KFD7srWL19Dl6gJO9aUyDT+Cp8QmMPuwN4wCKT78leNN6tj+x8ORWTo0fti4meFVXIuvVqEl9ytWS76fGD690yVe71lpbun0DCmeBtWGRHIEAAAAASUVORK5CYII=";
                
            }

            if (i == 1)
            {
                src = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAABJUlEQVR42u3a2Q3DIAyAYYbsXp0hS3WC7kGkSpWiqE04jA/4kXgufOaym5QO7fV85JV6OjcAAFgQIL+3/O1p8nYG+MwZAAAAAAAAAAAAAAAAADABOP7+XQ8LcDexkswtBEDvJKcAGJW7AwBA/xhDA0isghAAVwdnCICaa+3qvg8JcDXw2ofNdACSkMsASK8C8y1ghakOMHobuE+GAHB6EIYEkFwF5gDW20AVwOM5oFoRahl4aTFlOgCt8tgwgN7I1WaRreMVB5CIXGvN0A1Ab+RaawjmAFKRCw0gMXAAANjUKsRTAZi/A6QiV3uYuskGJSNX8p6QeLCZXoOlleBQf49rRU6qqecC3r484RMZAAAAAAAAAAAAAAD+AMzefz3hAQBgcYAd7WFWYBxJQjEAAAAASUVORK5CYII=";
            }

            if (i == 2)
            {
                src = "iVBORw0KGgoAAAANSUhEUgAAAGAAAAAQCAYAAADpunr5AAABtElEQVR42u2YP0sDQRDFD/zTRUih5BNYW9hd4SewtrGzjR/Awt7OVquAAcEmnXaCYC9i59ewE5TTd2SG5zm7zu4WNnvweLnL/YZj7s0mbNN8H0d7K93ZydQt3A81y6PyBTw+fFxPuu79zSUUwP2fD4daqPL5vL6A1+d7l6QAxA9Q+Ty+aITwufLlfH/gZDE/TxIXqHwBL7B3jIYFKl/G9wXwhfeHBJL1rPLlfF8AF29vLrut1ZEK55asBxD+bnNHlcOPZ/uqHH50cKrK4buLDVUOf7W2UHl5LQCh8eyxv1O8/sl1NJ49lUfj2VN5NJ49le+bz57Io/HsHv7XBLC3bauStxd6AJkA9pfJrsrDS/LZeSL+4iX54uPjmcrDS/LV59sqDy/JF39cf1KZPK9fVvrRePbY+melH41nj/FW+q1rId5KPxrPHl2/rfSj+ewR3ko/Gs8eXX6s9UqSzxMQGj+Ll+TzBKTww4lI5pfJ5wlI4TX5NAEpvCSfJ8BcfiyF0s97GTE+lH4vH0q/mw+k38uH0u/lQ+n/sReU+hdquJdR+TK+4SK5eyGVn/7PXlDdTi7nvwCWWKHZXnQJOQAAAABJRU5ErkJggg==";
            }

            Image textureImg = this.LoadImage(src);
            Bitmap textureBtm = (Bitmap)textureImg;
            Texture2D texture = this.Bitmap2Texture(textureBtm);
            return texture;
        }

        private Image LoadImage(String imageString)
        {

            byte[] bytes = Convert.FromBase64String(imageString);

            Image image;

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }

            return image;
        }

        private Texture2D Bitmap2Texture(Bitmap bmp)
        {

            MemoryStream s = new MemoryStream();

            bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            s.Seek(0, SeekOrigin.Begin);
            Texture2D tx = Texture2D.FromStream(StardewValley.Game1.graphics.GraphicsDevice, s);

            return tx;

        }

        public string getLetter(int i)
        {

            List<string> letters = new List<string>();
            letters.Add("Dear @,^  I hope you are doing well. Your Grandpa would have wanted me to give you his old Harp. Maybe you can play for him from time to time. I didn't get to play it much, since you left.^  Love, Dad  ^  P.S. I wrote the notes to your favorite birthday tune on the back.");
            letters.Add("Greetings, young adept.^I have enclosed in this package an item of arcane significance. Use it wisely.   ^   -M. Rasmodius, Wizard");
            letters.Add("Dear @,^  Congratulations to your wedding. I wish we could have been there, but you and !! have to visit us soon.^  Love, Dad  ^  P.S. Did you play our family wedding song during the ceremony?");
            letters.Add("Hey @,^ I loved playing with you in the rain. We should do that again some time. I wrote the notes to our song on the back of this Letter. See you soon!   ^   -Abigail");
            letters.Add("Dear @,^Thank you for rebuilding our community center and for becoming such a valuable part of our little Town! ^   -Mayor Lewis  ^  P.S. We found this inside the community vault, is it one of your songs?");
            letters.Add("Thank you @ for playing all those melodies for an old Fisherman.   ^");

            return letters[i];
        }

    }
}
