using StardewModdingAPI;
using StardewValley;
using PlatoTK;
using StardewValley.Objects;

namespace Snake
{
    public class SnakeMod : Mod
    {
        public static IMonitor monitor;
        public static xTile.Tiles.TileSheet ArcadeTilesheet = null;

        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
         
            helper.Events.GameLoop.SaveLoaded += (o, e) =>
            {
                if (Game1.IsMasterGame)
                {
                    SnakeMinigame.HighscoreTable = helper.Data.ReadSaveData<HighscoreList>("Platonymous.SnakeAcrcade.Highscore");
                    if (SnakeMinigame.HighscoreTable == null)
                        SnakeMinigame.HighscoreTable = new HighscoreList();

                    Monitor.Log("Loading Highscores");

                    foreach (Highscore h in SnakeMinigame.HighscoreTable.Entries)
                        Monitor.Log(h.Name + ": " + h.Value);
                }
            };

            helper.Events.GameLoop.Saving += (o, e) =>
            {
                if (Game1.IsMasterGame)
                    helper.Data.WriteSaveData<HighscoreList>("Platonymous.SnakeAcrcade.Highscore", SnakeMinigame.HighscoreTable);
            };

            helper.Events.Multiplayer.PeerContextReceived += (s, e) =>
            {
                if (Game1.IsMasterGame)
                    Helper.Multiplayer.SendMessage<HighscoreList>(SnakeMinigame.HighscoreTable, "HighscoreList", new string[] { "Platonymous.Snake" }, new long[] { e.Peer.PlayerID });
            };

            helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            if (Helper.ModRegistry.GetApi<PlatoTK.APIs.ISerializerAPI>("Platonymous.Toolkit") is PlatoTK.APIs.ISerializerAPI pytk)
            {
                pytk.AddPostDeserialization(ModManifest, (o) =>
                {
                    var data = pytk.ParseDataString(o);

                    if (o is Chest c && data.ContainsKey("@Type") && data["@Type"].Contains("SnakeMachine"))
                    {
                        return SnakeMachine.GetNew(c);
                    }

                    return o;
                });
            }

            Helper.GetPlatoHelper().Presets.RegisterArcade(
                id: "Snake",
                name: "Snake",
                objectName: "Snake Arcade Machine",
                start: () => SnakeMachine.start(Helper),
                sprite: Helper.ModContent.GetInternalAssetName("assets/arcade.png").Name,
                iconForMobilePhone: Helper.ModContent.GetInternalAssetName("assets/mobile_app_icon.png").Name
            );
        }

        private void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (e.Type == "HighscoreList")
            {
                var list = e.ReadAs<HighscoreList>();
                foreach (var score in list.Entries)
                    SnakeMinigame.setScore(Helper, score.Name, score.Value, false);
            }
            else if (e.Type == "Highscore")
            {
                var score = e.ReadAs<Highscore>();
                Snake.SnakeMinigame.setScore(Helper, score.Name, score.Value, false);
            }
        }        
    }
}
