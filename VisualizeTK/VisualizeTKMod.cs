using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.IO;
using StardewValley;
using HarmonyLib;
using System.Linq;
using System.Collections.Generic;
using StardewValley.Quests;
using System;
using System.Diagnostics;
using VisualizeTK;
using xTile;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace VisualizeTK
{
    public class VisualizeTKMod : Mod
    {
        private Effect Shader { get; set; } = null;

        public static VisualizeTKMod Singleton { get; set; } = null;

        private static bool ShaderActive { get; set; } = false;

        private float SwitchSpeed { get; set; } = 0f;

        private static bool World { get; set; } = false;

        private Color[] Colors { get; set; } = new Color[0];

        private Color TargetColor { get; set; } = Color.White;
        private int ColorIndex { get; set; } = 0;

        private Color CurrentColor { get; set; } = Color.White;

        public static bool Override { get; set; } = false;

        public bool CheckLocation { get; set; } = true;

        public ShaderParameters Temporary { get; set; } = null;

        float Process { get; set; } = 0f;
 
        public override void Entry(IModHelper helper)
        {
            Singleton = this;

            Harmony h = new Harmony("Platonymous.VisualizeTK");
            h.Patch(
               original: AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Begin)),
               prefix: new HarmonyMethod(typeof(VisualizeTKMod), nameof(AddShader)));

#if DEBUG || RELEASE
            h.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.DrawWorld)),
               prefix: new HarmonyMethod(typeof(VisualizeTKMod), nameof(DrawWorld1)),
               postfix: new HarmonyMethod(typeof(VisualizeTKMod), nameof(DrawWorld2)));
#endif

#if PREALPHADEBUG || PREALPHARELEASE
            h.Patch(
               original: AccessTools.Method(typeof(Game1), "Draw"),
               prefix: new HarmonyMethod(typeof(VisualizeTKMod), nameof(DrawWorld1)));
            h.Patch(
               original: AccessTools.Method(typeof(Game1), "ShouldShowOnscreenUsernames"),
               prefix: new HarmonyMethod(typeof(VisualizeTKMod), nameof(DrawWorld2)));

            h.Patch(AccessTools.Method(typeof(Event), nameof(Event.tryEventCommand)),
               new HarmonyMethod(typeof(VisualizeTKMod), nameof(TryEventCommandPre)),
               new HarmonyMethod(typeof(VisualizeTKMod), nameof(TryEventCommandPost)));
#endif

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.Content.AssetsInvalidated += Content_AssetsInvalidated;
            helper.Events.Player.Warped += Player_Warped;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked1;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
#if DEBUG || RELEASE
            StardewValley.Event.RegisterCommand("vtk_preset", (e,p,c) => VTKEventCommandPreset(e,p));
            StardewValley.Event.RegisterCommand("vtk_tint", (e, p, c) => VTKEventCommandColors(e, p));
            StardewValley.Event.RegisterCommand("vtk_tspeed", (e, p, c) => VTKEventCommandSpeed(e, p));
            StardewValley.Event.RegisterCommand("vtk_sat", (e, p, c) => VTKEventCommandSat(e, p));
            StardewValley.Event.RegisterCommand("vtk_clear", (e, p, c) => VTKEventCommandClear(e, p));
