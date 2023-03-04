using System;
using System.Collections.Generic;
using System.Linq;
using Werewolf.Game;
using Werewolf.Roles.Actions;

namespace Werewolf.Roles
{
    public class WerwolfRoleDescriptionLonewolf : WerwolfRoleDescriptionBase
    {
        public WerwolfRoleDescriptionLonewolf(WerwolfPlayer player) : base(player)
        {
        }

        public override WerwolfRoleTarget Target => WerwolfRoleTarget.WOLF;

        public override WerewolfRoleType Type { get; } = WerewolfRoleType.SECONDARY;

        public override string Name { get; } = "Lonewolf";

        public override string Description { get; } = "Does not know who the other Werewolves are. Can only kill villagers when they are the last remaining Werwolf.";

        public override void PreGame(WerwolfGame game, Action callback)
        {
            Player.Roles.FirstOrDefault(r => r is WerwolfRoleDescriptionWolf)?.RoleActions.ForEach(r => r.Deactivate());
            base.PreGame(game, callback);
        }

        public override List<string> KnownRole(WerwolfPlayer player, List<string> roles, bool truth)
        {
            roles = base.KnownRole(player, roles,truth);

            if (player.IsAlive && player.IsWolf(true) && roles.Contains("*Werewolf*"))
                roles.Remove("*Werewolf*");

            return roles;
        }

        public override bool IsWolf(bool truth)
        {
            return true;
        }

        public override void OnDeath(WerwolfGame game, WerwolfPlayer killed)
        {
            if (game.Players.Where(p => p.IsWolf(true) && p.PlayerID != Player.PlayerID).Count() == 0)
                Player.Roles.FirstOrDefault(r => r is WerwolfRoleDescriptionWolf)?.RoleActions.ForEach(r => r.Reactivate());

            base.OnDeath(game, killed);
        }

    }


}
