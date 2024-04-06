using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace Artista.Artpieces
{
    [XmlType("Mods_platonymous_Artista_SavedArtpiece")]
    public class SavedArtpiece
    {
        public string Name { get; set; }

        public string Author { get; set; }
        public int Width { get; set; }

        public int Height { get; set; }

        public int Depth { get; set; }

        public int ArtType { get; set; }
        public int Material { get; set; }

        public string CanvasColor { get; set; }

        public bool CanRotate { get; set; }

        public int Scale { get; set; }

        public string[] Colors { get; set; } = new string[0];

        public string OnlineId { get; set; } = "";

        public SavedArtpiece() { }

        public SavedArtpiece(string name, string author, int width , int height, int depth, int artType, int material, Color canvasColor, bool canRotate, int scale, Color[] colors, string onlineid)
        {
            OnlineId = onlineid;
            Name = name;
            Author = author;
            Width = width;
            Height = height;
            Depth = depth;
            ArtType = artType;
            Material = material;
            CanvasColor = ColorToString(canvasColor);
            CanRotate = canRotate;
            Scale = scale;
            Colors = colors.Select(c => ColorToString(c)).ToArray();
        }

        public string GetJsonData()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static SavedArtpiece FromJson(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SavedArtpiece>(json);
        }

        public static bool TryParseColorFromString(string value, out Color color)
        {
            var c = value.Split(',');

            if(c.Length == 4 ) {
                try
                {
                    color = new Color(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]), int.Parse(c[3]));
                    return true;
                }
                catch { 
                }
            }

            color = Color.Black;
            return false;
        }

        public static string ColorToString(Color color)
        {
            return $"{color.R},{color.G},{color.B},{color.A}";
        }
    }
}
