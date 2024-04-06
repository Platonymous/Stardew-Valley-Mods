using System.Collections.Generic;
using System.Linq;
using LandGrants.Game;

namespace LandGrants.Roles.Actions
{
    public class WerwolfRoleActionWitch : WerwolfRoleActionBase
    {
        public override string Name => "Poison";

        public override string Description => "Poisons a villager";

        public override bool IsUnique => true;

        public WerwolfRoleActionWitch(WerwolfPlayer player, IWerwolfRoleDescription role) : base(player, role)
        {

        }
        public override void BotPerform(WerwolfGame game)
        {
            if (ActionRandom.Next(10) <= game.Round)
                return;

            List<long> exclude = new List<long>();
            if (Role is WerwolfRoleDescriptionWitch witch && witch.Saved != -1)
                exclude.Add(witch.Saved);

            if (BotPerformVillagerChoice(game, exclude) is WerwolfPlayer wp)
                Perform(game, wp);
            else
                base.BotPerform(game);
        }

        public override void Perform(WerwolfGame game, WerwolfPlayer onPlayer)
        {
            string wasWolf = $"{ onPlayer.Character.Name } was a WEREWOLF!";
            string wasVillager = $"{onPlayer.Character.Name} was a VILLAGER!";
            string reveal = onPlayer.IsWolf(false) ? wasWolf : wasVillager;

            if (game.Kill(onPlayer, $"{onPlayer.Name}/{onPlayer.Character.Name} was poisoned last night and died. {reveal}", false))
            {
                base.Perform(game, onPlayer);
                game.SendMessage(new WerwolfMessage(Player.PlayerID, game.Host, game, WerwolfMessageType.INFO, onPlayer.Character.Name + " was poisoned.", "Witch"));
                game.SendPlayerUpdate(Player);
            }
            else
            {
                game.SendMessage(new WerwolfMessage(Player.PlayerID, game.Host, game, WerwolfMessageType.INFO, "Could not poison " + onPlayer.Character.Name, "", "Witch"));
                game.SendPlayerUpdate(Player);
            }
        }
    }
}
