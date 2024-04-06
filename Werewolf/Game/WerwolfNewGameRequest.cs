namespace LandGrants.Game
{
    public class WerwolfNewGameRequest : WerwolfMPMessage
    {
        public long Host { get; set; }
        public WerwolfUpdate InitialUpdate { get; set; }
        public override string Type { get; set; } = "NewGame";

        public Config Config { get; set; }

        public string GameInfo { get; set; }

        public WerwolfNewGameRequest()
        {

        }

        public WerwolfNewGameRequest(long host, long player, WerwolfGame game, WerwolfUpdate initialUpdate, Config config, string gameInfo)
           : base(player,host,game)
        {
            Host = host;
            InitialUpdate = initialUpdate;
            Config = config;
            GameInfo = gameInfo;
        }
    }
}