#endif

        }

        public static bool TryEventCommandPre(Event __instance, string[] split)
        {
            return TryEventCommand(__instance, split);
        }

        public static void TryEventCommandPost(Event __instance, string[] split)
        {
            TryEventCommand(__instance, split);
        }

        public static bool TryEventCommand(Event __instance, string[] split)
        {

            if (split.Length == 0)
                return true;

            if (split[0] == "vtk_preset")
            {
                Singleton.VTKEventCommandPreset(__instance, split);
                return false;
            }


            if (split[0] == "vtk_tint")
            {
                Singleton.VTKEventCommandColors(__instance, split);
                return false;
            }


            if (split[0] == "vtk_tspeed")
            {
                Singleton.VTKEventCommandSpeed(__instance, split);
                return false;
            }


            if (split[0] == "vtk_sat")
            {
                Singleton.VTKEventCommandSat(__instance, split);
                return false;
            }


            if (split[0] == "vtk_clear")
            {
                Singleton.VTKEventCommandClear(__instance, split);
                return false;
            }

            return true;
        }

        public void VTKEventCommandClear(Event @event, string[] args)
        {
            Temporary = null;
            CheckLocation = true;
            @event.CurrentCommand++;
        }

        public void VTKEventCommandPreset(Event @event, string[] args)
        {
            if(args.Length > 1)
            {
                if (Temporary == null)
                    Temporary = new ShaderParameters();

                Temporary.Preset = args[1];
                Presets.ApplyPreset(args[1]);
                ShaderActive = true;
            }

            @event.CurrentCommand++;
        }

        public void VTKEventCommandSpeed(Event @event, string[] args)
        {
            if (args.Length > 0 && float.TryParse(args[0], out float speed) && ShaderActive)
            {
                if (Temporary == null)
                    Temporary = new ShaderParameters();

                SwitchSpeed = speed;
            }
            @event.CurrentCommand++;
        }
        public void VTKEventCommandColors(Event @event, string[] args)
        {
            if (args.Length > 1 && ShaderActive)
            {
                if (Temporary == null)
                    Temporary = new ShaderParameters();
                List<string> paramter = new List<string>(args);
                paramter.RemoveAt(0);
                Temporary.Colors = paramter.Select(c => TryParseColorFromString(c, out Color color) ? color : Color.White).ToArray();
                if (Temporary.Colors.Length > 1)
                {
                    ColorIndex = 1;
                    Process = 0;
                    Colors = Temporary.Colors;
                    CurrentColor = Temporary.Colors[0];
                    TargetColor = Temporary.Colors[1];
                    SwitchSpeed = SwitchSpeed <= 0 ? Temporary.SwitchSpeed : SwitchSpeed;
                    SwitchSpeed = SwitchSpeed <= 0 ? 1 : SwitchSpeed;
                    SwitchColors();
                }
                else
                {
                    SwitchSpeed = 0;
                    SetTint(Temporary.Colors[0]);
                }

            }
            @event.CurrentCommand++;
        }

        public void VTKEventCommandSat(Event @event, string[] args)
        {
            if (args.Length > 1 && ShaderActive)
            {
                if (float.TryParse(args[1], out float sat))
                {
                    if (Temporary == null)
                        Temporary = new ShaderParameters();

                    GetShader();
                    Temporary.SatR = sat;
                    if (args.Length == 1)
                    {
                        Temporary.SatG = sat;
                        Temporary.SatB =sat;
                    }
                    else if (args.Length > 2 && float.TryParse(args[2], out float satg))
                        Temporary.SatG = satg;
                    else if (args.Length > 3 && float.TryParse(args[3], out float satb))
                        Temporary.SatB = satb;

                    Shader.Parameters["satr"].SetValue(1f - Temporary.SatR);
                    Shader.Parameters["satb"].SetValue(1f - Temporary.SatG);
                    Shader.Parameters["satg"].SetValue(1f - Temporary.SatB);
                }
            }
            @event.CurrentCommand++;
        }

        private void AddMapProperty(xTile.Map map, string name, string value) {

            if (map.Properties.ContainsKey(name))
                map.Properties[name] = value;
            else
                map.Properties.Add(name, value);
        
        }

        private void Display_Rendering(object sender, RenderingEventArgs e)
        {
            World = true;
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
#if DEBUG || PREALPHADEBUG

            if(e.DataType == typeof(Dictionary<string,string>) && e.NameWithoutLocale.Name.Contains("Farm"))
            {
                e.Edit(a =>
                {
                    var asset = a.AsDictionary<string, string>();
                    if (asset.Data.ContainsKey("65/m 25000/t 600 1200/H"))
                    {
                        asset.Data["65/m 25000/t 600 1200/H"] = asset.Data["65/m 25000/t 600 1200/H"].Replace("farmer 64 16 2 Demetrius 64 18 0", "farmer 64 16 2 Demetrius 64 18 0/vtk_preset Custom/vtk_sat 0.2 0.1 0.8/vtk_tint 255-0-0 0-255-0/vtk_tspeed 2.0");
                        asset.Data["65/m 25000/t 600 1200/H"] = asset.Data["65/m 25000/t 600 1200/H"].Replace("/end", "/vtk_clear/end");
                    }

                });
            }
            if (e.DataType == typeof(xTile.Map) && e.NameWithoutLocale.BaseName.Contains("FarmHouse"))
                e.Edit((data) =>
                {
                    data.AsMap().Data.Properties.Add("VTK_Preset", "Grey");
                    data.AsMap().Data.Properties.Add("VTK_Tint", "255-0-0,255-255-255");
                    data.AsMap().Data.Properties.Add("VTK_TintSpeed", "2");
                });
            else if (e.DataType == typeof(xTile.Map) && e.NameWithoutLocale.BaseName.Contains("Farm"))
                e.Edit((data) =>
                {
                    data.AsMap().Data.Properties.Add("VTK_Preset", "Midnight");
                });
            else if (e.DataType == typeof(xTile.Map) && e.NameWithoutLocale.BaseName.Contains("BusStop"))
                e.Edit((data) =>
                {
                    data.AsMap().Data.Properties.Add("VTK_Preset", "Sunset");
                });
            else if (e.DataType == typeof(xTile.Map) && e.NameWithoutLocale.BaseName.Contains("Town"))
                e.Edit((data) =>
                {
                    data.AsMap().Data.Properties.Add("VTK_Preset", "Sepia");
                });
            else if (e.DataType == typeof(xTile.Map) && e.NameWithoutLocale.BaseName.Contains("Beach"))
                e.Edit((data) =>
                {
                    data.AsMap().Data.Properties.Add("VTK_Preset", "Heat");
                });
            else if (e.DataType == typeof(xTile.Map) && e.NameWithoutLocale.BaseName.Contains("SeedShop"))
                e.Edit((data) =>
                {
                    data.AsMap().Data.Properties.Add("VTK_Preset", "Underwater");
                });
            else if (e.DataType == typeof(xTile.Map) && e.NameWithoutLocale.BaseName.Contains("Forest"))
                e.Edit((data) =>
                {
                    data.AsMap().Data.Properties.Add("VTK_Preset", "Desaturated");
                });
#endif
        }

        private void GameLoop_UpdateTicked1(object sender, UpdateTickedEventArgs e)
        {
            if(Context.IsWorldReady && CheckLocation)
            {
                if(Temporary != null)
                {
                    if (Presets.ApplyPreset(Temporary.Preset))
                        ShaderActive = true;
                }
                else if (Game1.currentLocation.Map.Properties.TryGetValue("VTK_Preset", out xTile.ObjectModel.PropertyValue v))
                {
                    if (Presets.ApplyPreset(v.ToString()))
                        ShaderActive = true;
                    else
                        ShaderActive = false;
                }
                else
                    ShaderActive = false;
                CheckLocation = false;
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            CheckLocation = true;
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            Temporary = null;
            ShaderActive = false;
            CheckLocation = true;
        }

        private void Content_AssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            if(e.Names.Any(i => i.Name.Contains("Map")))
                CheckLocation = true;
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            CheckLocation = true;
            ShaderActive = false;
            Temporary = null;
        }


        public static void DrawWorld1()
        {
            World = true;
        }
        public static void DrawWorld2()
        {
            World = false;
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            SwitchColors();
        }

        private void SwitchColors()
        {
            if (SwitchSpeed == 0 || Colors.Length < 2)
                return;

            Process += (0.005f) * SwitchSpeed;
            if (Process > 1f)
            {
                CurrentColor = Colors[ColorIndex];
                ColorIndex++;
                if (ColorIndex >= Colors.Length)
                    ColorIndex = 0;
                TargetColor = Colors[ColorIndex];
                Process = 0;
            }
            else
            {
                var c = Color.Lerp(CurrentColor, TargetColor, Process);
                c.A = 255;
                SetTint(c);
            }
        }


        private void SetTint(Color tint)
        {
           Shader.Parameters["tint"].SetValue(new Vector4(tint.R / 255f, tint.G / 255f, tint.B / 255f, 1));
        }

        public bool TryGetFloatFromProperty(xTile.Map map, string prop, out float value)
        {
            value = -1;
            if (map.Properties.TryGetValue(prop, out xTile.ObjectModel.PropertyValue val) && float.TryParse(val.ToString(), out float fval))
            {
                value = fval;
                return true;
            }

            return false;
        }

        public bool TryGetColorsFromProperty(xTile.Map map, string prop, out Color[] value)
        {
            value = new Color[0];
            if (map.Properties.TryGetValue(prop, out xTile.ObjectModel.PropertyValue val) && val.ToString().Split(',') is string[] colors)
            {
                value = colors.Select(c => TryParseColorFromString(c, out Color color) ? color : Color.White).ToArray();
                return true;
            }

            return false;
        }

        public static bool TryParseColorFromString(string value, out Color color)
        {
            var c = value.Split('-');

            if (c.Length == 3)
            {
                try
                {
                    color = new Color(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]));
                    return true;
                }
                catch
                {
                }
            }

            color = Color.White;
            return false;
        }

        public void SetShaderParameters(float satr, float satg, float satb, Color[] colors, float switchSpeed)
        {
            GetShader();

            if(Game1.currentLocation is GameLocation gl && gl.Map is xTile.Map map)
            {

                if (TryGetFloatFromProperty(map, "VTK_Sat", out float vsat))
                {
                    satr = vsat;
                    satg = vsat;
                    satb = vsat;
                }

                if (TryGetFloatFromProperty(map, "VTK_SatR", out float vsatr))
                    satr = vsatr;

                if (TryGetFloatFromProperty(map, "VTK_SatG", out float vsatg))
                    satg = vsatg;

                if (TryGetFloatFromProperty(map, "VTK_SatB", out float vsatb))
                    satb = vsatb;

                if (TryGetColorsFromProperty(map, "VTK_Tint", out Color[] vcolors))
                    colors = vcolors;

                if (TryGetFloatFromProperty(map, "VTK_TintSpeed", out float vspeed))
                    switchSpeed = vspeed;
                else if (colors.Length > 2 && switchSpeed == 0)
                    switchSpeed = 1;
            }

            if (Temporary != null)
            {
                satr = Temporary.SatR > 0 ? Temporary.SatR : satr;
                satg = Temporary.SatG > 0 ? Temporary.SatG : satg;
                satb = Temporary.SatB > 0 ? Temporary.SatB : satb;
                switchSpeed = Temporary.SwitchSpeed > 0 ? Temporary.SwitchSpeed : switchSpeed;
                colors = Temporary.Colors.Length > 0 ? Temporary.Colors : colors;
            }

            if (colors.Length > 1)
            {
                ColorIndex = 1;
                Process = 0;
                Colors = colors;
                CurrentColor = colors[0];
                TargetColor = colors[1];
                SwitchSpeed = switchSpeed;
                SwitchColors();
            }
            else
            {
                SwitchSpeed = 0;
                SetTint(colors[0]);
            }

           


                Shader.Parameters["satr"].SetValue(1f - satr);
                Shader.Parameters["satb"].SetValue(1f - satg);
                Shader.Parameters["satg"].SetValue(1f - satb);
                       
        }

        private Effect GetShader()
        {
            if (Shader == null)
            {
                byte[] bytecode = File.ReadAllBytes(Path.Combine(Helper.DirectoryPath, "visualizetk.ogl.mgfx"));
                Shader = new Effect(Game1.graphics.GraphicsDevice, bytecode);
                Shader.CurrentTechnique = Shader.Techniques["Visualize"];
            }

            return Shader;
        }

        public static void AddShader(ref Effect effect)
        {
            if(ShaderActive && World && effect == null)
                effect = Singleton.GetShader();

        }
    }
}
