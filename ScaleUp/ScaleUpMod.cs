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
            harmony.Patch(AccessTools.DeclaredMethod(typeof(Texture2D), "FromStream", new Type[] { typeof(GraphicsDevice), typeof(Stream) }), postfix: new HarmonyMethod(this.GetType().GetMethod("LoadFix", BindingFlags.Public | BindingFlags.Static)));
            harmony.Patch(
                original: AccessTools.Method(Type.GetType("StardewModdingAPI.Framework.Content.AssetDataForImage, StardewModdingAPI"), "PatchImage"),
                prefix: new HarmonyMethod(this.GetType().GetMethod("PatchImage", BindingFlags.Public | BindingFlags.Static))

            );
            loadContentPacks();
        }

        public static void PatchImage(IAssetDataForImage __instance, Texture2D source, Rectangle? sourceArea, Rectangle? targetArea, PatchMode patchMode)
        {
            if (source is ScaledTexture2D scaled)
            {
                var a = new Rectangle(0, 0, __instance.Data.Width, __instance.Data.Height);
                var s = new Rectangle(0, 0, source.Width, source.Height);
                var sr = !sourceArea.HasValue ? s : sourceArea.Value;
                var tr = !targetArea.HasValue ? sr : targetArea.Value;

                if (a == tr && patchMode == PatchMode.Replace)
                {
                   __instance.ReplaceWith(source);
                   return;
                }

                if (patchMode == PatchMode.Overlay)
                    scaled.AsOverlay = true;

                if (scaled.AsOverlay)
                {
                    Color[] data = new Color[(int)(tr.Width) * (int)(tr.Height)];
                    __instance.Data.getArea(tr).GetData(data);
                    scaled.SetData<Color>(data);
                }

                if (__instance.Data is MappedTexture2D map)
                    map.Set(tr, scaled);
                else
                    __instance.ReplaceWith(new MappedTexture2D(__instance.Data, new Dictionary<Rectangle?, Texture2D>() { { tr, scaled } }));

            }
        }

        public static void LoadFix(Stream stream, ref Texture2D __result)
        {
            if (stream is FileStream fs && Path.GetFileNameWithoutExtension(fs.Name) is string key)
            {
                bool scaled = false;
                bool animated = false;
                float scale = 1f;
                int tileWidth = 0;
                int tileHeight = 0;
                int fps = 0;
                string dir = Path.GetDirectoryName(fs.Name);
                bool loop = true;
                string dataFile = Path.Combine(dir, key + ".su.json");

                if (File.Exists(dataFile))
                {
                    ScaleUpData data = Newtonsoft.Json.JsonConvert.DeserializeObject<ScaleUpData>(File.ReadAllText(dataFile));

                    if (data is ScaleUpData)
                    {
                        if (data.Scale != 1f)
                        {
                            scale = data.Scale;
                            scaled = true;
                        }

                        if (data.Animation is Animation anim)
                        {
                            tileHeight = anim.FrameHeight;
                            tileWidth = anim.FrameWidth;
                            fps = anim.FPS;
                            loop = anim.Loop;
                            animated = true;
                        }
                    }
                }
                else
                {
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
                }

                if (scale == 1f)
                    scaled = false;

                if (tileHeight == -1)
                    tileHeight = __result.Height;

                if (tileWidth == -1)
                    tileWidth = __result.Width;

                if (tileWidth == __result.Width && tileHeight == __result.Height)
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
