using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notes
{
    class Note : PySObject
    {
        public string text = "";

        public Note() : base() { }
        public Note(CustomObjectData data) : base(data) { }
        public Note(CustomObjectData data, Vector2 tileLocation) : base( data, tileLocation) { }

        public override Item getOne()
        {
            return new Note(data) { TileLocation = Vector2.Zero, name = name, Price = price, Quality = quality, text = text };
        }

        public override string getDescription()
        {
            return Game1.parseText(text, Game1.smallFont, 272);
        }

        public override bool canStackWith(Item other)
        {
            return false;
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            return new Note(NotesMod.Note);
        }

        public override void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            base.rebuild(additionalSaveData, replacement);
            if (additionalSaveData.ContainsKey("text"))
                text = additionalSaveData["text"];
        }

        public override Dictionary<string, string> getAdditionalSaveData()
        {
            var data = base.getAdditionalSaveData();
            data.Add("text", text);
            return data;
        }
    }
}
