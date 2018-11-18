using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notes
{
    class Desk : PySObject
    {
        public Desk() : base() { }
        public Desk(CustomObjectData data) : base(data) { }
        public Desk(CustomObjectData data, Vector2 tileLocation) : base(data, tileLocation) { }

        public override Item getOne()
        {
            return new Desk(data, Vector2.Zero) { name = name, Price = price, Quality = quality };
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            return new Desk(NotesMod.Desk, additionalSaveData["tileLocation"].Split(',').toList(i => i.toInt()).toVector<Vector2>());
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
                return true;

            Game1.activeClickableMenu = (IClickableMenu)new NoteMenu(new NamingMenu.doneNamingBehavior((s) => {

                if (s.Length > 0) {
                    if (s.Length > 100)
                        s = s.Substring(0, 97) + "...";

                    Note note = (Note) NotesMod.Note.getObject();
                    note.text = s;
                    Game1.exitActiveMenu();
                    Game1.player.addItemByMenuIfNecessary(note);
                }

            }), NotesMod.NoteInfo, "");
            return false;
        }
    }
}
