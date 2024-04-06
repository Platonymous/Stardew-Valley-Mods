using System;
using System.Linq;
using LandGrants.Game;

namespace LandGrants.Roles
{
    public class WerwolfRoleDescriptionCursed : WerwolfRoleDescriptionBase
    {
        public WerwolfRoleDescriptionCursed(WerwolfPlayer player) : base(player)
        {
        }

        public override WerwolfRoleTarget Target => WerwolfRoleTarget.NONE;
        public override WerewolfRoleType Type => WerewolfRoleType.INGAME;

        public override string Name { get; } = "Cursed";

        public override string Description { get; } = "Till the game ends, the cursed show the mirror of their true soul on death or to the seer.";


        public override void PreGame(WerwolfGame game, Action callback)
        {
            base.PreGame(game, callback);
        }

        public override bool IsWolf(bool truth)
        {
            return !Player.Roles.Where(r => r is not WerwolfRoleDescriptionCursed).Any(wr => wr.IsWolf(false));
        }

    }


}
