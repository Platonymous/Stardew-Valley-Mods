namespace PyTK.Types
{
    public interface IContentPack
    {
        string name { get; set; }
        string version { get; set; }
        string author { get; set; }
        string folderName { get; set; }
        string fileName { get; set; }
    }
}
