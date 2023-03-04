using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using Werewolf.Roles;

namespace Werewolf.Game
{
    public class WerwolfGame
    {
        public static WerwolfGame CurrentGame { get; set; }

        public static List<WerwolfClientGame> BotGames { get; set; } = new List<WerwolfClientGame>();

        public static List<IWerwolfRoleDescription> WerwolfRoles { get; set; } = new List<IWerwolfRoleDescription>();
        public static List<IWerwolfRoleDescription> VillagerRoles { get; set; } = new List<IWerwolfRoleDescription>();
        public static List<IWerwolfRoleDescription> MayorRoles { get; set; } = new List<IWerwolfRoleDescription>();

        public static NPC Fenris { get; set; }

        public int Round { get; set; } = -1;

        public bool Started { get; set; } = false;

        public string GameID { get; set; } = "Platonymous.Werewolf";

        public bool GameIsActive { get; set; } = false;

        public long Host { get; set; }

        public Random HostRandom { get; set; }

        public List<WerwolfPlayer> Players { get; set; } = new List<WerwolfPlayer>();

        public WerwolfPlayer Mayor { get; set; }

        public List<WerwolfPlayer> Wolves => Players.Where(p => p.IsWolf(true)).ToList();

        public List<WerwolfPlayer> Villagers => Players.Where(p => !p.IsWolf(true)).ToList();

        public List<WerwolfPlayer> DeathPool { get; set; } = new List<WerwolfPlayer>();

        public List<WerwolfPlayer> DeathRow { get; set; } = new List<WerwolfPlayer>();

        public IModHelper Helper { get; }

        public IMonitor Monitor { get; }

        public Config Config { get; }

        private int PhaseProgress { get; set; } = 0;

        private int PhaseStep { get; set; } = 0;

        private List<Action> Phases { get; set; } = new List<Action>();

        private Action ProgressPhaseCallback { get; set; }

        public List<WerwolfCallbackRequest> Callbacks { get; } = new List<WerwolfCallbackRequest>();

        public WerwolfVotes CurrentVote { get; set; } = new WerwolfVotes(0);

        public List<WerwolfVotes> PastVotes { get; set; } = new List<WerwolfVotes>();

        public string GameInfo { get; set; } = "";

        public object locked = new object();

        public List<WerwolfPlayer> Winners { get; set; } = new List<WerwolfPlayer>();

        public string WinMessage { get; set; } = "";

        public bool WolvesWon { get; set; } = false;

        public WerwolfGame()
        {

        }

        public static void AddAvailableRole(IWerwolfRoleDescription role)
        {
            if(role.Type != WerewolfRoleType.INGAME)
            {
                if (role.Target == WerwolfRoleTarget.VILLAGER)
                    VillagerRoles.Add(role);
                else if (role.Target == WerwolfRoleTarget.WOLF)
                    WerwolfRoles.Add(role);
                else if (role.Target == WerwolfRoleTarget.MAYOR)
                    MayorRoles.Add(role);
            }
        }

        public static void LoadBasicRoles()
        {
            AddAvailableRole(new WerwolfRoleDescriptionAmor(null));
            AddAvailableRole(new WerwolfRoleDescriptionEnchantress(null));
            AddAvailableRole(new WerwolfRoleDescriptionJudge(null));
            AddAvailableRole(new WerwolfRoleDescriptionLonewolf(null));
            AddAvailableRole(new WerwolfRoleDescriptionMayor(null));
            AddAvailableRole(new WerwolfRoleDescriptionSeer(null));
            AddAvailableRole(new WerwolfRoleDescriptionWitch(null));
        }

        public WerwolfGame(IModHelper helper, IMonitor monitor, long host, Config config)
        {
            BotGames.Clear();
            Helper = helper;
            GameIsActive = true;
            Monitor = monitor;
            GameID = "Platonymous.Werewolf." + Game1.stats.DaysPlayed + "." + Game1.stats.stepsTaken;
            Host = host;
            Phases = new List<Action>() { () => {
                Round = Math.Max(Round,0);
                SendPlayerUpdateToAll();
            }, () => BeforeKills(), () => AfterKills(), () => BeforeVote(), () => Vote(), () => AfterVote(), () => AfterJudgement(), () => AfterRound() };
            ProgressPhaseCallback = () => ProgressPhase("callback");    
            CurrentGame = this;
            Config = config;
            HostRandom = new Random();

            StartGame();
        }

