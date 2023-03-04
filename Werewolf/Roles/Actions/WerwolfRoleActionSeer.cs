using System.Collections.Generic;
using System.Linq;
using Werewolf.Game;

namespace Werewolf.Roles.Actions
{
    public class WerwolfRoleActionSeer : WerwolfRoleActionBase
    {
        public override string Name => "See";

        public override string Description => "Reveals if a villager is a Werwolf";

        public override bool IsRequired { get; set; } = false;

        public WerwolfRoleActionSeer(WerwolfPlayer player, IWerwolfRoleDescription role) : base(player, role)
        {
        }

        public override void BotPerform(WerwolfGame game)
        {
            List<long> exclude = new List<long>();
            if (Role is WerwolfRoleDescriptionSeer seer) {
                exclude.AddRange(seer.SeenWolves.Select(s => s.PlayerID));
                exclude.AddRange(seer.SeenVillagers.Select(s => s.PlayerID));
            }

            if (BotPerformVillagerChoice(game, exclude) is WerwolfPlayer wp)
                Perform(game, wp);
            else
                base.BotPerform(game);
        }

        public override void Perform(WerwolfGame game, WerwolfPlayer onPlayer)
        {
            string isWolf = $"{onPlayer.Name}({onPlayer.Character.Name}) is a WERWOLF!";
            string isVillager = $"{onPlayer.Name}({onPlayer.Character.Name}) is a VILLAGER.";

            if (Role is WerwolfRoleDescriptionSeer seer)
                if (onPlayer.IsWolf(false) && !seer.SeenWolves.Contains(onPlayer))
                    seer.SeenWolves.Add(onPlayer);
                else if (!onPlayer.IsWolf(false) && !seer.SeenVillagers.Contains(onPlayer))
                    seer.SeenVillagers.Add(onPlayer);

            game.SendMessage(new WerwolfMessage(Player.PlayerID, game.Host, game, WerwolfMessageType.INFO, onPlayer.IsWolf(false) ? isWolf : isVillager, "You SEE"));
            base.Perform(game, onPlayer);
            game.SendPlayerUpdate(Player);
        }
    }
}
