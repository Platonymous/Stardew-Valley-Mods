using System;
using System.Collections.Generic;
using System.Linq;
using Werewolf.Game;

namespace Werewolf.Roles.Actions
{
    public abstract class WerwolfRoleActionBase : IWerwolfRoleAction
    {
        public virtual string ID { get; }

        public virtual string Name { get; }

        public virtual string Description { get; }

        public virtual IWerwolfRoleDescription Role { get; set; }

        public virtual WerwolfPlayer Player { get; set; }

        public virtual Random ActionRandom { get; set; }

        public virtual bool IsActive { get; set; } = true;

        public virtual bool IsUnique { get; set; } = false;

        public virtual bool IsRequired { get; set; } = false;

        public virtual bool IsDeaktivated { get; set; }


        public WerwolfRoleActionBase(WerwolfPlayer player, IWerwolfRoleDescription role)
        {
            ActionRandom = new Random();
            Player = player;
            Role = role;
        }

        public virtual void Deactivate()
        {
            IsDeaktivated = true;
            IsActive = false;
        }

        public virtual void Reactivate()
        {
            IsDeaktivated = false;
            IsActive = true;
        }

        public virtual bool CanPerform(WerwolfGame game, WerwolfPlayer onPlayer)
        {
            return Player.IsAlive && IsActive && onPlayer.IsAlive;
        }

        public virtual IWerwolfRoleAction GetAssigned(WerwolfPlayer player, IWerwolfRoleDescription role)
        {
            return (IWerwolfRoleAction)GetType().GetConstructor(new Type[] { typeof(WerwolfPlayer), typeof(IWerwolfRoleDescription) }).Invoke(new object[] { player, role });
        }

        public virtual void Perform(WerwolfGame game, WerwolfPlayer onPlayer)
        {
            IsActive = false;
        }

        public virtual WerwolfPlayer BotPerformVillagerChoice(WerwolfGame game, List<long> exclude)
        {
            List<List<string>> tiers = new List<List<string>>();

            List<string> tier1 = new List<string>();
            List<string> tier2 = new List<string>();

            game.PastVotes.ForEach(v =>
            {
                if (v.Votes.TryGetValue(Player.PlayerID, out List<long> values))
                    tier1.AddRange(values.Where(v =>
                    {
                        var player = game.Players.FirstOrDefault(p => p.PlayerID == v);
                        return !exclude.Contains(player.PlayerID) && player.PlayerID != Player.PlayerID && player.IsAlive;
                    }).Select(l => l.ToString()));
            });

            tier2.AddRange(game.Players.Where(player => !exclude.Contains(player.PlayerID) && player.PlayerID != Player.PlayerID && player.IsAlive).Select(l => l.PlayerID.ToString()));

            tiers.Add(tier1);
            tiers.Add(tier2);
            var answer = BotChoice(tiers.ToArray());

            if (!string.IsNullOrEmpty(answer) && long.TryParse(answer, out long pid) && game.Players.FirstOrDefault(p => p.PlayerID == pid) is WerwolfPlayer wp)
                return wp;
            else
                return null;
        }

        public virtual void BotPerform(WerwolfGame game)
        {
            IsActive = false;
        }

        public virtual string BotChoice(params List<string>[] tiers)
        {
            string answer = null;
            tiers.ToList().ForEach(t =>
            {
                if (t.Count > 0 && ActionRandom.Next(10) >= 6)
                    answer = t[ActionRandom.Next(t.Count)];
            });

            if (string.IsNullOrEmpty(answer) && tiers.First().Count > 0)
                answer = tiers.First()[ActionRandom.Next(tiers.First().Count)];

            if (string.IsNullOrEmpty(answer) && tiers.Last().Count > 0)
                answer = tiers.Last()[ActionRandom.Next(tiers.Last().Count)];

            return answer;
        }

        public virtual void AfterRound(WerwolfGame game)
        {
            if (!IsUnique && !IsDeaktivated)
                IsActive = true;
        }
    }
}
