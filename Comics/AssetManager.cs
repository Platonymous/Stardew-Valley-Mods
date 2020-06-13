using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PyTK.Extensions;

namespace Comics
{
    internal class AssetManager
    {
        internal IModHelper Helper;
        public Texture2D Placeholder;
        public static AssetManager Instance;

        public Dictionary<string, Issue> Issues { get; } = new Dictionary<string, Issue>();

        public AssetManager(IModHelper helper)
        {
            Helper = helper;
            Placeholder = LoadPlaceholder();
            Instance = this;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public IEnumerable<Issue> LoadIssuesForToday(int baseYear = -1)
        {
            var api = new CVApiClient(baseYear);
            var result = api.GetIssues(baseYear);
            foreach (Issue issue in result)
                if (!Issues.ContainsKey(issue.Id.ToString()))
                    Issues.Add(issue.Id.ToString(), issue);

            return result;
        }

        private Texture2D LoadPlaceholder()
        {
            return Helper.Content.Load<Texture2D>("assets/issues/placeholder.png");
        }

        public Issue GetIssue(string id)
        {
            var api = new CVApiClient();

            if (Issues.ContainsKey(id))
                return Issues[id];
            else if (api.GetIssue(id) is Issue issue)
            {
                Issues.Add(issue.Id.ToString(), issue);
                return issue;
            }

            return null;
        }

        public Texture2D DownloadImageFileForIssue(Uri file, string id, bool big = false)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string relative = Path.Combine("assets", "issues", id + (big ? "_big" : "") + ".png");
                    string absolute = Path.Combine(Helper.DirectoryPath, "assets", "issues", id + (big ? "_big" : "") + ".png");

                    client.DownloadFile(file,absolute);
                    Texture2D texture = Helper.Content.Load<Texture2D>(relative) ?? Placeholder;
                    return texture;
                }
            }
            catch
            {
                return Placeholder;
            }
        }

        public Texture2D LoadImage(string url, string id, bool big = false)
        {
            string relative = Path.Combine("assets", "issues", id + (big ? "_big" : "") + ".png");
            string absolute = Path.Combine(Helper.DirectoryPath, "assets", "issues", id + (big ? "_big" : "") + ".png");
            Texture2D texture = Placeholder;
            if (File.Exists(absolute))
                texture = Helper.Content.Load<Texture2D>(relative) ?? Placeholder;
            else if (Issues.ContainsKey(id))
                texture = DownloadImageFileForIssue(big ? Issues[id].Image.MediumUrl : Issues[id].Image.SmallUrl, id, big);
            else
                texture = DownloadImageFileForIssue(new Uri(url), id, big);
            float scale = texture.Height / 16f;
            var smallTexture = texture.ScaleUpTexture(1f / scale, false);

            if (texture.Height > 16)
                return ScaledTexture2D.FromTexture(smallTexture, texture, scale);
            else
                return texture;
        }

            public Texture2D LoadImageForIssue(string id, bool big = false)
        {
            string relative = Path.Combine("assets", "issues", id + (big ? "_big" : "") + ".png");
            string absolute = Path.Combine(Helper.DirectoryPath, "assets", "issues", id + (big ? "_big" : "") + ".png");
            var texture = Placeholder;
            if (File.Exists(absolute))
                texture = Helper.Content.Load<Texture2D>(relative) ?? Placeholder;
            else if (Issues.ContainsKey(id))
                texture = DownloadImageFileForIssue(big ? Issues[id].Image.MediumUrl : Issues[id].Image.SmallUrl, id, big);
            else if (GetIssue(id) is Issue issue)
                texture = DownloadImageFileForIssue(big ? issue.Image.MediumUrl : issue.Image.SmallUrl, id, big);
            
            float scale = texture.Height / 16f;
            var smallTexture = texture.ScaleUpTexture(1f / scale, false);

            if (texture.Height > 16)
                return ScaledTexture2D.FromTexture(smallTexture, texture, scale);
            else
                return texture;
        }
    }
}
