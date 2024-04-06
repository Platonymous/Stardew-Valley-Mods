using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using PlatoTK;

namespace CustomTV
{
    public class CustomTVMod : Mod
    {
        private Dictionary<string, string> Today = new Dictionary<string, string>();
        internal static IPlatoHelper PlatoHelper;
        public override void Entry(IModHelper helper)
        {
            PlatoHelper = helper.GetPlatoHelper();
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += (s, e) => Today.Clear();
            Helper.Events.GameLoop.ReturnedToTitle += (s, e) => Today.Clear();
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = Helper.ModRegistry.GetApi<PlatoTK.APIs.IContentPatcher>("Pathoschild.ContentPatcher");
            api.RegisterToken(ModManifest, "Channel", new CustomTVChannelToken());
            api.RegisterToken(ModManifest, "Screen", new CustomTVScreenToken());

            PlatoHelper.Events.QuestionRaised += (q, p) =>
            {
                if (p.IsTV)
                {
                    bool added = false;

                    foreach (string channel in CustomTVChannelToken.Channels.Keys.ToList())
                    {
                        Dictionary<string, string> channelData = Helper.GameContent.Load<Dictionary<string, string>>($"{CustomTVChannelToken.ChannelDataPrefix}{channel}");

                        if (channelData.ContainsKey("@Active") && channelData["@Active"].ToLower() == "false")
                            continue;

                        if (CustomTVChannelToken.Channels[channel] == null)
                        {
                            added = true;
                            CustomTVChannelToken.Channels[channel] = new TVChannel()
                            {
                                Id = channel,
                                Name = channelData.ContainsKey("@Name") ? channelData["@Name"] : "Missing Name",
                                Intro = "Missing Intro",
                            };


                            if (channelData.ContainsKey("@Intro"))
                            {
                                GetShowData(channelData["@Intro"], out string text, out string screen, out string overlay, out string music);

                                CustomTVChannelToken.Channels[channel].Intro = text;
                                CustomTVChannelToken.Channels[channel].IntroMusic = music;

                                if (!string.IsNullOrEmpty(screen) && CustomTVChannelToken.GetScreenId(channel, screen) is string screenId
                                && CustomTVChannelToken.Screens.ContainsKey(screenId)
                                && CustomTVChannelToken.Screens[screenId] is TVScreen tvs)
                                    CustomTVChannelToken.Channels[channel].IntroScreen =
                                new TemporaryAnimatedSprite(
                                tvs.Path,
                                new Rectangle(tvs.SourceBounds[0], tvs.SourceBounds[1], tvs.SourceBounds[2], tvs.SourceBounds[3]),
                                tvs.FrameDuration, tvs.Frames, int.MaxValue, Vector2.Zero, false, false, 0f, 0.0f, Color.White, 1f,
                                0.0f, 0.0f, 0.0f, false);

                                if (!string.IsNullOrEmpty(overlay) && CustomTVChannelToken.GetScreenId(channel, overlay) is string overlayId
                                && CustomTVChannelToken.Screens.ContainsKey(overlayId)
                                && CustomTVChannelToken.Screens[overlayId] is TVScreen os)
                                    CustomTVChannelToken.Channels[channel].IntroOverlay =
                                new TemporaryAnimatedSprite(
                                os.Path,
                                new Rectangle(os.SourceBounds[0], os.SourceBounds[1], os.SourceBounds[2], os.SourceBounds[3]),
                                os.FrameDuration, os.Frames, int.MaxValue, Vector2.Zero, false, false, 0f, 0.0f, Color.White, 1f,
                                0.0f, 0.0f, 0.0f, false);
                            }


                            if (channelData.ContainsKey("@Seasons"))
                                CustomTVChannelToken.Channels[channel].Seasons = channelData["@Seasons"].Split(' ');

                            if (channelData.ContainsKey("@Days"))
                                CustomTVChannelToken.Channels[channel].Days = channelData["@Days"].Split(' ');

                            if (channelData.ContainsKey("@Order"))
                                CustomTVChannelToken.Channels[channel].Random = channelData["@Order"].ToLower() == "random";
                        }

                        if (CustomTVChannelToken.Channels[channel] is TVChannel tvc && tvc.Seasons.Any(s => s.ToLower() == Game1.currentSeason.ToLower())
                            && tvc.Days.Any(d => d.ToLower() == Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).ToLower()))
                            p.AddResponse(new Response(channel, CustomTVChannelToken.Channels[channel].Name));
                    }

                    if (added)
                    {
                        p.PaginateResponses();

                    }
                }
            };

