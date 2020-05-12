using System;
using StardewModdingAPI;
using PyTK.Extensions;
using System.IO;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;

namespace ScaleUp
{
    public class ScaleUpMod : Mod
    {
        public static bool shouldBeTrue = false;
        public static ScaleUpMod modInstance = null;
        public static List<IContentPack> contentPacks = new List<IContentPack>();
        public const string scalePattern = @"scale@([\d.]+)x";
        public const string animPattern = @"anim@([\d.]+)x([\d.]+)x([\d.]+)";


        public override void Entry(IModHelper helper)
        {
            modInstance = this;
            helper.Content.AssetEditors.Add(new Scaler(helper));
            HarmonyInstance harmony = HarmonyInstance.Create("Platonymous.ScaleUp");
            harmony.Patch(
                AccessTools.DeclaredMethod(typeof(Texture2D), "FromStream", new Type[] { typeof(GraphicsDevice), typeof(Stream) }),
                prefix: new HarmonyMethod(AccessTools.DeclaredMethod(this.GetType(), "FromStream")));
            loadContentPacks();
        }


        public static void FromStream(Stream stream, ref Texture2D __result)
        {
            if (stream is FileStream fs && Path.GetFileNameWithoutExtension(fs.Name) is string key)
            {
                bool scaled = false;
                bool animated = false;
                float scale = 1f;
                int tileWidth = 0;
                int tileHeight = 0;
                int fps = 0;
                bool loop = true;

                scaled = Regex.IsMatch(key, scalePattern);
                animated = Regex.IsMatch(key, animPattern);

                if (!scaled && !animated)
                    return;

                if (!(animated && Regex.Match(key, animPattern) is Match ma
                    && int.TryParse(ma.Groups[1].Value, out tileWidth)
                    && int.TryParse(ma.Groups[2].Value, out tileHeight)
                    && int.TryParse(ma.Groups[3].Value, out fps)))
                    animated = false;

                if (!(scaled && Regex.Match(key, scalePattern) is Match m && float.TryParse(m.Groups[1].Value, out scale)))
                    scaled = false;

                if (scaled && scale == 1f)
                    scaled = false;

                if (animated && tileHeight == -1)
                    tileHeight = __result.Height;

                if (animated && tileWidth == -1)
                    tileWidth = __result.Width;

                if (animated && tileWidth == __result.Width && tileHeight == __result.Height)
                    animated = false;

                if (!scaled && !animated)
                    return;

                if (animated)
                    __result = new AnimatedTexture2D(__result, tileWidth, tileHeight, fps, loop, !scaled ? 1f : scale);
                else if (scaled)
                    __result = ScaledTexture2D.FromTexture(__result.ScaleUpTexture(1f / scale, false), __result, scale);
            }
        }

        private void loadContentPacks()
        {
            if (!Helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher"))
                return;
            var modData = Helper.ModRegistry.Get("Pathoschild.ContentPatcher");
            var contentPatcher = (Mod)modData.GetType().GetProperty("Mod", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetValue(modData);

            PyTK.APIs.IScalerAPI ScalerAPI = new PyTK.APIs.PyTKAPI();

            foreach (var pack in contentPatcher.Helper.ContentPacks.GetOwned())
            {

                Content content = pack.ReadJsonFile<Content>("content.json");
                if (content == null)
                    continue;

                foreach (Changes change in content.Changes)
                {
                    if ((change.Action != "Load" && change.Action != "EditImage") || !change.ScaleUp || (change.Action == "Load" && change.OriginalWidth == -1) || change.Target == "")
                        continue;

                    if (change.Action == "EditImage" && change.FromFileScaled != "" && change.ToArea.ContainsKey("X") && change.ToArea.ContainsKey("Y") && change.ToArea.ContainsKey("Width") && change.ToArea.ContainsKey("Height"))
                    {
                        change.Target = change.Target.Replace('/', '\\');
                        Texture2D sTex = pack.LoadAsset<Texture2D>(change.FromFileScaled);
                        Texture2D s = null;

                        if (change.AnimationFrameTime == -1)
                            s = ScaledTexture2D.FromTexture(PyTK.PyDraw.getRectangle(change.ToArea["Width"], change.ToArea["Height"], Color.White), sTex, sTex.Width / change.OriginalWidth);
                        else if ((sTex.Width / (float)change.OriginalWidth) is float sc)
                            s = new AnimatedTexture2D(sTex, (int)(change.ToArea["Width"] * sc), (int)(change.ToArea["Height"] * sc), 60 / change.AnimationFrameTime, true, sc);

                        ScalerAPI.ReplaceAssetAt(change.Target, new Microsoft.Xna.Framework.Rectangle(change.ToArea["X"], change.ToArea["Y"], change.ToArea["Width"], change.ToArea["Height"]), s);
                    }
                    else if (change.Action == "Load")
                        Scaler.Assets.AddOrReplace(change.Target, change.OriginalWidth);

                }
            }


        }

    }
}