        public static List<long> GetAllPeers(IModHelper helper, Config config)
        {
            var list = helper.Multiplayer.GetConnectedPlayers().Select(p => p.PlayerID).ToList();
            list.Add(Game1.player.UniqueMultiplayerID);
            return list;
        }

        public static long GetHostPeer(IModHelper helper)
        {
            if (Context.IsMainPlayer)
                return Game1.player.UniqueMultiplayerID;

           return helper.Multiplayer.GetConnectedPlayers().Where(p => p.IsHost).Select(h => h.PlayerID).First();
        }

        public int phasecount = 0;

        public void ProgressPhase(string debug)
        {
            if (!GameIsActive)
                return;

            lock (locked)
            {
                PhaseProgress--;
                phasecount--;

                if (PhaseProgress == 0)
                {
                    PhaseStep++;
                    if (PhaseStep >= Phases.Count)
                        PhaseStep = 0;

                    AddToPhase("NewPhase");

                    Phases[PhaseStep]();
                }
            }
        }

        private void AddToPhase(string debug)
        {
            lock (locked)
            {
                phasecount++;
                PhaseProgress++;
            }
        }

        public void End(List<WerwolfPlayer> winners, string message, bool wolveswon)
        {
            SendPlayerUpdateToAll(true);
            Game1.delayedActions.Add(new DelayedAction(3000, () =>
            {
                GameIsActive = false;
                Winners = winners;
                WolvesWon = wolveswon;
                WinMessage = message;
                SendPlayerUpdateToAll(true);
            }));

            CurrentGame = null;
            BotGames.Clear();
        }

        public void ProcessActionRequest(WerwolfActionRequest request)
        {
            if(Players.FirstOrDefault(p => p.PlayerID == request.SendFrom && p.IsAlive) is WerwolfPlayer player && Players.FirstOrDefault(p => p.PlayerID == request.TargetPlayer) is WerwolfPlayer targetPlayer)
                player.Roles.FirstOrDefault(r => r.RoleActions.Any(a => a.ID == request.ActionID))?.RoleActions.FirstOrDefault(ac => ac.ID == request.ActionID)?.Perform(this,targetPlayer);
        }

        public void SendVotesLetter()
        {
            var mayor = Players.First(p => p.IsMayor);
            var list = "";
            var loser = Players.First(p => p.PlayerID == CurrentVote.Decision);
            CurrentVote.Votes.Keys.ToList().ForEach(k =>
            {
                var player = Players.First(p => p.PlayerID == k);
                list += $"Votes for {player.Character.Name}: {string.Join(',', CurrentVote.Votes[k].Select(s => Players.First(pl => pl.PlayerID == s)).Select(c => c.Character.Name))}^";
            });

            var ultimate = $"{mayor.Character.Name} ({mayor.Roles.First(r => r.IsMayor).Name}) made the ultimate decision.";
            if (loser.PlayerID == mayor.PlayerID)
                ultimate = $"{mayor.Character.Name} ({mayor.Roles.First(r => r.IsMayor).Name}) will select a succesor.";

            var letter = $"Dear @, The votes have been cast. -{mayor.Character.Name}^ ^{list}^{ultimate}^{loser.Character.Name} will be executed promtly.";

            Players.ForEach(p =>
            {
                SendMessage(new WerwolfMessage(p.PlayerID, Host, this, WerwolfMessageType.LETTER, letter, "Public Announcement"));
            });
        }

        public void ExecuteKills()
        {
            var deathPool = DeathPool.Where(p => p.IsAlive).ToList();
            var deathRow = DeathRow.Where(p => p.IsAlive).ToList();
            DeathPool.Clear();
            DeathRow.Clear();

            deathPool.ForEach(p => {
                p.Kill();
                Players.ForEach(player =>
                {
                    player.Roles.ForEach(r => r.OnDeath(this, p));
                    SendMessage(new WerwolfMessage(player.PlayerID, Host, this, WerwolfMessageType.DIALOG, $"{p.DeathInfo}", p.Character.Name));
                });
            });

            deathRow.ForEach(p =>
            {
                p.Judge();
                Players.ForEach(player =>
                {
                    player.Roles.ForEach(r => r.OnDeath(this, p));
                    SendMessage(new WerwolfMessage(player.PlayerID, Host, this, WerwolfMessageType.DIALOG, $"{p.DeathInfo}", p.Character.Name));
                });
            });

            SendPlayerUpdateToAll();

            if (DeathPool.Count + DeathRow.Count > 0)
                ExecuteKills();
        }

