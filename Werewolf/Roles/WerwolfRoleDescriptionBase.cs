using System;
using System.Collections.Generic;
using System.Linq;
using LandGrants.Game;
using LandGrants.Roles.Actions;

namespace LandGrants.Roles
{
    public abstract class WerwolfRoleDescriptionBase : IWerwolfRoleDescription
    {

        public virtual string RoleID { get; } = "Platonymous.Werewolf.Villager";

        public virtual string RoleVersion { get; } = "1.0.0";

        public virtual string ModID { get; } = "Platonymous.Werewolf";

        public virtual bool IsMayor { get; } = false;

        public virtual bool CanVote { get; } = true;

        public abstract string Name { get; }

        public abstract string Description { get; }

        public virtual bool Remove { get; set; } = false;

        public virtual WerewolfRoleType Type { get; } = WerewolfRoleType.PRIMARY;

        public virtual WerwolfRoleTarget Target { get; } = WerwolfRoleTarget.VILLAGER;

        public virtual WerwolfPlayer Player { get; set; }

        public virtual List<IWerwolfRoleAction> RoleActions { get; } = new List<IWerwolfRoleAction>();

        public WerwolfRoleDescriptionBase(WerwolfPlayer player)
        {
            Player = player;
        }

        public virtual IWerwolfRoleDescription GetAssigned(WerwolfPlayer player)
        {
            return (IWerwolfRoleDescription)GetType().GetConstructor(new Type[] { typeof(WerwolfPlayer) }).Invoke(new[] { player });
        }

        public virtual bool ReadyToProgress()
        {
            return RoleActions.All(r => !r.IsRequired || !r.IsActive);
        }

        public virtual void PreGame(WerwolfGame game, Action callback)
        {
            callback();
        }

        public virtual void OnDeath(WerwolfGame game, WerwolfPlayer killed)
        {

        }

        public virtual List<string> KnownRole(WerwolfPlayer player, List<string> roles, bool truth)
        {
            if (!player.IsAlive)
                if (player.IsWolf(truth) && !roles.Contains("Werewolf"))
                    roles.Add("Werewolf");
                else if (!player.IsWolf(truth) && !roles.Contains("Villager"))
                    roles.Add("Villager");

            if (player.PlayerID == Player.PlayerID)
            {
                if (player.IsWolf(true) && !roles.Contains("Werewolf"))
                    roles.Add("Werewolf");
                else if (!player.IsWolf(true) && !roles.Contains("Villager"))
                    roles.Add("Villager");

                Player.Roles.ForEach(r =>
                {
                    if (!roles.Contains(r.Name))
                        roles.Add(r.Name);
                });
            }

            if (player.Roles.FirstOrDefault(r => r.IsMayor) is IWerwolfRoleDescription mayor && !roles.Contains(mayor.Name))
                roles.Add(mayor.Name);

            return roles;

        }

        public virtual void BeforeKills(WerwolfGame game, List<long> players, Action callback)
        {
            callback();
        }

        public virtual List<WerwolfPlayer> SetupVote(List<WerwolfPlayer> players)
        {
            return players.Where(p => p.PlayerID != Player.PlayerID).ToList();
        } 

        public virtual bool IsWolf(bool truth)
        {
            return false;
        }

        public virtual void AfterVote(WerwolfGame game, Action callback, WerwolfVotes votes)
        {
            callback();
        }
    }


}
