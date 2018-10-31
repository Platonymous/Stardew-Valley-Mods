using StardewValley;

namespace CustomWallsAndFloors
{
    public class SavedRoom
    {
        public long Id { get; set; } = 0;
        public string Location { get; set; } = "na";
        public int Room { get; set; } = -1;
        public string Floors { get; set; } = "na";
        public string Walls { get; set; } = "na";
        public int WallsNr { get; set; } = 0;
        public int FloorsNr { get; set; } = 0;

        public SavedRoom(long id, string location, int room, string floors, string walls, int whichWall, int whichFloor)
        {
            Id = id;
            Location = location;
            Room = room;
            Floors = floors;
            Walls = walls;
            WallsNr = whichWall;
            FloorsNr = whichFloor;
        }
    }
}
