using System.Collections.Generic;

namespace LandGrants.Game
{
    public class WerwolfUpdate : WerwolfMPMessage
    {
        public override string Type { get; set; } = "Update";
        public List<WerwolfClientPlayer> Players {get; set;} = new List<WerwolfClientPlayer>();
        public List<WerwolfClientPlayer> Winners { get; set; } = new List<WerwolfClientPlayer>();

        public string WinMessage { get; set; }

        public int Round { get; set; } = -1;

        public bool WolvesWon { get; set; }

        public WerwolfUpdate()
        {

        }

        public WerwolfUpdate(int round, long sendTo, long sendFrom, WerwolfGame game, List<WerwolfClientPlayer> players, List<WerwolfClientPlayer> winners, string winMessage, bool wolveswon) : base(sendTo, sendFrom, game)
        {
            WinMessage = winMessage;
            Players = players;
            Winners = winners ?? new List<WerwolfClientPlayer>();
            WinMessage = winMessage;
            WolvesWon = wolveswon;
            Round = round;
        }
    }
}
