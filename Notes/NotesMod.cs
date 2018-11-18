using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using PyTK.Extensions;
using PyTK.CustomElementHandler;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Objects;

namespace Notes
{
    public class NotesMod : Mod
    {
        internal static IMonitor Logger;
        internal ITranslationHelper i18n => Helper.Translation;
        internal static CustomObjectData Note;
        internal static CustomObjectData Desk;
        internal static string NoteInfo;
        internal static string displayNote = ""; 

        public override void Entry(IModHelper helper)
        {
            Logger = Monitor;
            NoteInfo = i18n.Get("notes.note.info");
            initNotes();

            ControlEvents.MouseChanged += (s,e) => checkForSigns();
            GraphicsEvents.OnPostRenderEvent += GraphicsEvents_OnPostRenderEvent;
        }

        private void GraphicsEvents_OnPostRenderEvent(object sender, System.EventArgs e)
        {
            if (displayNote == "")
                return;
            IClickableMenu.drawHoverText(Game1.spriteBatch, displayNote, Game1.smallFont, 0, 0, -1);
        }

        public static void checkForSigns()
        {
            if (Game1.activeClickableMenu != null)
                return;
            int xTile = (Game1.viewport.X + Game1.getOldMouseX()) / 64;
            int yTile = (Game1.viewport.Y + Game1.getOldMouseY()) / 64;
            Vector2 oneDown = new Vector2(xTile, yTile + 1);
            Vector2 pos = new Vector2(xTile, yTile);
            if (Game1.currentLocation != null
                && Game1.currentLocation.objects.ContainsKey(pos)
                && Game1.currentLocation.objects[pos] is Sign sign
                && sign.displayItem.Value is Note n)
                displayNote = n.text;
            else if (Game1.currentLocation != null
                && Game1.currentLocation.objects.ContainsKey(oneDown)
                && Game1.currentLocation.objects[oneDown] is Sign sign2
                && sign2.displayItem.Value is Note n2)
                displayNote = n2.text;
            else
                displayNote = "";
        }

        private void initNotes()
        {
            Texture2D noteTexture = Helper.Content.Load<Texture2D>("Assets/Note.png");
            Texture2D deskTexture = Helper.Content.Load<Texture2D>("Assets/Desk.png");
            Note = new CustomObjectData("Notes.Note", i18n.Get("notes.note.name") + "/0/-300/Basic/" + i18n.Get("notes.note.name") + "/" + i18n.Get("notes.note.name"),noteTexture,Color.White,type:typeof(Note));
            Desk = new CustomObjectData("Notes.Desk", $"{i18n.Get("notes.desk.name")}/100/-300/Crafting -9/{i18n.Get("notes.desk.description")}/true/true/0/{i18n.Get("notes.desk.name")}",deskTexture, Color.White, 0, true, typeof(Desk), new CraftingData("Notes.Desk", "388 2"));
        }
    }
}
