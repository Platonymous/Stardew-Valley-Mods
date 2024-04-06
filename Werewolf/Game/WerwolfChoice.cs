using System;
using System.Collections.Generic;

namespace LandGrants.Game
{
    public class WerwolfChoice : WerwolfMPMessage
    {
        public override string Type { get; set; } = "Choice";
        public string ChoiceID {get; set;}

        public string Question { get; set; }

        public List<WerwolfChoiceOption> Options { get; set; } = new List<WerwolfChoiceOption>();

        public WerwolfChoice()
        {

        }

        public WerwolfChoice(long sendTo, long sendFrom, WerwolfGame game, string choiceID, string question, List<WerwolfChoiceOption> options, Action<string, string> callback)
            : base(sendTo, sendFrom, game, choiceID, callback)
        { 
            ChoiceID = choiceID;
            Question = question;
            Options = options;
        }
    }
}

