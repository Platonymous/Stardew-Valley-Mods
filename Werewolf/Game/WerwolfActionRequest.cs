using System.Collections.Generic;

namespace LandGrants.Game
{
    public class WerwolfActionRequest : WerwolfMPMessage
    {
        public string ActionID { get; set; }

        public long TargetPlayer { get; set; }

        public override string Type { get; set; } = "Action";

        public WerwolfActionRequest()
        {

        }

        public WerwolfActionRequest(WerwolfClientGame game, string actionID, long targetPlayer) : base(game.Host, game.LocalPlayer.ID, game)
        {
            ActionID = actionID;
            TargetPlayer = targetPlayer;
        }
    }
}
