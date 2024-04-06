using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace InkStories
{
    public class InkStoriesStoreToken
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
            string id = values[0];
            string asset = PathUtilities.NormalizeAssetName(InkUtils.PlatformPath(InkStoriesMod.STOREASSET, id));
            InkStoriesMod.Store.Add(id, new Dictionary<string, string>());
            return new[] { asset };
        }
    }
}
