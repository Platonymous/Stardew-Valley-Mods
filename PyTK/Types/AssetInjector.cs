using StardewModdingAPI;
using System;

namespace PyTK.Types
{
    public class AssetInjector<T, TAsset> : IAssetLoader, IAssetEditor
    {
        private Func<TAsset, T> asset;
        private Func<IAssetInfo, bool> predicate;

        public AssetInjector(string assetName, T asset)
        {
            this.asset = new Func<TAsset, T>(delegate (TAsset a) { return asset; });
            predicate = new Func<IAssetInfo, bool>(delegate (IAssetInfo a) { return a.AssetNameEquals(assetName); });
        }

        public AssetInjector(string assetName, Func<TAsset, T> asset)
        {
            this.asset = asset;
            predicate = new Func<IAssetInfo, bool>(delegate (IAssetInfo a) { return a.AssetNameEquals(assetName); });
        }

        public AssetInjector(Func<IAssetInfo, bool> predicate, Func<TAsset, T> asset)
        {
            this.predicate = predicate;
            this.asset = asset;
        }

        public bool CanEdit<TAssetRequest>(IAssetInfo asset)
        {
            bool can = predicate.Invoke(asset);
            return can;
        }

        public bool CanLoad<TAssetRequest>(IAssetInfo asset)
        {
            return predicate.Invoke(asset);
        }

        public void Edit<TAssetRequest>(IAssetData asset)
        {
            if (typeof(TAsset) == typeof(IAssetDataForImage))
                this.asset.Invoke((TAsset)asset.AsImage());
            else
                asset.ReplaceWith(this.asset.Invoke(asset.GetData<TAsset>()));
        }

        public TAssetRequest Load<TAssetRequest>(IAssetInfo asset)
        {
            return (TAssetRequest) (object) this.asset.Invoke((TAsset)asset);
        }
    }
}
