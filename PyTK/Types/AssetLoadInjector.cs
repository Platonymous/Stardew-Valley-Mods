using System;
using StardewModdingAPI;

namespace PyTK.Types
{
    public class AssetLoadInjector<TAsset> : IAssetLoader
    {
        private readonly TAsset asset;
        private readonly Func<IAssetName, bool> predicate;

        public void Invalidate()
        {
            PyTKMod._helper.GameContent.InvalidateCache(a => predicate(a.Name));
        }

        public AssetLoadInjector(string assetName, TAsset asset)
        {
            this.asset = asset;
            predicate = a => a.IsEquivalentTo(assetName, useBaseName: true);
        }

        public bool CanLoad<TAssetRequest>(IAssetInfo asset)
        {
            return predicate(asset.NameWithoutLocale);
        }

        public TAssetRequest Load<TAssetRequest>(IAssetInfo asset)
        {
            return (TAssetRequest)(object)this.asset;
        }
    }
}