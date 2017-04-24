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


        void rebuildFromSave(dynamic additionalSaveData);


    }
}
