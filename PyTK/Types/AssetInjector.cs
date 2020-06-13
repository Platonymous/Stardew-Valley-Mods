using StardewModdingAPI;
using System;

namespace PyTK.Types
{
    public class AssetInjector<T, TAsset> : IAssetLoader, IAssetEditor
    {
        private Func<TAsset, T> asset;
        private Func<IAssetInfo, bool> predicate;
        private Func<bool> conditions;
        private bool lastCheck;
        private bool disabled = false;
        
        public void Invalidate()
        {
            PyTKMod._helper.Content.InvalidateCache(predicate);
        }

        public void Disable()
        {
            disabled = true;
            Invalidate();
        }

        public void Enable()
        {
            disabled = false;
            Invalidate();
        }

        public void ApplyConditions(Func<bool> conditions = null)
        {
            this.conditions = conditions;
            bool check = conditions == null || conditions.Invoke();

            if (check != lastCheck)
            {
                lastCheck = check;
                Invalidate();
            }
        }

        public AssetInjector(string assetName, T asset)
        {
            this.asset = new Func<TAsset, T>(delegate (TAsset a) { return asset; });
            predicate = new Func<IAssetInfo, bool>(delegate (IAssetInfo a) { return a.AssetNameEquals(assetName); });
            lastCheck = true;
        }

        public AssetInjector(string assetName, Func<TAsset, T> asset)
        {
            this.asset = asset;
            predicate = new Func<IAssetInfo, bool>(delegate (IAssetInfo a) { return a.AssetNameEquals(assetName); });
            lastCheck = true;
        }

        public AssetInjector(Func<IAssetInfo, bool> predicate, Func<TAsset, T> asset)
        {
            this.predicate = predicate;
            this.asset = asset;
            lastCheck = true;
        }

        public bool CanEdit<TAssetRequest>(IAssetInfo asset)
        {
            if (disabled)
                return false;

            bool can = predicate.Invoke(asset);
            if(can)
                lastCheck = conditions == null || conditions.Invoke();
            return can && lastCheck;
        }

        public bool CanLoad<TAssetRequest>(IAssetInfo asset)
        {
            bool can = predicate.Invoke(asset);
            if (can)
                lastCheck = conditions == null || conditions.Invoke();

            return can && lastCheck;
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
