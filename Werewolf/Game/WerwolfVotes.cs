using System.Linq;
using System.Collections.Generic;

namespace LandGrants.Game
{
    public class WerwolfVotes
    {
        public Dictionary<long, List<long>> Votes { get; } = new Dictionary<long, List<long>>();
        
        public long Decision { get; set; }

        public int Round { get; set; } = 0;

        public WerwolfVotes()
        {

        }

        public WerwolfVotes(int round)
        {
            Round = round;
        }

        public void AddVote(long vote, long player)
        {
            if (!Votes.ContainsKey(vote))
                Votes.Add(vote, new List<long>());

            if (!Votes[vote].Contains(player))
                Votes[vote].Add(player);
        }

        public List<long> Tally()
        {
            int max = Votes.Max(v => v.Value.Count);
            return Votes.Where(v => v.Value.Count == max).Select(v => v.Key).ToList();
        }

        public void Decide(long player)
        {
            Decision = player;
        }

    }


}
