
namespace TMXLoader
{
    public class ContentRequest
    {
        public int type { get; set; }
        public string assetName { get; set; }
        public bool fromGameContent { get; set; }

        public ContentRequest()
        {

        }

        public ContentRequest(ContentType type, string assetName, bool fromGameContent)
        {
            this.type = (int) type;
            this.assetName = assetName;
            this.fromGameContent = fromGameContent;
        }
    }
}
