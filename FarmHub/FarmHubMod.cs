using Firebase.Database;
using Firebase.Database.Query;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FarmHub
{
    public class FarmHubMod : Mod
    {
        internal static FirebaseClient client;
        internal static ChildQuery farms => client.Child("Farms");
        internal static string guid => config.UniqueId;
        internal static string password => config.Password;
        internal static string farmHub => config.FarmHub;
        internal static string[] requiredMods => config.RequiredMods;
        internal static List<FarmHubServer> farmHubServers = new List<FarmHubServer>();
        internal static FarmHubServer myServer;
        internal static FHConfig config;
        internal static string open;
        internal static List<FarmHubServer> temp;

        public override void Entry(IModHelper helper)
        {
            open = "open".toMD5Hash();
            Guid.NewGuid();
            config = Helper.ReadConfig<FHConfig>();
            TimeEvents.AfterDayStarted += StartFarmHubServer;

            new ConsoleCommand("farmhub", "Lists the available servers", (s, p) => populateList(writeListToConsole)).register();
            new ConsoleCommand("join", "Join a server by name. Name and password have to use _ instead of spaces. Usage: join [name] [password if required]", (s, p) => populateList((l) => findServer(l,p) )).register();
        }

        private void findServer(List<FarmHubServer> list, string[] p)
        {
            Monitor.Log("Looking for Server", LogLevel.Info);

            if (!(Game1.activeClickableMenu is TitleMenu))
            {
                Monitor.Log("You have to be in the Title Menu to join a server.", LogLevel.Error);
                return;
            }

            if (p.Length < 1)
                Monitor.Log("Please enter the name of the Server you want to join.", LogLevel.Error);
            else if (p.Length > 2)
                Monitor.Log("Too many arguments, make sure to use _ instead of spaces in the name and password", LogLevel.Error);

            FarmHubServer server;
            string pass = "open";
            if (p.Length > 1)
                pass = p[1].Replace('_', ' ');

            string name = p[0].Replace('_', ' ');
            temp = farmHubServers.Where(h => h.Name == name).ToList();

            if (temp.Count > 1)
            {
                Monitor.Log("Found more than one server of that name. The one that was last updated will be picked.", LogLevel.Warn);
                server = temp.Find(f => f.LastUpdate == temp.Max(g => g.LastUpdate));
            }
            else if (temp.Count == 0)
            {
                Monitor.Log("Could not find a server with that name.", LogLevel.Error);
                return;
            }
            else
                server = temp.First();

            if (pass.toMD5Hash() != server.Password)
            {
                Monitor.Log("Wrong password.", LogLevel.Error);
                return;
            }

            if(!hasMods(server.RequiredMods))
            {
                Monitor.Log("You don't have all the mods required on this server.", LogLevel.Error);
                return;
            }

            joinServer(server.InviteCode);
        }

        private void writeListToConsole(List<FarmHubServer> list)
        {
            Monitor.Log("---------|FarmHub|---------",LogLevel.Info);
            if (list.Count == 0)
            {
                Monitor.Log("No Server available", LogLevel.Error);
                Monitor.Log("---------------------------", LogLevel.Info);
            }
            for (int i = 0; i < list.Count; i++)
            {
                FarmHubServer f = list[i];
                Monitor.Log($"{i} : {f.Name} ({f.CurrentPlayers}/{f.MaxPlayers}) [{ (f.Password == open ? "open" : "pw") }]", LogLevel.Info);
                bool hMods = hasMods(f.RequiredMods);
                Monitor.Log("Required Mods: " + string.Join(",", f.RequiredMods), hMods ? LogLevel.Info : LogLevel.Error);
                Monitor.Log("---------------------------",LogLevel.Info);
            }
        }

        private void joinServer(string code)
        {
            Monitor.Log("Joining server.. ", LogLevel.Info);

            object lobbyFromInviteCode = Program.sdk.Networking.GetLobbyFromInviteCode(code);
            if (lobbyFromInviteCode == null)
            {
                Monitor.Log("Server isn't available", LogLevel.Error);
                return;
            }
            Game1.activeClickableMenu = new FarmhandMenu(Program.sdk.Networking.CreateClient(lobbyFromInviteCode));
        }

        private bool hasMods(string[] modIds)
        {
            foreach (string id in modIds)
                if(!Helper.ModRegistry.IsLoaded(id))
                    return false;

            return true;
        }

        private void populateList(Action<List<FarmHubServer>> callback = null)
        {
            if (client == null)
                client = new FirebaseClient(farmHub);
            Task.Run(async () =>
           {
               var servers = await farms.OnceAsync<FarmHubServer>();
               farmHubServers.Clear();
               foreach (var server in servers)
               {
                   int cTime = (Int32)(DateTime.UtcNow.Subtract(new DateTime(2018, 1, 1))).TotalSeconds;
                   FarmHubServer fhs = server.Object;
                   if (cTime - fhs.LastUpdate >= 60)
                       Task.Run(() => farms.Child(fhs.Id).DeleteAsync());
                   else
                       farmHubServers.Add(server.Object);
                   callback?.Invoke(farmHubServers);
               }
           });
        }

        private void waitForServerConnection(Action onConnection)
        {
            int timeout = 3000;
            while (timeout >= 0)
            {
                timeout--;
                if (Game1.server != null && Game1.server.connected())
                {
                    onConnection();
                    return;
                }
                Thread.Sleep(1);
            }
           
        }

        private void StartFarmHubServer(object sender, EventArgs e)
        {
            if(Game1.IsServer)
            {
                if (client == null)
                    client = new FirebaseClient(farmHub);
                
                if (myServer == null)
                {
                    Task.Run(() => waitForServerConnection(() => {
                        Monitor.Log("InviteCode:" + Game1.server.getInviteCode());
                        myServer = new FarmHubServer(Game1.server, Monitor);
                        Monitor.Log("Joining Farmhub", LogLevel.Info);
                    }));                    
                }
                }
            else
                if (myServer is FarmHubServer fsh)
                    fsh.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (myServer is FarmHubServer fhs)
                fhs.Dispose();
            if(client is FirebaseClient fc)
                fc.Dispose();
            base.Dispose(disposing);
        }
     
    }
}