            PlatoHelper.Events.TVChannelSelected += (s, p) =>
            {
                if (CustomTVChannelToken.Channels.ContainsKey(p.ChannelName) 
                && CustomTVChannelToken.Channels[p.ChannelName] is TVChannel channel
                && Helper.GameContent.Load<Dictionary<string, string>>($"{CustomTVChannelToken.ChannelDataPrefix}{p.ChannelName}") is Dictionary<string,string> channelData
                )
                {
                    if (TryPickShow(channelData, p.ChannelName,out string text, out string screen, out string overlay, out string music, channel.Random))
                    {
                        if (channel.IntroScreen != null)
                        {
                            channel.IntroScreen.Position = new Vector2(
                                                            p.ScreenPosition.X + (channel.IntroScreenOffset[0] * p.Scale),
                                                            p.ScreenPosition.Y + (channel.IntroScreenOffset[1] * p.Scale));
                            channel.IntroScreen.layerDepth = p.ScreenLayerDepth;
                            channel.IntroScreen.scale = p.Scale;
                        }

                        if (channel.IntroOverlay != null)
                        {
                            channel.IntroOverlay.Position = new Vector2(
                                                            p.ScreenPosition.X + (channel.IntroOverlayOffset[0] * p.Scale),
                                                            p.ScreenPosition.Y + (channel.IntroOverlayOffset[1] * p.Scale));
                            channel.IntroOverlay.layerDepth = p.OverlayLayerDepth;
                            channel.IntroOverlay.scale = p.Scale;
                        }

                        string cMusic = Game1.getMusicTrackName();
                        bool changeMusic = false;
                        if (!string.IsNullOrEmpty(channel.IntroMusic) && channel.IntroMusic != "none")
                        {
                            changeMusic = true;
                            try
                            {
                                Game1.changeMusicTrack(channel.IntroMusic);
                            }
                            catch
                            {

                            }
                        }

                        p.ShowScene(channel.IntroScreen, channel.IntroOverlay, channel.Intro, () =>
                          {
                              TemporaryAnimatedSprite screenSprite = channel.IntroScreen;
                              TemporaryAnimatedSprite overlaySprite = null;

                              if (!string.IsNullOrEmpty(screen) && CustomTVChannelToken.GetScreenId(p.ChannelName, screen) is string screenId
                              && CustomTVChannelToken.Screens.ContainsKey(screenId)
                              && CustomTVChannelToken.Screens[screenId] is TVScreen tvs)
                                  screenSprite  =
                              new TemporaryAnimatedSprite(
                              tvs.Path,
                              new Rectangle(tvs.SourceBounds[0], tvs.SourceBounds[1], tvs.SourceBounds[2], tvs.SourceBounds[3]),
                              tvs.FrameDuration, tvs.Frames, int.MaxValue, 
                              new Vector2(p.ScreenPosition.X + (tvs.Offset[0] * p.Scale),p.ScreenPosition.Y + (tvs.Offset[1] * p.Scale))
                              , false, false, p.ScreenLayerDepth, 0.0f, Color.White, p.Scale,
                              0.0f, 0.0f, 0.0f, false);

                              if (!string.IsNullOrEmpty(overlay) && CustomTVChannelToken.GetScreenId(p.ChannelName, overlay) is string overlayId
                              && CustomTVChannelToken.Screens.ContainsKey(overlayId)
                              && CustomTVChannelToken.Screens[overlayId] is TVScreen os)
                                  overlaySprite =
                              new TemporaryAnimatedSprite(
                              os.Path,
                              new Rectangle(os.SourceBounds[0], os.SourceBounds[1], os.SourceBounds[2], os.SourceBounds[3]),
                              os.FrameDuration, os.Frames, int.MaxValue,
                              new Vector2(p.ScreenPosition.X + (os.Offset[0] * p.Scale), p.ScreenPosition.Y + (os.Offset[1] * p.Scale))
                              , false, false, p.OverlayLayerDepth, 0.0f, Color.White, p.Scale,
                              0.0f, 0.0f, 0.0f, false);

                              if (!string.IsNullOrEmpty(music) && music != "none")
                              {
                                  changeMusic = true;
                                  try
                                  {
                                      Game1.changeMusicTrack(music);
                                  }
                                  catch
                                  {

                                  }
                              }

                              p.ShowScene(screenSprite, overlaySprite, text, () => {
                                  try
                                  {
                                      if (changeMusic)
                                          Game1.changeMusicTrack(cMusic);
                                  }
                                  catch
                                  {

                                  }
                                  p.TurnOffTV();
                              });
                          });
                    }

                    p.PreventDefault();
                }
            };
        }

