using StardewValley;
using System.Collections.Generic;
using LandGrants.Roles;
using System.Linq;

namespace LandGrants.Game
{
    public class WerwolfPlayer
    {
        public string Name { get; set; }

        public long PlayerID { get; set; }

        public NPC Character { get; set; }

        public bool IsBot { get; set; } = false;

        public bool KilledByWolves { get; set; } = false;

        public bool HasDisconnected { get; set; } = false;

        public string DeathInfo { get; set; } = "";

        public bool IsMayor => Roles.Any(r => r.IsMayor);

        public List<IWerwolfRoleDescription> Roles { get; set; } = new List<IWerwolfRoleDescription>();
        public List<IWerwolfRoleDescription> NewRoles { get; set; } = new List<IWerwolfRoleDescription>();

        public WerwolfPlayer()
        {

        }
        public WerwolfPlayer(string name, long playerID, NPC character, bool isBot, List<IWerwolfRoleDescription> roles)
        {
            Name = name;
            PlayerID = playerID;
            Character = character;
            IsBot = isBot;
            Roles = roles;
        }

        public bool IsAlive
        {
            get => !WasJudged && !WasKilled;
        }

        public bool IsWolf(bool truth)
        {
            return Roles.Any(r => r.IsWolf(truth));
        }

        public void Revive(WerwolfGame game)
        {
            WasJudged = false;
            WasKilled = false;
            KilledByWolves = false;
            DeathInfo = "";
            game.DeathPool.RemoveAll(p => p.PlayerID == PlayerID);
            game.DeathRow.RemoveAll(p => p.PlayerID == PlayerID);
        }

        public void Kill() => WasKilled = true;

        public void Judge() => WasJudged = true;

        public WerwolfClientPlayer GetPlayerInfo(bool withRoles, WerwolfGame game, bool end)
        {
            return WerwolfClientPlayer.FromWerwolfPlayer(game, this, withRoles, end);
        }

        public bool WasJudged { get; set; } = false;

        public bool WasKilled { get; set; } = false;

    }
}
