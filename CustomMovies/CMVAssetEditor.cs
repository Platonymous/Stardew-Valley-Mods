using StardewModdingAPI.Events;

namespace CustomMovies
{
    static class CMVAssetEditor
    {
        public static CustomMovieData CurrentMovie { get; set; } = null;

        public static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (CurrentMovie is not null && e.Name.IsEquivalentTo("LooseSprites/Movies"))
                e.Edit(asset => asset.ReplaceWith(CurrentMovie._texture));
        }
    }
}