        public void Judge(WerwolfPlayer player, string info)
        {
            if (!DeathRow.Contains(player))
            {
                DeathRow.Add(player);
                player.DeathInfo = info;
                player.KilledByWolves = false;
            }
        }

        public bool Kill(WerwolfPlayer player, string info, bool bywolves = true)
        {
            if (!DeathPool.Contains(player))
            {
                DeathPool.Add(player);
                player.DeathInfo = info;
                player.KilledByWolves = bywolves;
                return true;
            }

            return false;
        }

        public void UpdateRole(WerwolfPlayer player, IWerwolfRoleDescription role)
        {
            player.Roles.RemoveAll(r => r.RoleID == role.RoleID);
            player.Roles.Add(role);
        }

        private void SendMultiplayerMessage<T>(T message) where T : WerwolfMPMessage
        {
            if (message.SendTo == Game1.player.UniqueMultiplayerID || (Players.FirstOrDefault(p => p.PlayerID == message.SendTo) is WerwolfPlayer wp && wp.IsBot))
                WerwolfMod.ReceiveMPMessage(message.Type, message, Config, Monitor, Helper);
            else
                Helper.Multiplayer.SendMessage(message, message.Type, new[] {"Platonymous.Werewolf"}, new[] { message.SendTo });
        }

        private WerwolfUpdate GetUpdate(WerwolfPlayer player)
        {
            var players = Players.Select(p =>
            {
                if (p.PlayerID != player.PlayerID)
                    return p.GetPlayerInfo(false, this, false);

                return p.GetPlayerInfo(true, this, false);
            }).ToList();

            var update = new WerwolfUpdate(Round, player.PlayerID, Host, this, players, Winners.Select(w => w.GetPlayerInfo(false,this, false)).ToList(), WinMessage, WolvesWon);
            return update;
        }

        public void SendNewGameRequest(WerwolfPlayer player)
        {
            var newgame = new WerwolfNewGameRequest(Host, player.PlayerID, this, GetUpdate(player), Config, GameInfo);
            SendMultiplayerMessage(newgame);
        }


        public void SendPlayerUpdate(WerwolfPlayer player, bool end = false)
        {
            var players = Players.Select(p =>
            {
                if (p.PlayerID != player.PlayerID && !end)
                    return p.GetPlayerInfo(false, this, end);

                return p.GetPlayerInfo(true, this, end);
            }).ToList();

            var update = new WerwolfUpdate(Round, player.PlayerID, Host, this, players, Winners.Select(w => w.GetPlayerInfo(false, this, end)).ToList(), WinMessage, WolvesWon);

            this.SendUpdate(update);
        }

        public void SendPlayerUpdateToAll(bool end = false)
        {
            foreach(var player in Players)
                SendPlayerUpdate(player, end);
        }

        public void SendUpdate(WerwolfUpdate update)
        {
            SendMultiplayerMessage(update);
        }

        public void SendMessage(WerwolfMessage message)
        {
            SendMultiplayerMessage(message);
        }

        public void SendChoice(WerwolfChoice choice)
        {
            SendMultiplayerMessage(choice);
        }

        public void ProcessCallbacks()
        {
            if (!Started && Round >= 0 && !Players.Any(p => p.Name == "???"))
            {
                Players = Players.OrderBy(p => p.Name + "_" + p.Character.Name).ToList();
                Started = true;
                SendPlayerUpdateToAll();
            }

            Callbacks.RemoveAll(c => c.Finished);
            Callbacks.ForEach(c => c.CheckCallback(this));
        }

        public void ProcessCallbackAnswer(WerwolfChoiceResponse response)
        {
            Callbacks.FirstOrDefault(c => c.CallbackID == response.ChoiceID)?.ReceiveCallback(response.ChoiceID, response.Answer);
        }

