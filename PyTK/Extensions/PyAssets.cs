using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using xTile;
using Microsoft.Xna.Framework;

namespace PyTK.Extensions
{
    public static class PyAssets
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        /* Basics */

        public static AssetInjector<T, IAssetInfo> injectLoad<T, IAssetInfo>(this AssetInjector<T, IAssetInfo> t)
        {
            Helper.Content.AssetLoaders.Add(t);
            return t;
        }

        public static AssetInjector<T, TAsset> injectEdit<T, TAsset>(this AssetInjector<T, TAsset> t)
        {
            Helper.Content.AssetEditors.Add(t);
            return t;
        }

        /* Textures */

        public static AssetInjector<Texture2D, IAssetInfo> inject(this Texture2D t, string assetName)
        {
            return new AssetInjector<Texture2D, IAssetInfo>(assetName, t).injectLoad();
        }

        public static AssetInjector<Texture2D, Texture2D> injectAs(this Texture2D t, string assetName)
        {
            return new AssetInjector<Texture2D, Texture2D>(assetName, t).injectEdit();
        }

        public static AssetInjector<IAssetDataForImage, IAssetDataForImage> injectInto(this Texture2D t, string assetName, Rectangle? source, Rectangle? target, PatchMode mode = PatchMode.Replace)
        {
            Func<IAssetDataForImage, IAssetDataForImage> merger = new Func<IAssetDataForImage, IAssetDataForImage>(delegate (IAssetDataForImage asset)
            {
                asset.PatchImage(t,source,target,mode);
                return asset;
            });

            return new AssetInjector<IAssetDataForImage, IAssetDataForImage>(assetName, merger).injectEdit();
        }

        public static AssetInjector<IAssetDataForImage, IAssetDataForImage> injectInto(this Texture2D t, string assetName, Point position, PatchMode mode = PatchMode.Replace)
        {
           return t.injectInto(assetName, null, new Rectangle(position.X, position.Y, t.Width, t.Height), mode);
        }

        public static AssetInjector<IAssetDataForImage, IAssetDataForImage> injectTileInto(this Texture2D t, string assetName, int targetTileIndex, int sourceTileIndex = 0, int tileWidth = 16, int tileHeight = 16, PatchMode mode = PatchMode.Replace)
        {
            Func<IAssetDataForImage, IAssetDataForImage> merger = new Func<IAssetDataForImage, IAssetDataForImage>(delegate (IAssetDataForImage asset)
            {
                Rectangle source = Game1.getSourceRectForStandardTileSheet(t, sourceTileIndex, tileWidth, tileHeight);
                Rectangle target = Game1.getSourceRectForStandardTileSheet(asset.Data, targetTileIndex, tileWidth, tileHeight);
                asset.PatchImage(t, source, target, mode);
                return asset;
            });

            return new AssetInjector<IAssetDataForImage, IAssetDataForImage>(assetName, merger).injectEdit();
        }

        public static AssetInjector<IAssetDataForImage, IAssetDataForImage> injectTileInto(this Texture2D t, string assetName, Range targetTileIndex, Range sourceTileIndex, int tileWidth = 16, int tileHeight = 16, PatchMode mode = PatchMode.Replace)
        {
            Func<IAssetDataForImage, IAssetDataForImage> merger = new Func<IAssetDataForImage, IAssetDataForImage>(delegate (IAssetDataForImage asset)
            {
                for(int i = 0; i < sourceTileIndex.length; i++)
                {
                    Rectangle source = Game1.getSourceRectForStandardTileSheet(t, sourceTileIndex[i], tileWidth, tileHeight);
                    Rectangle target = Game1.getSourceRectForStandardTileSheet(asset.Data, targetTileIndex[i], tileWidth, tileHeight);
                    asset.PatchImage(t, source, target, mode);
                }
                return asset;
            });

            return new AssetInjector<IAssetDataForImage, IAssetDataForImage>(assetName, merger).injectEdit();
        }

        /* Maps */

        public static AssetInjector<Map, IAssetInfo> inject(this Map t, string assetName)
        {
            return new AssetInjector<Map, IAssetInfo>(assetName, t).injectLoad();
        }

        public static AssetInjector<Map, Map> injectAs(this Map t, string assetName)
        {
            return new AssetInjector<Map, Map>(assetName, t).injectEdit();
        }

        public static AssetInjector<Map, Map> injectInto(this Map t, string assetName, Vector2 position, Rectangle? sourceRectangle)
        {
            Func<Map, Map> merger = new Func<Map, Map>(delegate (Map asset)
            {
                return t.mergeInto(asset, position, sourceRectangle);
            });

            return new AssetInjector<Map, Map>(assetName, merger).injectEdit();
        }

        /* Data */

        public static AssetInjector<Dictionary<TKey, TValue>, IAssetInfo> inject<TKey, TValue>(this Dictionary<TKey, TValue> t, string assetName)
        {
            return new AssetInjector<Dictionary<TKey, TValue>, IAssetInfo>(assetName, t).injectLoad();
        }

        public static AssetInjector<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>> injectAs<TDict, TKey, TValue>(this Dictionary<TKey, TValue> t, string assetName)
        {
            return new AssetInjector<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>>(assetName, t).injectEdit();
        }

        public static AssetInjector<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>> injectInto<TKey, TValue>(this Dictionary<TKey, TValue> t, string assetName)
        {
            Func<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>> merger = new Func<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>>(delegate (Dictionary<TKey, TValue> asset)
            {
                return asset.AddOrReplace(t);
            });

            return new AssetInjector<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>>(assetName, merger).injectEdit();
        }
    }
}
