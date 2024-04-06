using System;
using System.Collections.Generic;
using System.Linq;
using LandGrants.Game;

namespace LandGrants.Roles
{
    public class WerwolfRoleDescriptionAmor : WerwolfRoleDescriptionBase
    {
        List<long> Lovers = new List<long>();
        public WerwolfRoleDescriptionAmor(WerwolfPlayer player) : base(player)
        {
        }

        public override WerewolfRoleType Type => WerewolfRoleType.SECONDARY;

        public override string Name { get; } = "Amor";

        public override string Description { get; } = "Chooses two villagers to fall in love. They will die together.";

        public override void PreGame(WerwolfGame game, Action callback)
        {
            List<WerwolfChoiceOption> choices = game.Players.Where(v => v.PlayerID != Player.PlayerID && v.IsAlive).Select(l => new WerwolfChoiceOption($"{l.Name}/{l.Character.Name}", l.PlayerID.ToString())).ToList();
            WerwolfPlayer firstLover = null;
            game.SendChoice(new WerwolfChoice(
                Player.PlayerID,
                game.Host,
                game, $"Amor_First_{game.GameID}_{game.Round}_{Player.PlayerID}",
                "Who should should fall in love?",
                choices,
                (q, c) =>
                {
                    if (long.TryParse(c, out long result) && game.Players.FirstOrDefault(p => p.PlayerID == result) is WerwolfPlayer newLover)
                    {
                        firstLover = newLover;
                        choices.RemoveAll(p => p.ID == c);
                    }

                    game.SendChoice(new WerwolfChoice(
               Player.PlayerID,
               game.Host,
               game, $"Amor_Second_{game.GameID}_{game.Round}_{Player.PlayerID}",
               "With whom should they fall in love?",
               choices,
               (q, c) =>
               {
                   if (long.TryParse(c, out long result) && game.Players.FirstOrDefault(p => p.PlayerID == result) is WerwolfPlayer newLover)
                   {
                       newLover.NewRoles.Add(new WerwolfRoleDescriptionLover(newLover, firstLover));
                       firstLover.NewRoles.Add(new WerwolfRoleDescriptionLover(firstLover, newLover));
                       Lovers.Add(newLover.PlayerID);
                       Lovers.Add(firstLover.PlayerID);
                       game.SendPlayerUpdateToAll();
                       game.SendMessage(new WerwolfMessage(firstLover.PlayerID, game.Host, game, WerwolfMessageType.INFO, "You fell in love with " + newLover.Character.Name, "Amor"));
                       game.SendMessage(new WerwolfMessage(newLover.PlayerID, game.Host, game, WerwolfMessageType.INFO, "You fell in love with " + firstLover.Character.Name, "Amor"));
                   }

                   base.PreGame(game, callback);
               }
               ));
                }
                ));


            
        }

        public override List<string> KnownRole(WerwolfPlayer player, List<string> roles, bool truth)
        {
            roles = base.KnownRole(player, roles, truth);

            if (Lovers.Contains(player.PlayerID) && !roles.Contains("*Lover*"))
                roles.Add("*Lover*");

            return roles;
        }

    }




}