        public void AddCallback(WerwolfCallbackRequest request)
        {
            Callbacks.RemoveAll(c => c.CallbackID == request.CallbackID);
            Callbacks.Add(request);
        }

        public void StartGame()
        {
            Config.MinPlayers = Math.Max(Config.MinPlayers, 4);
            PhaseStep = -1;
            AddToPhase("StartGame");
            List<long> players = GetAllPeers(Helper,Config).OrderBy(m => (int)HostRandom.Next(100000)).ToList();
            List<string> characters = new List<string>(Config.NPCs);
            characters = characters.OrderBy(c => (int)HostRandom.Next(100000)).ToList();
            int count = Math.Max(players.Count, Config.MinPlayers);
            int maxPlayers = characters.Count();
            int wolves = (int)Math.Max(Math.Floor(count / 4f), 1f);
            int roles = (int)Math.Min((count - wolves) / 3f, VillagerRoles.Count());
            List<IWerwolfRoleDescription> werwolfRoles = WerwolfRoles.Where(r => Config.WerwolfRoles.Contains(r.Name)).OrderBy(r => HostRandom.Next(1000000)).ToList();
            List<IWerwolfRoleDescription> villagerRoles = VillagerRoles.Where(r => Config.VillagerRoles.Contains(r.Name)).OrderBy(r => HostRandom.Next(1000000)).ToList();
            List<IWerwolfRoleDescription> mayorRoles = MayorRoles.Where(r => Config.MayorRoles.Contains(r.Name)).OrderBy(r => HostRandom.Next(1000000)).ToList();
            
            players.ForEach(p =>
            {
                if (Players.Count < maxPlayers)
                {
                    NPC npc = null;
                    while (npc == null)
                    {
                        var chr = characters.Last();
                        if (characters.Contains("Lewis"))
                            chr = "Lewis";
                        characters.Remove(chr);
                        npc = Game1.getCharacterFromName(chr, true, true);
                    }
                    var wp = new WerwolfPlayer(Game1.player.UniqueMultiplayerID == p ? Game1.player.Name : "???", p, npc, false, new List<IWerwolfRoleDescription>());
                    Players.Add(wp);
                }
            });

            if (Config.FillWithBots && Players.Count < Config.MinPlayers && Players.Count < maxPlayers)
                while (Players.Count < Config.MinPlayers && Players.Count < maxPlayers)
                {
                    NPC npc = null;
                    while (npc == null)
                    {
                        var chr = characters.Last();
                        characters.Remove(chr);
                        npc = Game1.getCharacterFromName(chr, true, true);
                    }
                    var wp = new WerwolfPlayer($"{npc.Name}-Bot", Helper.Multiplayer.GetNewID(), npc, true, new List<IWerwolfRoleDescription>());
                    Players.Add(wp);
                }

            Players = Players.OrderBy(p => HostRandom.Next(1000000)).ToList();
            var assignedWolves = 0;
            List<string> assignedRoles = new List<string>();
            Players.ForEach(p =>
            {
                if (wolves > 0)
                {
                    p.Roles.Add(new WerwolfRoleDescriptionWolf(p));
                    if (werwolfRoles.Count > 0)
                    {
                        var wrole = werwolfRoles.Last();
                        werwolfRoles.Remove(wrole);
                        p.Roles.Add(wrole.GetAssigned(p));
                        assignedRoles.Add(wrole.Name);
                    }
                    assignedWolves++;
                    wolves--;
                }
                else
                {
                    p.Roles.Add(new WerwolfRoleDescriptionVillager(p));
                    if (villagerRoles.Count > 0 && roles > 0)
                    {
                        var vrole = villagerRoles.Last();
                        villagerRoles.Remove(vrole);
                        p.Roles.Add(vrole.GetAssigned(p));
                        roles--;
                        assignedRoles.Add(vrole.Name);
                    }
                }
            });

            Players = Players.OrderBy(p => p.Character.Name).ToList();

            if (Players.FirstOrDefault(p => p.Character.Name == "Lewis") is WerwolfPlayer lewis)
            {
                if (mayorRoles.Count > 0)
                    lewis.Roles.Add(mayorRoles.Last().GetAssigned(lewis));
                else
                    lewis.Roles.Add(new WerwolfRoleDescriptionMayor(lewis));
            }
            else
            {
                var first = Players.First();
                if (mayorRoles.Count > 0)
                    first.Roles.Add(mayorRoles.Last().GetAssigned(first));
                else
                    first.Roles.Add(new WerwolfRoleDescriptionMayor(first));
            }

            GameInfo = $"{assignedWolves} Werewolves, {assignedRoles.Count} Others: {string.Join(',', assignedRoles)}";

            Players.ForEach(p =>
            {
                if (!p.IsBot)
                    SendNewGameRequest(p);
                else
                    BotGames.Add(new WerwolfClientGame(Helper, Monitor, new WerwolfNewGameRequest(Host, p.PlayerID, this, GetUpdate(p), Config, GameInfo),p.PlayerID));
            });

            PreGame();
        }

