using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;

namespace InkStories
{
    public class InkStoriesToken
    {
        public bool IsMutable() => false;
        public bool AllowsInput() => true;
        public bool RequiresInput() => true;
        public bool CanHaveMultipleValues(string input = null) => false;
        public bool UpdateContext() => false;
        public bool IsReady() => true;

        public virtual IEnumerable<string> GetValues(string input)
        {
            string[] values = input.Trim().Split(' ', System.StringSplitOptions.TrimEntries);
            bool isJson = values.Length > 1 && values[1].ToUpper() == "JSON";
            string id = values[0];
            string asset = PathUtilities.NormalizeAssetName(InkUtils.PlatformPath(InkStoriesMod.STORIESASSET, id));
            InkStoriesMod.Stories.Add(id, new InkStory(id, asset, isJson ? "JSON" : "TEXT"));
            return new[] { asset };
        }
    }
}
