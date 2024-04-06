using System.Collections.Generic;
using System.Linq;
using LandGrants.Game;

namespace LandGrants.Roles.Actions
{
    public class WerwolfRoleActionWolf : WerwolfRoleActionBase
    {
        public static bool WerwolfHasKillded { get; set; } = false; 

        public override string Name => "Kill";

        public override string Description => "Kills a villager";

        public override bool IsRequired { get; set; } = true;

        public override bool IsActive { get => !WerwolfHasKillded && _isActive; set => _isActive = value; }

        public bool _isActive = true;


        public WerwolfRoleActionWolf(WerwolfPlayer player, IWerwolfRoleDescription role) : base(player, role)
        {
        }

        public override void BotPerform(WerwolfGame game)
        {
            if (!IsActive)
                return;

            List<long> exclude = game.Players.Where(p => p.IsWolf(true)).Select(w => w.PlayerID).ToList();

            if (BotPerformVillagerChoice(game, exclude) is WerwolfPlayer wp)
                Perform(game, wp);
            else
            {
                WerwolfHasKillded = true;
                base.BotPerform(game);
            }
        }

        public override bool CanPerform(WerwolfGame game, WerwolfPlayer onPlayer)
        {
            return base.CanPerform(game, onPlayer) && !onPlayer.IsWolf(true) && !WerwolfHasKillded;
        }

        public override void Perform(WerwolfGame game, WerwolfPlayer onPlayer)
        {
            if (game.Kill(onPlayer, $"{onPlayer.Name}/{onPlayer.Character.Name} was killed by a Werwolf last night.", true))
            {
                WerwolfHasKillded = true;
                game.Players.Where(wp => wp.IsWolf(true)).ToList().ForEach(p =>
                {
                    if (p.Roles.FirstOrDefault(r => r is WerwolfRoleDescriptionWolf wrd)?.RoleActions.FirstOrDefault(a => a is WerwolfRoleActionWolf) is WerwolfRoleActionWolf wra)
                        wra.IsActive = false;

                    game.SendPlayerUpdate(p);
                    game.SendMessage(new WerwolfMessage(p.PlayerID, game.Host, game, WerwolfMessageType.INFO, $"{onPlayer.Name}/{onPlayer.Character.Name} was marked for death.", "Werewolf"));
                });
                base.Perform(game, onPlayer);
                game.SendPlayerUpdate(Player);
            }
            else
            {
                game.SendMessage(new WerwolfMessage(Player.PlayerID, game.Host, game, WerwolfMessageType.INFO, $"{onPlayer.Name}/{onPlayer.Character.Name} could not be killed.", "", "Werewolf"));
                game.SendPlayerUpdate(Player);
                if (Player.IsBot)
                    BotPerform(game);
            }


        }

        public override void AfterRound(WerwolfGame game)
        {
            WerwolfHasKillded = false;
            base.AfterRound(game);
        }
    }
}
