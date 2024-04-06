using Artista.Artpieces;
using RestSharp;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Text.Json.Serialization;

namespace Artista.Online
{

    public partial class OnlineArtAPI
    {
        const string ApiServer = "https://artdb.senddomain.de/api";
        public RestClient Client { get; set; }

        public User CurrentUser
        {
            get
            {
                return Data.User;
            }
            set
            {
                Data.User = value;
            }
        }

        public OnlineData Data { get; set; }

        public OnlineArtAPI(IModHelper helper)
        {
            Data = helper.Data.ReadJsonFile<OnlineData>("client.json") ?? new OnlineData();
            helper.Data.WriteJsonFile<OnlineData>("client.json", Data);

            Client = new RestClient(ApiServer);

        }

        public void SetDownloaded(OnlineArtpiece art)
        {
            if (!Data.Downloads.Contains(art.id))
            {
                Data.Downloads.Add(art.id);
                ArtistaMod.Singleton.Helper.Data.WriteJsonFile<OnlineData>("client.json", Data);
            }
        }

        public User CreateUser(string name)
        {
            User user = new User() { name = name, p1 = Guid.NewGuid().ToString(), p2 = Guid.NewGuid().ToString(), active = true };
            try
            {
                var request = new RestRequest("collections/User/records");
                request.AddJsonBody(user);
                request.Method = Method.POST;
                var result = Client.Execute<User>(request);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    return result.Data;
                else
                {
                    ArtistaMod.Singleton.Monitor.Log("Error", StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.ErrorMessage, StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.StatusCode.ToString(), StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.Content, StardewModdingAPI.LogLevel.Error);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public ListArtRequest GetArt(int page = 1, string collection = "default", int perpage = 12, bool owned = false)
        {
            try
            {
                var sort = collection == "default" ? owned ? $"&filter=(owner='{CurrentUser.id}')" : "" : owned ? $"&filter=(owner='{CurrentUser.id}' %26%26 collection='{collection}')" : $"&filter=(collection='{collection}')";
                var request = new RestRequest($"collections/Art/records?page={page}&perPage={perpage}{sort}&sort=-downloads");
                request.Method = Method.GET;
                var result = Client.Execute<ListArtRequest>(request);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return result.Data;
                }
                else
                {
                    ArtistaMod.Singleton.Monitor.Log("Error", StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.ErrorMessage, StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.StatusCode.ToString(), StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.Content, StardewModdingAPI.LogLevel.Error);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public ListCompetitons GetCompetitions(int page = 1, int perpage = 1)
        {
            try
            {
                var request = new RestRequest($"collections/Competitions/records?page={page}&perPage={perpage}&sort=-created&filter=(active=true)");
                request.Method = Method.GET;
                var result = Client.Execute<ListCompetitons>(request);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return result.Data;
                }
                else
                {
                    ArtistaMod.Singleton.Monitor.Log("Error", StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.ErrorMessage, StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.StatusCode.ToString(), StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.Content, StardewModdingAPI.LogLevel.Error);
                }
            }
            catch
            {
                return new ListCompetitons();
            }

            return new ListCompetitons();
        }

        public bool DeleteArt(OnlineArtpiece orp)
        {
            if (orp == null)
                return false;

            try
            {
                var request = new RestRequest($"collections/Art/records/{orp.id}");
                request.Method = Method.DELETE;
                request.AddHeader("p1", CurrentUser.p1);
                request.AddHeader("p2", CurrentUser.p2);
                request.AddHeader("oid", CurrentUser.id);
                var result = Client.Execute(request);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return true;
                    ArtistaMod.Singleton.Monitor.Log($"p1={CurrentUser.p1}&p2={CurrentUser.p2}&oid={CurrentUser.id}", StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log("Error", StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.ErrorMessage, StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.StatusCode.ToString(), StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.Content, StardewModdingAPI.LogLevel.Error);
                }
            }
            catch
            {
                return true;
            }

            return true;
        }

        public Artpiece DownloadArt(OnlineArtpiece orp)
        {
            if (orp == null)
                return null;


            if(Data.Downloads.Contains(orp.id)){
                var sav = SavedArtpiece.FromJson(orp.artwork);
                if (sav.ArtType == (int)ArtType.Painting)
                    return new Painting(sav);

                return null;
            }

            var body = new UpdateDownloads() { downloads = orp.downloads + 1};

            try
            {
                var request = new RestRequest("collections/Art/records/" + orp.id);
                request.AddJsonBody(body);
                request.Method = Method.PATCH;
                var result = Client.Execute<OnlineArtpiece>(request);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var sav = SavedArtpiece.FromJson(result.Data.artwork);
                    if (sav.ArtType == (int)ArtType.Painting)
                    {
                        SetDownloaded(result.Data);
                        return new Painting(sav);
                    }

                    return null;
                }
                else
                {
                    ArtistaMod.Singleton.Monitor.Log("Error", StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.ErrorMessage, StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.StatusCode.ToString(), StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.Content, StardewModdingAPI.LogLevel.Error);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public bool CheckOnline()
        {
            if (GetArt(1, "default", 2) is ListArtRequest)
                return true;

            return false;
        }

        public void SetUser(Action callback)
        {
            if(CurrentUser == null)
            {
                Game1.activeClickableMenu?.exitThisMenu();
                Game1.activeClickableMenu = new StardewValley.Menus.NamingMenu((s) =>
                {
                    CurrentUser = CreateUser(s);
                    ArtistaMod.Singleton.Helper.Data.WriteJsonFile<OnlineData>("client.json", Data);
                    Game1.activeClickableMenu?.exitThisMenu();
                    callback();
                }, "Your Artist Name", Game1.player?.Name ?? "Unknown");
            }
            callback();
        }

        public OnlineArtpiece UploadArt(Artpiece art, string collection = "default")
        {
            if(art == null || CurrentUser == null)
                return null;


            var sv = art.Save();
            var jsv = sv.GetJsonData();

            

            OnlineArtpiece orp = new OnlineArtpiece() { active = true, artwork = jsv, owner = CurrentUser.id, author = CurrentUser.name, collection = collection, downloads = 0 };
            try
            {
                var request = new RestRequest("collections/Art/records");
                request.AddJsonBody(orp);
                request.Method = Method.POST;
                var result = Client.Execute<OnlineArtpiece>(request);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    SetDownloaded(result.Data);
                    return result.Data;
                }
                else
                {
                    ArtistaMod.Singleton.Monitor.Log("Error", StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.ErrorMessage, StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.StatusCode.ToString(), StardewModdingAPI.LogLevel.Error);
                    ArtistaMod.Singleton.Monitor.Log(result.Content, StardewModdingAPI.LogLevel.Error);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }        
    }
}
