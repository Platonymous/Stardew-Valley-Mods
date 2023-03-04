using System;
using System.Collections.Generic;
using System.Linq;
using Werewolf.Game;

namespace Werewolf.Roles
{
    public class WerwolfRoleDescriptionLover : WerwolfRoleDescriptionBase
    {
        public WerwolfRoleDescriptionLover(WerwolfPlayer player, WerwolfPlayer otherLover) : base(player)
        {
            OtherLover = otherLover;
        }

        public override WerewolfRoleType Type => WerewolfRoleType.INGAME;

        public override string Name { get; } = "Lover";

        public override WerwolfRoleTarget Target => WerwolfRoleTarget.NONE;

        public WerwolfPlayer OtherLover { get; set; }

        public override string Description { get; } = "Will die with their lover. Wins when the lovers survive.";

        public override void PreGame(WerwolfGame game, Action callback)
        {
            base.PreGame(game, callback);
        }

        public override List<WerwolfPlayer> SetupVote(List<WerwolfPlayer> players)
        {
            players = base.SetupVote(players);
            players.RemoveAll(p => p.Roles.Any(r => r is WerwolfRoleDescriptionLover));
            return players;
        }

        public override List<string> KnownRole(WerwolfPlayer player, List<string> roles, bool truth)
        {
            roles = base.KnownRole(player, roles, truth);

            if (player.Roles.Any(r => r is WerwolfRoleDescriptionLover) && !roles.Contains("*Lover*") && !roles.Contains("Lover"))
                roles.Add("*Lover*");

            return roles;
        }

        public override void OnDeath(WerwolfGame game, WerwolfPlayer killed)
        {
            if(Player.IsAlive && killed.PlayerID == OtherLover.PlayerID)
                game.Kill(Player, $"{Player.Name}/{Player.Character.Name} died from grief.", false);

            if (game.GameIsActive && Player.IsWolf(true) != OtherLover.IsWolf(true) && game.Players.Where(p => p.IsAlive).Count() == 2 && Player.IsAlive && OtherLover.IsAlive)
                game.End(new List<WerwolfPlayer>() { Player, OtherLover }, "Only the Lovers remain.", true);

            base.OnDeath(game, killed);
        }

    }


}
