using StardewModdingAPI;

namespace TMXLoader
{
    public interface ITMXLAPI
    {
        void AddContentPack(IContentPack pack);
    }

    public class TMXLAPI : ITMXLAPI
    {

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
