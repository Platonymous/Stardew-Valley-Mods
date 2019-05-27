using System.Collections.Generic;

namespace ChessBoard
{
    public class SaveData
    {
        public Dictionary<string, Session> Sessions = new Dictionary<string, Session>();
        public List<GameResult> LastSessions = new List<GameResult>();
        public Dictionary<string, LeaderBoardEntry> LeaderBoard = new Dictionary<string, LeaderBoardEntry>();
    }
}
