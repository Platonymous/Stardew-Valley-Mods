using System;
using System.Collections.Generic;
using System.Linq;
using Werewolf.Game;

namespace Werewolf.Roles
{
    public class WerwolfRoleDescriptionMayor : WerwolfRoleDescriptionBase
    {
        public WerwolfRoleDescriptionMayor(WerwolfPlayer player) : base(player)
        {
        }

        public override bool IsMayor => true;

        public override WerewolfRoleType Type => WerewolfRoleType.SECONDARY;

        public override WerwolfRoleTarget Target => WerwolfRoleTarget.MAYOR;

        public override string Name { get; } = "Mayor";

        public override string Description { get; } = "Decides who gets executed in case of a draw in the vote.";

        public override void AfterVote(WerwolfGame game, Action callback, WerwolfVotes votes)
        {
            if (votes.Tally().Count > 1)
            {
                List<WerwolfChoiceOption> choices = votes.Tally().Where(wp => wp != Player.PlayerID).Select(l => new WerwolfChoiceOption($"{game.Players.First(p => p.PlayerID == l).Name}/{game.Players.First(p => p.PlayerID == l).Character.Name}", l.ToString())).ToList();
                if (choices.Count > 1)
                {
                    game.SendChoice(new WerwolfChoice(
                        Player.PlayerID,
                        game.Host,
                        game, $"Mayor_Decision_{game.GameID}_{game.Round}_{Player.PlayerID}",
                        "Who should be executed?",
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
                    game.CurrentVote.Decide(long.Parse(choices.First().ID));
                    base.AfterVote(game, callback, votes);
                }
            }
            else
            {
                game.CurrentVote.Decide(game.CurrentVote.Tally().First());
                base.AfterVote(game, callback, votes);
            }
        }

    }




}
