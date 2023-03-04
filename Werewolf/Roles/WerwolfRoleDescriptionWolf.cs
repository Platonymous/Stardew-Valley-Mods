using System;
using System.Collections.Generic;
using System.Linq;
using Werewolf.Game;
using Werewolf.Roles.Actions;

namespace Werewolf.Roles
{
    public class WerwolfRoleDescriptionWolf : WerwolfRoleDescriptionBase
    {
        public WerwolfRoleDescriptionWolf(WerwolfPlayer player) : base(player)
        {
        }

        public override string Name { get; } = "Werewolf";

        public override string Description { get; } = "Can kill villagers. Wins when all the villagers are dead.";

        public override void PreGame(WerwolfGame game, Action callback)
        {
            RoleActions.Add(new WerwolfRoleActionWolf(Player, this));
            base.PreGame(game, callback);
        }

        public override List<string> KnownRole(WerwolfPlayer player, List<string> roles, bool truth)
        {
            roles = base.KnownRole(player, roles, truth);

            if (player.IsWolf(true) && !roles.Contains("Werewolf"))
            {
                if (roles.Contains("Villager"))
                {
                    roles.Remove("Villager");
                    if(!roles.Contains("Werewolf") && !roles.Contains("*Werewolf*"))
                        roles.Add("*Werewolf*");
                }
                else
                    if(player.IsAlive && !roles.Contains("Werewolf") && !roles.Contains("*Werewolf*"))
                    roles.Add("*Werewolf*");
                else if (!player.IsAlive && !roles.Contains("Werewolf") && !roles.Contains("*Werewolf*"))
                    roles.Add("Werewolf");
            }

            return roles;
        }

        public override bool IsWolf(bool truth)
        {
            return true;
        }

        public override List<WerwolfPlayer> SetupVote(List<WerwolfPlayer> players)
        {
            if(Player.IsBot)
                players = players.Where(p => !p.IsWolf(true)).ToList();

            return base.SetupVote(players);
        }

        public override void OnDeath(WerwolfGame game, WerwolfPlayer killed)
        {
            if (game.GameIsActive && game.Players.Where(p => p.IsAlive && !p.IsWolf(true)).Count() == 0)
                game.End(game.Wolves, "All villagers are dead. The wolfpack has won.", true);

            base.OnDeath(game, killed);
        }

    }


}
