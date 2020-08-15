using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using PlatoTK;

namespace Comics
{
    internal class AssetManager
    {
        internal IModHelper Helper;
        public Texture2D Placeholder;
        public static AssetManager Instance;
        public static bool LoadImagesInShop { get; set; } = false;

        public Dictionary<string, Issue> Issues { get; } = new Dictionary<string, Issue>();

        public AssetManager(IModHelper helper)
        {
            Helper = helper;
            Placeholder = LoadPlaceholder();
            Instance = this;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public IEnumerable<Issue> LoadIssuesForToday(int baseYear, uint daysPlayed)
        {
            var api = new CVApiClient(baseYear);
            var result = api.GetIssues(baseYear, daysPlayed);
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
            var api = new CVApiClient(-1);

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
            string absolute = Path.Combine(Helper.DirectoryPath, relative);
            Texture2D texture = null;
            try
            {
                texture = Helper.Content.Load<Texture2D>(relative);
            }
            catch
            {
                texture = null;
            }

            if ( texture == null && Issues.ContainsKey(id))
                texture = DownloadImageFileForIssue(big ? Issues[id].Image.MediumUrl : Issues[id].Image.SmallUrl, id, big);
            else if(texture == null)
                texture = DownloadImageFileForIssue(new Uri(url), id, big);

            if (texture == null)
                texture = Placeholder;

            float scale = texture.Height / 16f;
            float tScale = 1f / scale;
            Texture2D smallTexture = null;

            try
            {
                smallTexture = Helper.GetPlatoHelper().Content.Textures.ResizeTexture(texture, (int)(tScale * texture.Width), (int)(tScale * texture.Height));
            }
            catch
            {
                smallTexture = Helper.GetPlatoHelper().Content.Textures.ExtractArea(texture, new Microsoft.Xna.Framework.Rectangle(0, 0, (int)(tScale * texture.Width), (int)(tScale * texture.Height)));
            }

            if(smallTexture == null)
                smallTexture = Helper.GetPlatoHelper().Content.Textures.GetRectangle((int)(tScale * texture.Width), (int)(tScale * texture.Height), Microsoft.Xna.Framework.Color.Red);

            if (texture.Height > 16)
            {
                smallTexture.Tag = Helper.ModRegistry.ModID + ".Comic_" + id;
                return Helper.GetPlatoHelper().Harmony.GetDrawHandle<Texture2D>("Comic_" + id, texture, (handler) =>
                {
                    handler.Texture = handler.Data;
                    handler.SourceRectangle = null;
                    handler.Draw();
                    return true;
                },smallTexture);
            }

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
            float tScale = 1f / scale;
            var smallTexture = Helper.GetPlatoHelper().Content.Textures.ResizeTexture(texture, (int)(tScale * texture.Width), (int)(tScale * texture.Height));

            if (texture.Height > 16)
            {
                smallTexture.Tag = Helper.ModRegistry.ModID + ".Comic_" + id;
                return Helper.GetPlatoHelper().Harmony.GetDrawHandle<Texture2D>("Comic_" + id, texture, (handler) =>
                {
                    handler.Texture = handler.Data;
                    handler.SourceRectangle = null;
                    handler.Draw();
                    return true;
                }, smallTexture);
            }

            return texture;
        }
    }
}
