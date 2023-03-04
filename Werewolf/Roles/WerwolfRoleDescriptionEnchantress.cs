using System;
using System.Collections.Generic;
using System.Linq;
using Werewolf.Game;

namespace Werewolf.Roles
{
    public class WerwolfRoleDescriptionEnchantress : WerwolfRoleDescriptionBase
    {
        public WerwolfRoleDescriptionEnchantress(WerwolfPlayer player) : base(player)
        {
        }

        public long CursedPlayer { get; set; }

        public override WerewolfRoleType Type => WerewolfRoleType.SECONDARY;

        public override string Name { get; } = "Enchantress";

        public override string Description { get; } = "Enchants one villager to show the mirror of their true soul.";


        public override void PreGame(WerwolfGame game, Action callback)
        {
            List<WerwolfChoiceOption> choices = game.Players.Where(v => v.PlayerID != Player.PlayerID && v.IsAlive).Select(l => new WerwolfChoiceOption($"{l.Name}/{l.Character.Name}", l.PlayerID.ToString())).ToList();

            game.SendChoice(new WerwolfChoice(
                Player.PlayerID,
                game.Host,
                game, $"Enchantress_Curse_{game.GameID}_{game.Round}_{Player.PlayerID}",
                "Who should be cursed?",
                choices,
                (q, c) =>
                {
                    if (long.TryParse(c, out long result) && game.Players.FirstOrDefault(p => p.PlayerID == result) is WerwolfPlayer cursed)
                    {
                        Player.Roles.Remove(this);
                        cursed.NewRoles.Add(new WerwolfRoleDescriptionCursed(cursed));
                        CursedPlayer = cursed.PlayerID;
                        game.SendMessage(new WerwolfMessage(cursed.PlayerID, game.Host, game, WerwolfMessageType.INFO, "You have been cursed.", "Enchantress"));
                        game.SendPlayerUpdateToAll();
                    }

                    base.PreGame(game, callback);
                }
                ));

            
        }


        public override List<string> KnownRole(WerwolfPlayer player, List<string> roles, bool truth)
        {
            roles = base.KnownRole(player, roles, truth);

            if (player.PlayerID == CursedPlayer && !roles.Contains("*Cursed*"))
                roles.Add("*Cursed*");

            return roles;
        }

    }


}
