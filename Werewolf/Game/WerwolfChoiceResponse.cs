using System.Collections.Generic;

namespace Werewolf.Game
{
    public class WerwolfChoiceResponse : WerwolfMPMessage
    {
        public string ChoiceID { get; set; }
        public string Answer { get; set; }
        public override string Type { get; set; } = "Response";

        public WerwolfChoiceResponse()
        {

        }
        public WerwolfChoiceResponse(WerwolfClientGame game, string choiceid, string answer) : base(game.Host, game.LocalPlayer.ID, game)
        {
            ChoiceID = choiceid;
            Answer = answer;
        }
    }
}
