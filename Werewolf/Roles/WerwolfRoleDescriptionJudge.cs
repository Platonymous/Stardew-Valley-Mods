using System;
using System.Collections.Generic;
using System.Linq;
using LandGrants.Game;

namespace LandGrants.Roles
{
    public class WerwolfRoleDescriptionJudge : WerwolfRoleDescriptionBase
    {
        public WerwolfRoleDescriptionJudge(WerwolfPlayer player) : base(player)
        {
        }

        public override bool IsMayor => true;

        public override WerewolfRoleType Type => WerewolfRoleType.SECONDARY;

        public override WerwolfRoleTarget Target => WerwolfRoleTarget.MAYOR;

        public override bool CanVote => false;

        public override string Name { get; } = "Judge";

        public override string Description { get; } = "Decides who gets executed, unless he is voted out.";

        public override void AfterVote(WerwolfGame game, Action callback, WerwolfVotes votes)
        {
            List<WerwolfChoiceOption> choices = votes.Votes.Where(v => v.Value.Count > 0 && v.Key != Player.PlayerID).Select(k => k.Key).Select(l => new WerwolfChoiceOption($"{game.Players.First(p => p.PlayerID == l).Name}/{game.Players.First(p => p.PlayerID == l).Character.Name}", l.ToString())).ToList();

            if (choices.Count > 1)
            {
                game.SendChoice(new WerwolfChoice(
                    Player.PlayerID,
                    game.Host,
                    game, $"Judge_Decision_{game.GameID}_{game.Round}_{Player.PlayerID}",
                    "Who is guilty?",
                    choices,
                    (q, c) =>
                    {
                        if (long.TryParse(c, out long result))
                            game.CurrentVote.Decide(result);

                        base.AfterVote(game, callback, votes);
                    }
                    ));
            }
            else
            {
                if(votes.Tally().Count > 1)
                    game.CurrentVote.Decide(game.CurrentVote.Tally().First(v => v != Player.PlayerID));
                else
                    game.CurrentVote.Decide(game.CurrentVote.Tally().First());

                base.AfterVote(game, callback, votes);
            }
        }
    }




}
