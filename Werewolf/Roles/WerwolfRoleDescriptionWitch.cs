using System;
using System.Collections.Generic;
using System.Linq;
using Werewolf.Game;
using Werewolf.Roles.Actions;

namespace Werewolf.Roles
{
    public class WerwolfRoleDescriptionWitch : WerwolfRoleDescriptionBase
    {
        public long Saved { get; set; } = -1;

        public WerwolfRoleDescriptionWitch(WerwolfPlayer player) : base(player)
        {
        }

        public override WerewolfRoleType Type => WerewolfRoleType.SECONDARY;

        public bool HasRevived { get; set; } = false;

        public override string Name { get; } = "Witch";

        public override string Description { get; } = "Can save a villager from being killed by Werewolves once, as well as poison one person.";


        public override void PreGame(WerwolfGame game, Action callback)
        {
            RoleActions.Add(new WerwolfRoleActionWitch(Player, this));
            base.PreGame(game, callback);
        }

        public override void BeforeKills(WerwolfGame game, List<long> players, Action callback)
        {
            if (!HasRevived && game.Players.FirstOrDefault(p => players.Contains(p.PlayerID) && p.KilledByWolves) is WerwolfPlayer wp)
                game.SendChoice(new WerwolfChoice(
                    Player.PlayerID, game.Host,
                    game
                    , $"Witch_Save_{game.GameID}_{game.Round}_{Player.PlayerID}",
                    $"{wp.Name}/{wp.Character.Name} was attacked by Wolves last night. Do you use your healing power?",
                    new List<WerwolfChoiceOption>() { new WerwolfChoiceOption("Yes", "yes"), new WerwolfChoiceOption("No", "no") },
                    (s, c) =>
                    {
                        if (c == "yes")
                        {
                            wp.Revive(game);
                            Saved = wp.PlayerID;
                            HasRevived = true;
                        }

                        base.BeforeKills(game, players, callback);
                    }
                    ));
            else
                base.BeforeKills(game, players, callback);
        }


        public override List<string> KnownRole(WerwolfPlayer player, List<string> roles, bool truth)
        {
            roles = base.KnownRole(player, roles, truth);

            if (Saved != -1 && player.PlayerID == Saved && !roles.Contains("Villager") && !roles.Contains("Werewolf"))
                roles.Add("Villager");

            return roles;
        }
    }


}
