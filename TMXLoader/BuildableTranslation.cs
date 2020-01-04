using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMXLoader
{
    public class BuildableTranslation
    {
        public string name { get; set; }

        public string set { get; set; } = "Others";

        internal string getSetName()
        {
            if (set != "Others")
                return set;
            else
                return TMXLoaderMod._instance.i18n.Get("Others");
        }

        public static BuildableTranslation FromEdit(BuildableEdit edit)
        {
            var t = new BuildableTranslation();
            t.name = edit.name;
            t.set = edit.set;

            return t;
        }

    }
}