        private string GetMailFlag(string channelid, string showid, bool rewatch = false)
        {
            return $"CustomTV_{channelid}_{showid}{(rewatch ? "_Rewatch" : "")}";
        }

        private bool TryPickShow(Dictionary<string, string> channelData, string channelId, out string text, out string screen, out string overlay, out string music, bool random = false, bool rewatch = false, bool check = false)
        {
            if(Today.ContainsKey(channelId) && channelData.ContainsKey(Today[channelId]))
            {
                GetShowData(channelData[Today[channelId]], out text, out screen, out overlay, out music);
                return true;
            }

            if(Today.ContainsKey(channelId))
                Today.Remove(channelId);

            Random r = new Random((int)Game1.stats.daysPlayed + (int)Game1.player.UniqueMultiplayerID + Game1.player.LuckLevel);

            Func<string, bool> p = (k) =>
                        !k.StartsWith("@")
                        && !Game1.MasterPlayer.hasOrWillReceiveMail(GetMailFlag(channelId,k, rewatch));

            if (!random && channelData.Keys.FirstOrDefault(p)
                is string show && !string.IsNullOrEmpty(show))
            {
                GetShowData(channelData[show], out text, out screen, out overlay, out music);
                Today.Add(channelId, show);
                string mailId = GetMailFlag(channelId, show, rewatch);
                if (!Game1.MasterPlayer.mailReceived.Contains(mailId))
                    Game1.MasterPlayer.mailReceived.Add(mailId);
                return true;
            } else if(random && channelData.Keys.Where(p) is IEnumerable<string> keys && keys.Count() is int keyCount && keyCount > 0) {
                string rKey = keys.Skip(r.Next(0, keyCount - 1)).First();
                GetShowData(channelData[rKey], out text, out screen, out overlay, out music);
                Today.Add(channelId, rKey);
                string mailId = GetMailFlag(channelId, rKey, rewatch);
                if(!Game1.MasterPlayer.mailReceived.Contains(mailId))
                    Game1.MasterPlayer.mailReceived.Add(mailId);
                return true;
            }

            if (!rewatch)
            {
                string mailId = GetMailFlag(channelId, "All", false);
                bool startRewatch = TryPickShow(channelData, channelId, out text, out screen, out overlay, out music, random, true);
                if (!Game1.MasterPlayer.mailReceived.Contains(mailId) && startRewatch)
                    Game1.MasterPlayer.mailReceived.Add(mailId);

                return startRewatch;
            }
            else if (!check)
            {
                Game1.MasterPlayer.mailReceived
                    .Where(m => m.StartsWith($"CustomTV_{channelId}") && m.EndsWith("_Rewatch"))
                    .ToList().ForEach(m => Game1.MasterPlayer.mailReceived.Remove(m));
                return TryPickShow(channelData, channelId, out text, out screen, out overlay, out music, random, true, true);
            }

            text = "Missing Text";
            music = "none";
            screen = null;
            overlay = null;

            return false;
        }

        private void GetShowData(string dataString, out string text, out string screen, out string overlay, out string music)
        {
            string[] parts = dataString.Split('/');
            text = parts[0];
            screen = null;
            overlay = null;
            music = "none";
            if (parts.Length <= 1)
                return;

            string[] screens = parts[1].Split(' ');
            screen = screens[0];
            overlay = screens.Length > 1 ? screens[1] : null;

            if (parts.Length > 2)
                music = parts[2].Trim();
        }
    }
}
