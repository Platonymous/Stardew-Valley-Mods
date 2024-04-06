using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using xTile.Tiles;
using StardewValley;

namespace Portraiture2
{
    internal class ScaleUpToken
    {
        public bool IsMutable() => false;
        public bool AllowsInput() => false;
        public bool RequiresInput() => false;
        public bool CanHaveMultipleValues(string input = null) => false;
        public bool UpdateContext() => false;
        public bool IsReady() => true;

        public virtual IEnumerable<string> GetValues(string input)
        {
            return new[] { ScaleUpMod.ScaleUpdDataAsset };
        }
    }

    public class ScaleUpMod : Mod
    {      
        public const string ScaleUpdDataAsset = @"Platonymous.ScaleUp/Assets"; 
        public static Dictionary<string,ScaleUpData> Scales { get; set; } = new Dictionary<string, ScaleUpData>();
        public static ScaleUpMod Singleton { get; set; }
        public override void Entry(IModHelper helper)
        {
            Singleton = this;
            HarmonyPatches.PatchAll(this);
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Content.AssetsInvalidated += Content_AssetsInvalidated;
            helper.Events.Content.AssetReady += Content_AssetReady;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Scales = Helper.GameContent.Load<Dictionary<string, ScaleUpData>>(ScaleUpdDataAsset);
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            api.RegisterToken(ModManifest, "Assets", new ScaleUpToken());
        }

        private void Content_AssetReady(object sender, AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.IsDirectlyUnderPath("Platonymous.ScaleUp"))
            {
                Scales = Helper.GameContent.Load<Dictionary<string, ScaleUpData>>(ScaleUpdDataAsset);
                CheckForDuplicates();
                foreach(var key in Scales.Keys)
                {
                    Monitor.Log($"ScaleData for the Asset {Scales[key].Asset} was added by {key}.", LogLevel.Trace);
                }
            }
        }

        private void Content_AssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(a => a.IsDirectlyUnderPath("Platonymous.ScaleUp")))
            {
                Scales.Clear();
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsDirectlyUnderPath("Platonymous.ScaleUp"))
            {
                e.LoadFrom(() => Scales, AssetLoadPriority.High);
            }
        }
        public void CheckForDuplicates()
        {
            foreach (var item in Scales.Values.ToArray())
            {
                if(Scales.Values.Any(v => v != item && v.Asset == item.Asset))
                {
                    var keys = Scales.Keys.Where(k => Scales[k].Asset == item.Asset).ToArray();
                    foreach (var item1 in keys)
                        Scales.Remove(item1);

                    Monitor.Log($"The Asset {item.Asset} was targeted by multiple mods ({string.Join(',',keys)}), all scaleup-data for this asset was removed.", LogLevel.Error);
                }
            }
        }
    }

}
