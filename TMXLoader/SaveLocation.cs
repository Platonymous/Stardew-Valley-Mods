using System.Collections.Generic;

namespace TMXLoader
{
    public class SaveLocation
    {
        public string Objects { get; set; }
        public string Name { get; set; }
        public SaveLocation(string name, string objects)
        {
            Objects = objects;
            Name = name;
        }
    }
}
