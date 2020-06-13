namespace Comics
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public partial class IssuesRequest
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("limit")]
        public long Limit { get; set; }

        [JsonProperty("offset")]
        public long Offset { get; set; }

        [JsonProperty("number_of_page_results")]
        public long NumberOfPageResults { get; set; }

        [JsonProperty("number_of_total_results")]
        public long NumberOfTotalResults { get; set; }

        [JsonProperty("status_code")]
        public long StatusCode { get; set; }

        [JsonProperty("results")]
        public List<Issue> Results { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public partial class Issue
    {
        
        [JsonProperty("description")]
        public string Description { get; set; }

        
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("issue_number")]
        public string IssueNumber { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("volume")]
        public Volume Volume { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("medium_url")]
        public Uri MediumUrl { get; set; }

        [JsonProperty("screen_url")]
        public Uri ScreenUrl { get; set; }

        [JsonProperty("screen_large_url")]
        public Uri ScreenLargeUrl { get; set; }

        [JsonProperty("small_url")]
        public Uri SmallUrl { get; set; }

        [JsonProperty("super_url")]
        public Uri SuperUrl { get; set; }

        [JsonProperty("thumb_url")]
        public Uri ThumbUrl { get; set; }

        [JsonProperty("tiny_url")]
        public Uri TinyUrl { get; set; }

        [JsonProperty("original_url")]
        public Uri OriginalUrl { get; set; }
    }

    public partial class Volume
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
