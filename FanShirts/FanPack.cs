using PyTK.Types;
using System.Collections.Generic;

namespace FanShirts
{
    public class FanPack : IContentPack
    {
        public string name { get; set; }
        public string version { get; set; }
        public string author { get; set; }
        public string folderName { get; set; }
        public string fileName { get; set; }
        public string id { get; set; }
        public List<Jersey> jerseys { get; set; }
    }
}
