using System.Collections.Generic;

namespace PlanImporter
{
    public class Import
    {
        public string id { get; set; } = "";
        public List<ImportTile> tiles { get; set; }
        public List<ImportTile> buildings { get; set; }
    }

    public class ImportTile
    {
        public string type { get; set; }
        public string y { get; set; }
        public string x { get; set; }
    }
}
