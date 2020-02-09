using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomMusic
{
    public interface IGMCMAPI
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

        void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);

        void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);

        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc,
                                   Func<Vector2, object, object> widgetUpdate,
                                   Func<SpriteBatch, Vector2, object, object> widgetDraw,
                                   Action<object> onSave);
    }

    public class GMCMConfig
    {
        public List<GMCMOption> Options = new List<GMCMOption>();
        public static IGMCMAPI Api = null;
        public IManifest Manifest = null;
        public Action<string, string> SaveHandler;
        public static bool Patched = false;
        public GMCMOption Label = null;
        public GMCMConfig(IManifest manifest, Action<string, string> saveHandler, List<GMCMOption> options, GMCMLabel label = null)
        {
            Manifest = manifest;
            Options = options;
            SaveHandler = saveHandler;
            Label = label;
        }

        public static void PatchGMCM(IModHelper helper)
        {
            if (Patched)
                return;

            HarmonyInstance instance = HarmonyInstance.Create("CustomMusic.GMCMConfig");
            instance.Patch(Type.GetType("GenericModConfigMenu.UI.Dropdown, GenericModConfigMenu").GetMethod("Update"), new HarmonyMethod(typeof(GMCMConfig).GetMethod("UpdateGMCM")));
            instance.Patch(Type.GetType("GenericModConfigMenu.UI.Dropdown, GenericModConfigMenu").GetMethod("Draw"), new HarmonyMethod(typeof(GMCMConfig).GetMethod("DrawGMCM")));

            helper.Events.Input.MouseWheelScrolled += (s, e) =>
            {
                List<object> obj = new List<object>();
                foreach (var dropDown in currentIndex.Keys.Where(k => ((bool)k.GetType().GetField("dropped").GetValue(k))))
                {
                    obj.Add(dropDown);
                    currentIndex[dropDown] -= e.Delta / 120;
                    currentIndex[dropDown] = Math.Max(currentIndex[dropDown], 0);
                    break;
                }
            };

            Patched = true;

            instance.Patch(Type.GetType("GenericModConfigMenu.UI.Scrollbar, GenericModConfigMenu").GetMethod("Scroll"), new HarmonyMethod(typeof(GMCMConfig).GetMethod("ScrollGMCM")));
        }

        public static bool ScrollGMCM(object __instance)
        {
            return Mouse.GetState().LeftButton != ButtonState.Pressed;
        }

            public static IGMCMAPI GetAPI(IModHelper helper)
        {
            if (Api is IGMCMAPI)
                return Api;

            if (!helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                return null;

            Api = helper.ModRegistry.GetApi<IGMCMAPI>("spacechase0.GenericModConfigMenu");
            return Api;
        }

        public bool register(IModHelper helper)
        {
            if (GetAPI(helper) == null)
                return false;

            PatchGMCM(helper);

            Api.RegisterModConfig(Manifest, () =>
            {
                foreach(var option in Options)
                    option.ActiveIndex = option.DefaultIndex;

                activeSound?.Stop(true);
            }, () => SaveHandler.Invoke("save","file"));


            if (Label != null)
                Api.RegisterLabel(Manifest, Label.Name, Label.Description);

            foreach (var option in Options)
            Api.RegisterChoiceOption(Manifest, option.Name, option.Description, () =>
            {
                activeSound?.Stop(true);
                return option.Choices[option.ActiveIndex];
            }, (s) =>
            {
                activeSound?.Stop(true);
                option.ActiveIndex = option.Choices.IndexOf(s);
                SaveHandler(option.Name, s);
            }, option.Choices.ToArray());

            Api.RegisterLabel(Manifest, "", "");
            Api.RegisterLabel(Manifest, "", "");
            Api.RegisterLabel(Manifest, "", "");
            Api.RegisterLabel(Manifest, "", "");
            Api.RegisterLabel(Manifest, "", "");
            Api.RegisterLabel(Manifest, "", "");

            return true;
        }

        internal static Dictionary<object, int> currentIndex = new Dictionary<object, int>();
        internal const int maxValues = 5;
        public static SoundEffectInstance activeSound = null;

        public static bool DrawGMCM(object __instance, SpriteBatch b)
        {
            List<string> choicesc = new List<string>((string[])__instance.GetType().GetProperty("Choices").GetValue(__instance));

            if (!choicesc[0].StartsWith("CM:"))
                return true;

            Vector2 pos = (Vector2)__instance.GetType().GetProperty("Position").GetValue(__instance);
            Texture2D tex = (Texture2D)__instance.GetType().GetProperty("Texture").GetValue(__instance);
            Microsoft.Xna.Framework.Rectangle rect = (Microsoft.Xna.Framework.Rectangle)__instance.GetType().GetProperty("BackgroundTextureRect").GetValue(__instance);
            Microsoft.Xna.Framework.Rectangle rect2 = (Microsoft.Xna.Framework.Rectangle)__instance.GetType().GetProperty("ButtonTextureRect").GetValue(__instance);
            string[] choices = (string[])__instance.GetType().GetProperty("Choices").GetValue(__instance);
            string value = (string)__instance.GetType().GetProperty("Value").GetValue(__instance);
            int aChoice = (int)__instance.GetType().GetProperty("ActiveChoice").GetValue(__instance);
            bool dropped = (bool)__instance.GetType().GetField("dropped").GetValue(__instance);

            IClickableMenu.drawTextureBox(b, tex, rect, (int)pos.X, (int)pos.Y, 252, 44, Color.White, 4f, false);
            b.DrawString((SpriteFont)Game1.smallFont, value, new Vector2(pos.X + 4f, pos.Y + 8f), (Color)Game1.textColor);
            b.Draw(tex, new Vector2((float)((double)pos.X + 300.0 - 48.0), pos.Y), new Microsoft.Xna.Framework.Rectangle?(rect2), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0f);

            if (!dropped)
                return true;

            if (!currentIndex.ContainsKey(__instance))
                currentIndex.Add(__instance, 0);

            currentIndex[__instance] = Math.Min(Math.Max(Math.Min(currentIndex[__instance], choices.Length - 1), 0), choices.Length - maxValues);
            int num = maxValues * 44;
            IClickableMenu.drawTextureBox(b, tex, rect, (int)pos.X, (int)pos.Y, 252, num, Color.White, 4f, false);
            int cIndex = 0;
            for (int index = currentIndex[__instance]; index < Math.Min(choices.Length, currentIndex[__instance] + maxValues); ++index)
            {

                if (index == aChoice)
                    b.Draw((Texture2D)Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)pos.X + 4, (int)pos.Y + cIndex * 44, 244, 44), new Microsoft.Xna.Framework.Rectangle?(), Color.Wheat, 0.0f, Vector2.Zero, SpriteEffects.None, 0.98f);

                b.DrawString((SpriteFont)Game1.smallFont, choices[index], new Vector2(pos.X + 4f, (float)((double)pos.Y + (double)(cIndex * 44) + 8.0)), (Color)Game1.textColor, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                cIndex++;
            }

            return false;
        }

        public static bool UpdateGMCM(object __instance, ref bool __state)
        {
            List<string> choices = new List<string>((string[])__instance.GetType().GetProperty("Choices").GetValue(__instance));

            if (!choices[0].StartsWith("CM:"))
                return true;

            if (!currentIndex.ContainsKey(__instance))
                currentIndex.Add(__instance, 0);

            bool dropped = (bool)__instance.GetType().GetField("dropped").GetValue(__instance);
            Vector2 pos = (Vector2)__instance.GetType().GetProperty("Position").GetValue(__instance);
            int aChoice = (int)__instance.GetType().GetProperty("ActiveChoice").GetValue(__instance);
            object callback = __instance.GetType().GetField("Callback").GetValue(__instance);
            object parent = __instance.GetType().GetProperty("Parent").GetValue(__instance);


            MouseState state;
            int num;

            if (new Rectangle((int)pos.X, (int)pos.Y, 300, 44).Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) && Game1.oldMouseState.LeftButton == ButtonState.Released)
            {
                state = Mouse.GetState();
                num = state.LeftButton == ButtonState.Pressed ? 1 : 0;
            }
            else
                num = 0;
            if (num != 0)
            {
                __instance.GetType().GetField("dropped").SetValue(__instance, true);
                parent.GetType().GetProperty("RenderLast").SetValue(parent, __instance);
            }
            if (!dropped)
                currentIndex[__instance] = Math.Min(aChoice, choices.Count - maxValues);

            if (!dropped)
                return false;
            state = Mouse.GetState();
            if (state.LeftButton == ButtonState.Released)
            {
                __instance.GetType().GetField("dropped").SetValue(__instance, false);
                if (parent.GetType().GetProperty("RenderLast").GetValue(parent) == __instance)
                    parent.GetType().GetProperty("RenderLast").SetValue(parent, null);

                string value = (string)__instance.GetType().GetProperty("Value").GetValue(__instance);
                activeSound?.Stop(true);
                Game1.stopMusicTrack(Game1.MusicContext.Default);

                if (value.StartsWith("Vanilla:"))
                {
                   
                }
                else if (value != "Any" && value != "Random" && value != "CM:Default" && CustomMusicMod.Music.First(m => Path.GetFileNameWithoutExtension(m.Path) == value) is StoredMusic sm)
                {
                    activeSound = sm.Sound.CreateInstance();
                    activeSound.Play();
                }
            }

            if (new Rectangle((int)pos.X, (int)pos.Y, 300, 44 * maxValues).Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
            {
                int idx = ((Game1.getOldMouseY() - (int)pos.Y) / 44) + currentIndex[__instance];
                idx = Math.Min(idx, choices.Count - 1);
                __instance.GetType().GetProperty("ActiveChoice").SetValue(__instance, idx);
                if (callback != null)
                {
                    try
                    {
                        callback.GetType().GetMethod("Invoke").Invoke(callback, new[] { __instance });
                    }
                    catch
                    {

                    }
                }
            }

            return false;
        }

    }

    

    public static class ReflectionHelper
    {
        public static object GetFieldValue(this object obj, string field, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            return t.GetField(field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(isStatic ? null : obj);
        }

        public static void SetFieldValue(this object obj, object value, string field, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            t.GetField(field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue(isStatic ? null : obj, value);
        }

        public static object GetPropertyValue(this object obj, string property, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            return t.GetProperty(property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(isStatic ? null : obj);
        }

        public static void SetPropertyValue(this object obj, object value, string property, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            t.GetProperty(property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue(isStatic ? null : obj, value);
        }
    }
}
