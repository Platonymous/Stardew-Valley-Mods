using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTV
{
    internal class CustomTVChannelToken
    {
        internal static readonly Dictionary<string,TVChannel> Channels = new Dictionary<string, TVChannel>();
        internal static readonly Dictionary<string, TVScreen> Screens = new Dictionary<string, TVScreen>(); 
        internal const string ChannelDataPrefix = @"Data/TV/Channel_";
        internal const string ScreenTexturePrefix = @"Data/TV/Screens/Screen_";
        public bool IsMutable() => false;
        public bool AllowsInput() => true;
        public bool RequiresInput() => true;
        public bool CanHaveMultipleValues(string input = null) => false;
        public bool UpdateContext() => false;
        public bool IsReady() => true;
        
        internal static string GetScreenId(string channel, string screen)
        {
            return $"{channel}.{screen}";
        }

        public virtual IEnumerable<string> GetValues(string input)
        {
            string output = $"{ChannelDataPrefix}{input}";

            if (!Channels.ContainsKey(input))
            {
                Channels.Add(input, null);
                CustomTVMod.PlatoHelper.Content.Injections.InjectLoad<Dictionary<string, string>>(output, new Dictionary<string, string>());
            }

            return new[] { output };
        }
    }

}
