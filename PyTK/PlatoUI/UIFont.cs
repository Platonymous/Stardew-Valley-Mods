using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;

namespace PyTK.PlatoUI
{
    public class UIFont
    {
        public virtual string Id { get; set; } = "";
        public virtual FontFile FontFile { get; set; } = null;

        public virtual Dictionary<char, FontChar> CharacterMap { get; set; } = null;

        public virtual List<Texture2D> FontPages { get; set; } = null;

        public UIFont(IModHelper helper, string assetName, string id = "")
        {
            if (id == "")
                id = assetName;

            Id = id;

            FontFile = FontLoader.Parse(File.ReadAllText(Path.Combine(helper.DirectoryPath, assetName)));

            CharacterMap = new Dictionary<char, FontChar>();

            foreach (FontChar fontChar in FontFile.Chars)
            {
                char cid = (char)fontChar.ID;
                CharacterMap.Add(cid, fontChar);
            }

            FontPages = new List<Texture2D>();

            foreach (FontPage page in FontFile.Pages)
                FontPages.Add(helper.ModContent.Load<Texture2D>($"{Path.GetDirectoryName(assetName)}/{page.File}"));
        }
    }
}
