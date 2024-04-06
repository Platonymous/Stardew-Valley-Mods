namespace TMXLoader
{
    public class ContentResponse
    {
        public string assetName { get; set; }
        public string content { get; set; }
        public bool toGameContent { get; set; }
        public int type { get; set; }

        public ContentResponse()
        {

        }

        public ContentResponse(string assetName, int type, string content, bool toGameContent)
        {
            this.assetName = assetName;
            this.content = content;
            this.toGameContent = toGameContent;
            this.type = type;
        }
    }
}
