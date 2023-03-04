using System;
using System.Collections.Generic;
using System.Linq;
using Werewolf.Game;
using Werewolf.Roles.Actions;

namespace Werewolf.Roles
{
    public class WerwolfRoleDescriptionSeer : WerwolfRoleDescriptionBase
    {
        public List<WerwolfPlayer> SeenWolves { get; set; } = new List<WerwolfPlayer>();
        public List<WerwolfPlayer> SeenVillagers { get; set; } = new List<WerwolfPlayer>();

        public WerwolfRoleDescriptionSeer(WerwolfPlayer player) : base(player)
        {
        }

        public override WerewolfRoleType Type => WerewolfRoleType.SECONDARY;

        public override string Name { get; } = "Seer";

        public override string Description { get; } = "Can see into the Soul of a villager each day to determine if they are a werewolf.";


        public override void PreGame(WerwolfGame game, Action callback)
        {
            RoleActions.Add(new WerwolfRoleActionSeer(Player, this));
            base.PreGame(game, callback);
        }

        public override List<string> KnownRole(WerwolfPlayer player, List<string> roles, bool truth)
        {
            roles = base.KnownRole(player, roles, truth);

            if (Player.IsAlive && SeenWolves.Any(w => w.PlayerID == player.PlayerID) && !roles.Contains("*Werewolf*"))
                roles.Add("*Werewolf*");


            if (Player.IsAlive && SeenVillagers.Any(w => w.PlayerID == player.PlayerID) && !roles.Contains("*Villager*"))
                roles.Add("*Villager*");

            return roles;
        }

    }


}
