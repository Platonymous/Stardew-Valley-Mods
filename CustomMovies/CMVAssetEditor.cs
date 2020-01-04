using StardewModdingAPI;
using System;

namespace CustomMovies
{
    class CMVAssetEditor : IAssetEditor
    {
        public static CustomMovieData CurrentMovie { get; set; } = null;
        private IModHelper helper;

        public CMVAssetEditor(IModHelper helper)
        {
            this.helper = helper;
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"LooseSprites\Movies");
        }

        public void Edit<T>(IAssetData asset)
        {
            if (CurrentMovie != null)
                asset.ReplaceWith(CurrentMovie._texture);
        }

    }
}
