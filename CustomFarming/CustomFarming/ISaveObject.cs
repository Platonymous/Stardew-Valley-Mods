using Microsoft.Xna.Framework;
using StardewValley;


namespace CustomFarming
{
    public interface ISaveObject
    {
        bool InStorage
        {
            get;
            set;
        }

        GameLocation Environment
        {
            get;
            set;
        }

        Vector2 Position
        {
            get;
            set;
        }

        StardewValley.Object getReplacement();
        dynamic getAdditionalSaveData();
        void rebuildFromSave(dynamic additionalSaveData);


    }
}
