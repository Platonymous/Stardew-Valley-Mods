using FarmHub.Firebase.Database.Query;
using System;
using System.Reflection;
using System.Threading.Tasks;
using PyTK.Extensions;
using StardewValley.Network;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace FarmHub
{
    public class FarmHubServer
    {
        public string InviteCode { get; set; }
        public string IP { get; set; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public int LastUpdate { get; set; }
        public string Password { get; set; }
        public string Id { get; set; }
        public string[] RequiredMods { get; set; }
        private string Guid { get; set; }
        private ChildQuery farms => FarmHubMod.farms;
        private IMonitor Monitor;

        public FarmHubServer()
        {

        }

        public void Update(object sender = null, TimeChangedEventArgs e = null)
        {
            Multiplayer multiplayer = (Multiplayer)typeof(Game1).GetField("multiplayer", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            MaxPlayers = multiplayer.MaxPlayers;
            CurrentPlayers = Game1.otherFarmers.Where(f => f.Value != Game1.player && f.Value.isActive()).Count();
            LastUpdate = (Int32)(DateTime.UtcNow.Subtract(new DateTime(2018, 1, 1))).TotalSeconds;
            if (CurrentPlayers >= MaxPlayers)
                Dispose();
            else
                Task.Run(() => farms.Child(Id).PutAsync(this));
            
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            return "na";
        }

        public FarmHubServer(IGameServer server, IMonitor monitor)
        {
            Monitor = monitor;
            InviteCode = server.getInviteCode();
            IP = FarmHubMod.config.UseIP ? GetLocalIPAddress() : "na";
            Name = Game1.player.farmName.Value;
            Password = FarmHubMod.password.toMD5Hash();
            Guid = FarmHubMod.guid;
            FarmHubMod.events.GameLoop.TimeChanged += Update;
            FarmHubMod.events.GameLoop.ReturnedToTitle += DelistServer;
            Id = "Farm_" + Name + "_" + Guid;
            RequiredMods = FarmHubMod.requiredMods;
            Update();
        }

        private void DelistServer(object sender = null, ReturnedToTitleEventArgs e = null)
        {
            Monitor.Log("Delisting FarmHubServer");
            FarmHubMod.events.GameLoop.TimeChanged -= Update;
            FarmHubMod.events.GameLoop.ReturnedToTitle -= DelistServer;
            Task.Run(() => farms.Child(Id).DeleteAsync());
            FarmHubMod.myServer = null;
        }

        public void Dispose()
        {
            Monitor.Log("Disposing FarmHubServer");
            DelistServer();
        }
    }
}
