namespace PyTK.CustomElementHandler
{
    public class CODSync
    {
        public string Id { get; set; }
        public int Index { get; set; }

        public CODSync()
        {

        }

        public CODSync(string id, int index)
        {
            Id = id;
            Index = index;
        }
    }
}
