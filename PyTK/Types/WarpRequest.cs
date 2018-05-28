using StardewValley;

namespace PyTK.Types
{
    public class WarpRequest
    {
        public long farmerId { get; set; }
        public string locationName { get; set; }
        public bool isStructure { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int facing { get; set; }

        public WarpRequest()
        {

        }

        public WarpRequest(Farmer farmer, string location, int x, int y, bool isStructure, int facingAfterWarp = -1)
        {
            farmerId = farmer.UniqueMultiplayerID;
            this.isStructure = isStructure;
            this.x = x;
            this.y = y;
            facing = facingAfterWarp;
            locationName = location;
        }
    }
}
