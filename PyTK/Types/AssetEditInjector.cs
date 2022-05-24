using System;
using StardewModdingAPI;

namespace PyTK.Types
{
    public class AssetEditInjector<TAsset, TSource> : IAssetEditor
    {
        private readonly Func<TSource, TAsset> asset;
        private readonly Func<IAssetName, bool> predicate;

        public void Invalidate()
        {
            PyTKMod._helper.GameContent.InvalidateCache(a => predicate(a.Name));
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

        public bool CanEdit<TAssetRequest>(IAssetInfo asset)
        {
            return predicate(asset.NameWithoutLocale);
        }

        public void Edit<TAssetRequest>(IAssetData asset)
        {
            if (typeof(TSource) == typeof(IAssetDataForImage))
                this.asset((TSource)asset.AsImage());
            else
                asset.ReplaceWith(this.asset.Invoke(asset.GetData<TSource>()));
        }
    }
}