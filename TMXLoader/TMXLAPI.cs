using StardewModdingAPI;
using StardewValley;
using System;

namespace TMXLoader
{
    public interface ITMXLAPI
    {
        void AddContentPack(IContentPack pack);

        event EventHandler<GameLocation> OnLocationRestoring;
    }

    public class TMXLAPI : ITMXLAPI
    {
        protected static event EventHandler<GameLocation> OnLocationRestoringEvent;
        
        public event EventHandler<GameLocation> OnLocationRestoring
        {
            add
            {
                OnLocationRestoringEvent += value;
            }
            remove
            {
                OnLocationRestoringEvent -= value;
            }
        }

        internal static void RaiseOnLocationRestoringEvent(GameLocation inGame)
        {
            OnLocationRestoringEvent?.Invoke(null, inGame);
        }

        public void AddContentPack(IContentPack pack)
        {
            if (TMXLoaderMod.AddedContentPacks.Contains(pack))
                return;

            TMXLoaderMod.AddedContentPacks.Add(pack);

            if (TMXLoaderMod.contentPacksLoaded)
                TMXLoaderMod._instance.loadPack(pack,"content");
        }
    }
}
