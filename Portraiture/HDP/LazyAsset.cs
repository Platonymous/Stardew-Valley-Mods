using StardewModdingAPI.Events;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Portraiture.HDP
{
    //https://github.com/tlitookilakin/AeroCore/blob/master/Generics/LazyAsset.cs
    public abstract class LazyAsset
    {
        internal Func<string> getPath;
        internal bool ignoreLocale;

        internal static readonly ConditionalWeakTable<LazyAsset, IModHelper> Watchers = new();
        internal static void Init()
        {
            PortraitureMod.helper.Events.Content.AssetsInvalidated += CheckWatchers;
        }
        internal static void CheckWatchers(object _, AssetsInvalidatedEventArgs ev)
        {
            foreach ((var asset, var helper) in Watchers)
            {
                string path = asset.getPath();
                foreach (var name in asset.ignoreLocale ? ev.NamesWithoutLocale : ev.Names)
                {
                    if (name.IsEquivalentTo(path))
                    {
                        asset.Reload(); break;
                    }
                }
            }
        }
        public abstract void Reload();
    }
    public class LazyAsset<T> : LazyAsset
    {
        private readonly IModHelper helper;
        private T cached = default;
        private bool isCached = false;

        public T Value => GetAsset();
        public string LastError { get; private set; } = null;
        public bool CatchErrors { get; set; } = false;
        public event Action<LazyAsset<T>> AssetReloaded;

        public LazyAsset(IModHelper Helper, Func<string> AssetPath, bool IgnoreLocale = true)
        {
            getPath = AssetPath;
            helper = Helper;
            ignoreLocale = IgnoreLocale;

            Watchers.Add(this, Helper);
        }
        public T GetAsset()
        {
            if (!isCached)
            {
                LastError = null;
                isCached = true;
                if (CatchErrors)
                {
                    try
                    {
                        cached = helper.GameContent.Load<T>(getPath());
                    }
                    catch (Exception e)
                    {
                        LastError = e.ToString();
                        cached = default;
                    }
                }
                else
                {
                    cached = helper.GameContent.Load<T>(getPath());
                }
            }
            return cached;
        }
        public override void Reload()
        {
            cached = default;
            isCached = false;
            LastError = null;
            AssetReloaded?.Invoke(this);
        }
    }
}
