using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;

namespace PelicanTTS
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
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max, int interval);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max, float interval);

        void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);

        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc,
                                   Func<Vector2, object, object> widgetUpdate,
                                   Func<SpriteBatch, Vector2, object, object> widgetDraw,
                                   Action<object> onSave);

        void SubscribeToChange(IManifest mod, Action<string, bool> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, int> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, float> changeHandler);
        void SubscribeToChange(IManifest mod, Action<string, string> changeHandler);

        void OpenModMenu(IManifest mod);
    }
}
