using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PyTKLite
{
    public class PyTKLiteMod : Mod
    {
        static IAssetName LastAsset;
        public static IMonitor mon;

        internal static Dictionary<string, ScaledTexture2D> ScaledTextures = new Dictionary<string, ScaledTexture2D>();

        public override void Entry(IModHelper helper)
        {
            mon = Monitor;
            Monitor.Log("Using PyTK Lite", LogLevel.Info);
            var instance = new Harmony("Platonymous.PyTKLite.ScaleUp");

            instance.Patch(AccessTools.Method(Type.GetType("StardewModdingAPI.Framework.ContentManagers.ModContentManager, StardewModdingAPI"), "LoadRawImageData"), null, new HarmonyMethod(this.GetType(), nameof(LoadRawImageData)));

            instance.Patch(AccessTools.Method(Type.GetType("StardewModdingAPI.Framework.ContentCoordinator, StardewModdingAPI"), "ParseAssetName"), null, new HarmonyMethod(this.GetType(), nameof(ParseAssetName)));

            instance.Patch(AccessTools.Method(Type.GetType("StardewModdingAPI.Framework.InternalExtensions, StardewModdingAPI"), "SetName", 
                new Type[] { typeof(Texture2D), typeof(IAssetName) }),
                new HarmonyMethod(typeof(PyTKLiteMod), nameof(SetName)));

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        public static bool SetName(Texture2D texture, IAssetName assetName, ref Texture2D __result)
        {
            if (ScaledTextures.TryGetValue(assetName.Name, out ScaledTexture2D tex))
            {
                ScaledTextures.Remove(assetName.Name);
                __result = tex;
                return false;
            }

            return true;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            OvSpritebatchNew.initializePatch(new Harmony("Platonymous.PyTKLite.ScaleUp.Spritebatch"));
        }

        public static void ParseAssetName(ref IAssetName __result)
        {
            LastAsset = __result;
        }
        public static void LoadRawImageData(object __instance, FileInfo file, bool forRawData, ref IRawTextureData __result)
        {
            foreach (var f in file.Directory.GetFiles())
            {
                var fname = Path.GetFileNameWithoutExtension(file.Name);
                var cfname = Path.GetFileNameWithoutExtension(f.Name);
                bool isPyTK = (f.FullName.EndsWith(".pytk.json") || f.FullName.EndsWith(".pytk"));

                if (isPyTK)
                {
                    if (JsonConvert.DeserializeObject<ScaleUpData>(File.ReadAllText(f.FullName)) is ScaleUpData scaleData)
                    {

                        Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, __result.Width, __result.Height);

                        texture.Name = LastAsset.Name;
                        texture.SetData(__result.Data);
                        texture = Premultiply(texture);


                        ScaledTextures.Remove(LastAsset.Name);

                        if (ScaleUp(texture, scaleData, false) is ScaledTexture2D scaled)
                        {
                            if(!ScaledTextures.ContainsKey(LastAsset.Name))
                                ScaledTextures.Add(LastAsset.Name, scaled);

                            __result = scaled;
                        }

                    }

                    break;
                }

            }
        }

        private static Texture2D Premultiply(Texture2D texture)
        {
            int count = texture.Width * texture.Height;
            Color[] data = ArrayPool<Color>.Shared.Rent(count);
            try
            {
                texture.GetData(data, 0, count);

                bool changed = false;
                for (int i = 0; i < count; i++)
                {
                    ref Color pixel = ref data[i];
                    if (pixel.A is (byte.MinValue or byte.MaxValue))
                        continue;

                    data[i] = new Color(pixel.R * pixel.A / byte.MaxValue, pixel.G * pixel.A / byte.MaxValue, pixel.B * pixel.A / byte.MaxValue, pixel.A);
                    changed = true;
                }

                if (changed)
                    texture.SetData(data, 0, count);
            }
            finally
            {
                ArrayPool<Color>.Shared.Return(data);
            }

            return texture;
        }

        public static Texture2D ScaleUp(Texture2D texture, ScaleUpData data, bool full)
        {
            if (data is ScaleUpData && !(texture is ScaledTexture2D))
            {
                bool scaled = false, animated = false, loop = true;
                float scale = 1f;
                int tileWidth = 0, tileHeight = 0, fps = 0;

                if (data.SourceArea is int[] area && area.Length == 4)
                    texture = texture.getArea(new Rectangle(area[0], area[1], area[2], area[3]));

                if (data.Scale != 1f)
                {
                    scale = data.Scale;
                    scaled = true;
                }

                if (data.Animation is Animation anim)
                {
                    tileHeight = anim.FrameHeight == -1 ? texture.Height : Math.Min(texture.Height, anim.FrameHeight);
                    tileWidth = anim.FrameWidth == -1 ? texture.Width : Math.Min(texture.Width, anim.FrameWidth);
                    fps = anim.FPS;
                    loop = anim.Loop;

                    if (!(tileWidth == texture.Width && tileHeight == texture.Height))
                        animated = true;
                }

                if (animated)
                    return new AnimatedTexture2D(Premultiply(texture), tileWidth, tileHeight, fps, loop, full ? 1f : !scaled ? 1f : scale);
                else if (scaled)
                    return ScaledTexture2D.FromTexture(full ? texture : texture.ScaleUpTexture(1f / scale, false), Premultiply(texture), scale);
            }
            return texture;
        }


    }

}