using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using xTile;
using Microsoft.Xna.Framework;
using Range = PyTK.Types.Range;

namespace PyTK.Extensions
{
    public static class PyAssets
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        /* Basics */

        public static AssetLoadInjector<T> injectLoad<T>(this AssetLoadInjector<T> t)
        {
            Helper.Events.Content.AssetRequested += t.OnAssetRequested;
            return t;
        }

        public static AssetEditInjector<T, TAsset> injectEdit<T, TAsset>(this AssetEditInjector<T, TAsset> t)
        {
            Helper.Events.Content.AssetRequested += t.OnAssetRequested;
            return t;
        }

        /* Textures */

        public static AssetLoadInjector<Texture2D> inject(this Texture2D t, string assetName)
        {
            return new AssetLoadInjector<Texture2D>(assetName, t).injectLoad();
        }

        public static AssetEditInjector<Texture2D, Texture2D> injectAs(this Texture2D t, string assetName)
        {
            return new AssetEditInjector<Texture2D, Texture2D>(assetName, t).injectEdit();
        }

        public static AssetEditInjector<IAssetDataForImage, IAssetDataForImage> injectInto(this Texture2D t, string assetName, Rectangle? source, Rectangle? target, PatchMode mode = PatchMode.Replace)
        {
            Func<IAssetDataForImage, IAssetDataForImage> merger = new Func<IAssetDataForImage, IAssetDataForImage>(delegate (IAssetDataForImage asset)
            {
                asset.PatchImage(t,source,target,mode);
                return asset;
            });

            return new AssetEditInjector<IAssetDataForImage, IAssetDataForImage>(assetName, merger).injectEdit();
        }

        public static AssetEditInjector<IAssetDataForImage, IAssetDataForImage> injectInto(this Texture2D t, string assetName, Point position, PatchMode mode = PatchMode.Replace)
        {
           return t.injectInto(assetName, null, new Rectangle(position.X, position.Y, t.Width, t.Height), mode);
        }

        public static AssetEditInjector<IAssetDataForImage, IAssetDataForImage> injectTileInto(this Texture2D t, string assetName, int targetTileIndex, int sourceTileIndex = 0, int tileWidth = 16, int tileHeight = 16, PatchMode mode = PatchMode.Replace)
        {
            Func<IAssetDataForImage, IAssetDataForImage> merger = new Func<IAssetDataForImage, IAssetDataForImage>(delegate (IAssetDataForImage asset)
            {
                Rectangle source = Game1.getSourceRectForStandardTileSheet(t, sourceTileIndex, tileWidth, tileHeight);
                Rectangle target = Game1.getSourceRectForStandardTileSheet(asset.Data, targetTileIndex, tileWidth, tileHeight);
                asset.PatchImage(t, source, target, mode);
                return asset;
            });

            return new AssetEditInjector<IAssetDataForImage, IAssetDataForImage>(assetName, merger).injectEdit();
        }

        public static AssetEditInjector<IAssetDataForImage, IAssetDataForImage> injectTileInto(this Texture2D t, string assetName, Range targetTileIndex, Range sourceTileIndex, int tileWidth = 16, int tileHeight = 16, PatchMode mode = PatchMode.Replace)
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

            return new AssetEditInjector<IAssetDataForImage, IAssetDataForImage>(assetName, merger).injectEdit();
        }

        /* Maps */

        public static AssetLoadInjector<Map> inject(this Map t, string assetName)
        {
            return new AssetLoadInjector<Map>(assetName, t).injectLoad();
        }

        public static AssetEditInjector<Map, Map> injectAs(this Map t, string assetName)
        {
            return new AssetEditInjector<Map, Map>(assetName, t).injectEdit();
        }

        public static AssetEditInjector<Map, Map> injectInto(this Map t, string assetName, Vector2 position, Rectangle? sourceRectangle)
        {
            Func<Map, Map> merger = new Func<Map, Map>(delegate (Map asset)
            {
                return t.mergeInto(asset, position, sourceRectangle);
            });

            return new AssetEditInjector<Map, Map>(assetName, merger).injectEdit();
        }

        /* Data */

        public static AssetLoadInjector<IDictionary<TKey, TValue>> inject<TKey, TValue>(this IDictionary<TKey, TValue> t, string assetName)
        {
            return new AssetLoadInjector<IDictionary<TKey, TValue>>(assetName, t).injectLoad();
        }

        public static AssetEditInjector<IDictionary<TKey, TValue>, IDictionary<TKey, TValue>> injectAs<TDict, TKey, TValue>(this IDictionary<TKey, TValue> t, string assetName)
        {
            return new AssetEditInjector<IDictionary<TKey, TValue>, IDictionary<TKey, TValue>>(assetName, t).injectEdit();
        }

        public static AssetEditInjector<IDictionary<TKey, TValue>, IDictionary<TKey, TValue>> injectInto<TKey, TValue>(this IDictionary<TKey, TValue> t, string assetName)
        {
            Func<IDictionary<TKey, TValue>, IDictionary<TKey, TValue>> merger = new Func<IDictionary<TKey, TValue>, IDictionary<TKey, TValue>>(delegate (IDictionary<TKey, TValue> asset)
            {
                return asset.AddOrReplace(t);
            });

            return new AssetEditInjector<IDictionary<TKey, TValue>, IDictionary<TKey, TValue>>(assetName, merger).injectEdit();
        }
    }
}
