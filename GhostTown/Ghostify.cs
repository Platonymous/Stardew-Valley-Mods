using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace GhostTown
{
    class Ghostify
    {
        public readonly IModHelper helper;
        private readonly ColorManipulation spriteGhostifyer;
        private readonly ColorManipulation portraitGhostifyer;
        private readonly ColorManipulation mapsGhostifyer;

        private readonly HashSet<string> IgnoreNpcSpriteNames = new(
            new [] { "Gunther", "Marlon", "Krobus", "Bouncer", "Morris", "Sandy", "Henchman", "Dwarf", "Henchman", "Junimo", "MrQi", "robot", "Mariner" },
            StringComparer.OrdinalIgnoreCase
        );

        public Ghostify(IModHelper helper)
        {
            this.helper = helper;
            float ts = 0.6f;
            float tp = 0.9f;
            List<Color> colors = new List<Color>() { Color.Black * 0, Color.Gray * tp, Color.LightCyan * tp, Color.LightBlue * tp, Color.LightGray * tp, Color.LightSkyBlue * tp, Color.LightSlateGray * tp, Color.MidnightBlue * tp, Color.DarkSlateGray * tp, Color.DimGray * tp, new Color(1,1,1), Color.DarkGray * tp, Color.AliceBlue * tp, Color.Aqua * tp, Color.DarkBlue * tp, Color.WhiteSmoke * tp, Color.Blue * tp, Color.CadetBlue * tp, Color.SlateBlue * tp, Color.DarkSlateBlue * tp };
            List<Color> colorsTransparent = new List<Color>() { Color.Black * 0, Color.Gray * ts, Color.LightCyan * ts, Color.LightBlue * ts, Color.LightGray * ts, Color.LightSkyBlue * ts, Color.LightSlateGray * ts, Color.MidnightBlue * ts, Color.DarkSlateGray * ts, Color.DimGray * ts, new Color(10, 10, 10) * ts, Color.DarkGray * ts, Color.AliceBlue * ts, Color.Aqua * ts, Color.DarkBlue * ts, Color.WhiteSmoke * ts, Color.Blue * ts, Color.CadetBlue * ts, Color.SlateBlue * ts, Color.DarkSlateBlue * ts };
            portraitGhostifyer = new ColorManipulation(colors);
            spriteGhostifyer = new ColorManipulation(colorsTransparent);
            mapsGhostifyer = GhostTownMod.config.desaturate ? new ColorManipulation(40, 100) : new ColorManipulation();
        }

        public void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (this.CanEdit(e.DataType, e.NameWithoutLocale, out ColorManipulation effect))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsImage();
                    editor.ReplaceWith(editor.Data.changeColor(effect));
                });
            }
        }

        public bool CanEdit(Type assetType, IAssetName assetName, out ColorManipulation effect)
        {
            effect = null;
            if (!typeof(Texture2D).IsAssignableFrom(assetType))
                return false;

            // animals
            if (assetName.IsDirectlyUnderPath("Animals"))
            {
                if (GhostTownMod.config.animals)
                    effect = spriteGhostifyer;
            }

            // critters
            else if (assetName.IsEquivalentTo("LooseSprites/critters"))
            {
                if (GhostTownMod.config.critters)
                    effect = spriteGhostifyer;
            }

            // NPC portraits
            else if (assetName.IsDirectlyUnderPath("Portraits"))
            {
                if (GhostTownMod.config.people)
                    effect = portraitGhostifyer;
            }

            // NPC sprites
            else if (assetName.IsDirectlyUnderPath("Characters"))
            {
                if (GhostTownMod.config.people)
                {
                    string npcName = PathUtilities.GetSegments(assetName.Name, limit: 2)[1];
                    if (!this.IgnoreNpcSpriteNames.Contains(npcName))
                        effect = spriteGhostifyer;
                }
            }

            // any other non-farmer textures
            else if (!assetName.StartsWith("Characters/Farmer/"))
                effect = mapsGhostifyer;

            return effect != null;
        }
    }
}
