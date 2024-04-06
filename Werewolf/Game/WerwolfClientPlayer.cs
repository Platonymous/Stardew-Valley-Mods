using System.Collections.Generic;
using System.Linq;
using LandGrants.Roles;

namespace LandGrants.Game
{
    public class WerwolfClientPlayer
    {
        public string Name { get; set; }
        public string Charakter { get; set; }
        public long ID { get; set; }
        public bool Alive { get; set; } = true;

        public bool IsBot { get; set; } = false;

        public bool HasDisconnected { get; set; } = false;
        public List<WerwolfClientRole> Roles { get; set; } = new List<WerwolfClientRole>();

        public Dictionary<long, string> KnownRoles { get; set; } = new Dictionary<long, string>();

        public WerwolfClientPlayer()
        {

        }

        public WerwolfClientPlayer(string name, string charakter, long id, bool alive, List<WerwolfClientRole> roles, Dictionary<long,string> knownRoles, bool isBot = false)
        {
            Name = name;
            Charakter = charakter;
            ID = id;
            Alive = alive;
            Roles = roles;
            IsBot = isBot;
            KnownRoles = knownRoles;
        }

        public static WerwolfClientPlayer FromWerwolfPlayer(WerwolfGame game, WerwolfPlayer player, bool withRoles, bool end)
        { 
            if (withRoles)
            {
                Dictionary<long, string> knownRoles = new Dictionary<long, string>();
                game.Players.ForEach(p =>
                {
                    var roles = new List<string>();

                    if (!end)
                        player.Roles.ForEach(r =>
                        {
                            roles = r.KnownRole(p, roles, false);
                        });
                    else
                        roles.AddRange(p.Roles.Select(r => r.Name));

                    if (roles.Count > 0 && !knownRoles.ContainsKey(p.PlayerID))
                        knownRoles.Add(p.PlayerID, string.Join(',', roles));
                });

                return new WerwolfClientPlayer(
                player.Name,
                player.Character.Name,
                player.PlayerID,
                player.IsAlive,
                player.Roles.Select(
                    role => new WerwolfClientRole(
                        role.Name,
                        role.RoleID,
                        role.RoleActions.Select(
                            action => new WerwolfClientAction(
                                action.Name, action.Description,
                                action.IsActive,
                                action.ID) as WerwolfClientAction).ToList(),
                        role.Description)).ToList(),
                        knownRoles,
                        player.IsBot);
            }
            else
                return new WerwolfClientPlayer(
                player.Name,
                player.Character.Name,
                player.PlayerID,
                player.IsAlive,
                new List<WerwolfClientRole>(),
                new Dictionary<long, string>(),
                player.IsBot)
                ;
        }
    }
}
