using Newtonsoft.Json;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
namespace Comics
{
    public class CVApiClient
    {
        private readonly Uri BaseUrl;
        private readonly string APIKey;
        private readonly int BaseYear;
        private readonly static Dictionary<string, IEnumerable<Issue>> Loaded = new Dictionary<string, IEnumerable<Issue>>();

        public CVApiClient(int baseYear = -1)
        {
            BaseYear = baseYear;
            BaseUrl = new Uri("https://comicvine.gamespot.com/api/issues/");
            APIKey = Credentials.Key;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        public Issue GetIssue(string id)
        {
            if (Loaded.ContainsKey(id))
                return Loaded[id].FirstOrDefault();

            try
            {
                using (WebClient client = new WebClient())
                {
                    var query = new NameValueCollection();
                    query.Add("format", "json");
                    query.Add("api_key", APIKey);
                    query.Add("filter", "id:" + id);
                    query.Add("sort", "store_date:desc");
                    query.Add("limit", "1");
                    client.QueryString = query;
                    string response = client.DownloadString(BaseUrl);
                    var result = JsonConvert.DeserializeObject<IssuesRequest>(response)?.Results ?? new List<Issue>();
                    Loaded.Add(id, result);
                    return result.FirstOrDefault();
                }
            }
            catch
            {
                return null;
            }
        }

            public IEnumerable<Issue> GetIssues(int baseYear = -1)
        {
            if (baseYear == -1)
                baseYear = BaseYear;

            uint d = Game1.stats.DaysPlayed;

            DateTime baseDate = DateTime.Today;

            if (BaseYear != -1)
                baseDate = new DateTime(BaseYear, 1, 1);
            else
                d = 0;

            if (baseDate >= DateTime.Today)
                baseDate = DateTime.Today;

            int tDays = (int)(d * 3.25f);
            DateTime start = baseDate.AddDays(-4).AddDays(tDays);
            DateTime end = baseDate.AddDays(3).AddDays(tDays);

            return GetIssues(start, end);
        }
            private IEnumerable<Issue> GetIssues(DateTime start, DateTime end)
            {

            string range = start.ToString("yyyy-MM-dd") + "|" + end.ToString("yyyy-MM-dd");
            if (Loaded.ContainsKey(range))
                return Loaded[range];

            IEnumerable<Issue> result = new List<Issue>();

           

            try
            {
                using (WebClient client = new WebClient())
                {
                    var query = new NameValueCollection();
                    query.Add("format", "json");
                    query.Add("api_key", APIKey);
                    query.Add("filter", "store_date:" + range);
                    query.Add("sort", "store_date:desc");
                    query.Add("limit", "50");
                    client.QueryString = query;
                    string response = client.DownloadString(BaseUrl);
                    result = JsonConvert.DeserializeObject<IssuesRequest>(response)?.Results ?? new List<Issue>();
                    Loaded.Add(range, result);
                }
            }
            catch
            {
                result = new List<Issue>();
            }

            return result;
        }
    }
}