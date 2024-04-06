using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
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
                    editor.ReplaceWith(changeColor(editor.Data,effect));
                });
            }
        }

        public static Texture2D applyPalette(Texture2D t, List<Color> palette)
        {
            ColorManipulation manipulation = new ColorManipulation(palette);
            return changeColor(t, manipulation);
        }

        public static Texture2D setSaturation(Texture2D t, float saturation)
        {
            ColorManipulation manipulation = new ColorManipulation(saturation);
            return changeColor(t, manipulation);
        }

        public static Texture2D setLight(Texture2D t, float light)
        {
            ColorManipulation manipulation = new ColorManipulation(100, light);
            return changeColor(t,manipulation);
        }


        public static Texture2D changeColor(Texture2D t, ColorManipulation manipulation)
        {
            Color[] colorData = new Color[t.Width * t.Height];
            t.GetData(colorData);
            for (int x = 0; x < t.Width; x++)
                for (int y = 0; y < t.Height; y++)
                    colorData[x * t.Height + y] = changeColor(colorData[x * t.Height + y], manipulation);

            t.SetData(colorData);

            return t;
        }


        public static Color changeColor(Color t, ColorManipulation manipulation)
        {
            t = setLight(t,manipulation.light);
            t = setSaturation(t,manipulation.saturation);
            if (manipulation.palette.Count > 0)
                t = applyPalette(t,manipulation.palette);
            return t;
        }
        public static Color setSaturation(Color t, float saturation, Vector3? saturationMultiplier = null)
        {
            Vector3 m = saturationMultiplier.HasValue ? saturationMultiplier.Value : new Vector3(0.2125f, 0.7154f, 0.0721f);
            float l = m.X * t.R + m.Y * t.G + m.Z * t.B;
            float s = 1f - (saturation / 100);

            float newR = t.R;
            float newG = t.G;
            float newB = t.B;

            if (s != 0)
            {
                newR = newR + s * (l - newR);
                newG = newG + s * (l - newG);
                newB = newB + s * (l - newB);
            }

            t.R = (byte)MathHelper.Min(newR, 255);
            t.G = (byte)MathHelper.Min(newG, 255);
            t.B = (byte)MathHelper.Min(newB, 255);

            return t;
        }

        public static Color setLight(Color t, float light)
        {
            float l = light / 100;
            t.R = (byte)Math.Min(t.R * l, 255);
            t.G = (byte)Math.Min(t.G * l, 255);
            t.B = (byte)Math.Min(t.B * l, 255);

            return t;
        }

        public static int getDistanceTo(Color current, Color match)
        {
            int redDifference;
            int greenDifference;
            int blueDifference;

            redDifference = current.R - match.R;
            greenDifference = current.G - match.G;
            blueDifference = current.B - match.B;

            return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
        }

        public static Color applyPalette(Color current, List<Color> palette)
        {
            int index = -1;
            int shortestDistance = int.MaxValue;

            for (int i = 0; i < palette.Count; i++)
            {
                int distance = getDistanceTo(current,palette[i]);
                if (distance < shortestDistance)
                {
                    index = i;
                    shortestDistance = distance;
                }
            }

            return palette[index];
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

    public class ColorManipulation
    {
        public float saturation;
        public float light;
        public List<Color> palette;

        public ColorManipulation(List<Color> palette, float saturation = 100, float light = 100)
        {
            this.saturation = saturation;
            this.light = light;
            this.palette = palette;
        }

        public ColorManipulation(float saturation = 100, float light = 100)
        {
            this.saturation = saturation;
            this.light = light;
            palette = new List<Color>();
        }
    }
}
