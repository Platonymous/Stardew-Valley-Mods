namespace Comics
{
    public interface IComicsModAPI
    {
        void PreventShopPlacement();

        void SetShopKeeper(string name);

        void SetShopText(string text);

    }
}