        public void FixRoles()
        {
            var players = Players.ToList();
            for (int i = 0; i < players.Count(); i++)
            {
                players[i].Roles.RemoveAll(rol => rol.Remove);
                players[i].Roles.AddRange(players[i].NewRoles);
                players[i].NewRoles.Clear();
            }
        }

        public void PopulatePhase(List<WerwolfPlayer> players)
        {
            FixRoles();

            for (int i = 0; i < players.Count(); i++)
            {
                var rlist = new List<IWerwolfRoleDescription>(players[i].Roles.ToArray());
                var r = rlist.Count();
                for (int j = 0; j < r; j++)
                    AddToPhase("Player");
            }
        }

        public void PreGame()
        {
            if (!Players.Any(p => p.Name == "???"))
            {
                PopulatePhase(Players);

                Players.ToList().ForEach(p => p.Roles.ToList().ForEach(r =>
                {
                    r.PreGame(this, ProgressPhaseCallback);
                }));


                ProgressPhase("Pregame");
            } else
                Game1.delayedActions.Add(new DelayedAction(1000, () => PreGame()));
        }

        public void DayStart()
        {
            FixRoles();
            SendPlayerUpdateToAll();
            if (ReadyToProgress())
                ProgressPhase("DayStart");
        }

        public bool ReadyToProgress()
        {
            if (PhaseStep == 0)
            { 
                var players = Players.Where(p => p.IsBot && p.IsAlive).ToList();

                for(int i = 0; i < players.Count; i++)
                {
                    var bot = players[i];
                    var roles = bot.Roles.ToList();
                    for(int j = 0; j < roles.Count; j++)
                    {
                        var r = roles[j];
                        var actions = r.RoleActions.Where(a => a.IsActive).ToList();

                        for(int k = 0; k < actions.Count; k++)
                        {
                            var act = actions[k];
                            act.BotPerform(this);
                            act.IsActive = false;
                        }
                    }
                }
            }

            return PhaseStep == 0 && Players.Where(pl => pl.IsAlive).All(p => p.Roles.All(r =>
            {
                var ready = r.ReadyToProgress();
                return ready;
            }));
        }

        public void BeforeKills()
        {
            SendPlayerUpdateToAll();

            PopulatePhase(Players.Where(player => player.IsAlive).ToList());

            Players.Where(player => player.IsAlive).ToList().ForEach(p => p.Roles.ForEach(r =>
            {
                r.BeforeKills(this, DeathPool.Select(d => d.PlayerID).ToList(), ProgressPhaseCallback);
            }));
            ProgressPhase("BeforeKills");
        }

        public void AfterKills()
        {
            ExecuteKills();
            ProgressPhase("AfterKills");
        }

        public void ChooseNewMayer(WerwolfPlayer wp, Action callback)
        {
            List<WerwolfChoiceOption> choices = Players.Where(v => v.PlayerID != wp.PlayerID && v.IsAlive).Select(l => new WerwolfChoiceOption($"{l.Name}/{l.Character.Name}", l.PlayerID.ToString())).ToList();

            SendChoice(new WerwolfChoice(
                wp.PlayerID,
                Host,
                this, $"Mayor_Succession_{GameID}_{Round}_{wp.PlayerID}",
                "Who should succeed you?",
                choices,
                (q, c) =>
                {
                    if (long.TryParse(c, out long result) && Players.FirstOrDefault(p => p.PlayerID == result) is WerwolfPlayer newMayor)
                    {
                        var mrole = wp.Roles.FirstOrDefault(r => r.IsMayor);
                        newMayor.NewRoles.Add(mrole.GetAssigned(newMayor));
                        wp.Roles.RemoveAll(r => r.IsMayor);
                        Players.ForEach(p => {
                            SendMessage(new WerwolfMessage(p.PlayerID, Host, this, WerwolfMessageType.INFO, $"{newMayor.Character.Name} is succeeding {wp.Character.Name}. ({mrole.Name})","Werewolf"));
                            });
                        SendPlayerUpdateToAll();
                    }
                    callback();
                }
                ));
        }

