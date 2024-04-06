using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace TMXLoader
{
    public class AssetEditInjector<TAsset, TSource>
    {
        private readonly Func<TSource, TAsset> asset;
        private readonly Func<IAssetName, bool> predicate;

        public void Invalidate()
        {
            TMXLoaderMod.helper.GameContent.InvalidateCache(a => predicate(a.Name));
        }

        public AssetEditInjector(string assetName, TAsset asset)
        {
            this.asset = _ => asset;
            predicate = a => a.IsEquivalentTo(assetName, useBaseName: true);
        }

        public AssetEditInjector(string assetName, Func<TSource, TAsset> asset)
        {
            this.asset = asset;
            predicate = a => a.IsEquivalentTo(assetName, useBaseName: true);
        }

        internal void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (predicate(e.NameWithoutLocale))
            {
                if (typeof(TSource) == typeof(IAssetDataForImage))
                    e.Edit(asset => this.asset((TSource)asset.AsImage()));
                else
                    e.Edit(asset => asset.ReplaceWith(this.asset.Invoke(asset.GetData<TSource>())));
            }
        }
    }
}