        public void BeforeVote()
        {
            FixRoles();
            ExecuteKills();
            SendPlayerUpdateToAll();

            if (Players.FirstOrDefault(p => p.IsMayor) is WerwolfPlayer wp && !wp.IsAlive)
                ChooseNewMayer(wp, () => ProgressPhase("BeforeVote"));
            else
                ProgressPhase("BeforeVote");
        }

        public void Vote()
        {
            FixRoles();
            SendPlayerUpdateToAll();
            var alive = Players.Where(player => player.IsAlive).ToList();
            foreach (WerwolfPlayer pl in alive)
                AddToPhase("PlayerVote");

            foreach (WerwolfPlayer pl in alive)
            {
                List<WerwolfPlayer> players = Players.Where(wp => wp.IsAlive).ToList();
                bool canVote = true;
                foreach (IWerwolfRoleDescription r in pl.Roles.ToList())
                {
                    players = r.SetupVote(players);
                    canVote = canVote && r.CanVote;
                }
                if (players.Count > 0 && canVote)
                    SendVoteRequest(pl, players);
                else
                    ProgressPhaseCallback();
            }

            ProgressPhase("Vote");
        }

        public void SendVoteRequest(WerwolfPlayer pl, List<WerwolfPlayer> players)
        {
            SendChoice(new WerwolfChoice(
                   pl.PlayerID,
                   Host,
                   this,
                   $"Vote_{pl.PlayerID}_{GameID}_{Round}",
                   "Who should be executed for being a Werwolf?",
                   players.Select(p => new WerwolfChoiceOption($"{p.Name}/{p.Character.Name}", p.PlayerID.ToString())).ToList(),
                   (s, c) =>
                   {
                       if (long.TryParse(c, out long l))
                       {
                           CurrentVote.AddVote(l, pl.PlayerID);
                           ProgressPhaseCallback();
                       }
                       else
                           SendVoteRequest(pl,players);
                   }));
        }

        public void AfterVote()
        {
            PopulatePhase(Players.Where(player => player.IsAlive).ToList());

            Players.Where(player => player.IsAlive).ToList().ForEach(p => p.Roles.ForEach(r =>
            {
                r.AfterVote(this, ProgressPhaseCallback,CurrentVote);
            }));

            SendPlayerUpdateToAll();
            ProgressPhase("AfterVote");
        }


        public void AfterJudgement()
        {
            SendVotesLetter();

            if (Players.FirstOrDefault(p => p.PlayerID == CurrentVote.Decision) is WerwolfPlayer loser)
            {
                var wasWolf = loser.IsWolf(false) ? $"{loser.Character.Name} was indeed a WERWOLF." : $"{loser.Character.Name} was actually a VILLAGER.";
                Judge(loser, $"{loser.Name}/{loser.Character.Name} was executed for allegedly being a werwolf. {wasWolf}");
            }

            ExecuteKills();

            if (Players.FirstOrDefault(p => p.IsMayor) is WerwolfPlayer wp && !wp.IsAlive)
                ChooseNewMayer(wp, () => ProgressPhase("AfterJudgement"));
            else
                ProgressPhase("AfterJudgement");
        }


        public void AfterRound()
        {
            SendPlayerUpdateToAll();
            Round++;
            PastVotes.Add(CurrentVote);
            CurrentVote = new WerwolfVotes(Round);
            Players.Where(p => p.IsAlive).ToList().ForEach(
                p =>
                {
                    p.Roles.ToList().ForEach(r =>
                    {
                        r.RoleActions.ToList().ForEach(a =>
                        {
                            a.AfterRound(this);
                        });
                    });
                }

                );
            SendPlayerUpdateToAll();
            ProgressPhase("AfterRound");
        }

    }
}
