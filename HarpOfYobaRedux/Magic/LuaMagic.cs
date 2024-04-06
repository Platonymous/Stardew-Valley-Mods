using StardewModdingAPI;
using System.IO;

namespace HarpOfYobaRedux
{
    class LuaMagic : IMagic
    {
        IModHelper helper;

        public LuaMagic(IModHelper helper)
        {
            this.helper = helper;
        }


        public void doMagic(bool playedToday)
        {
            //PyLua.loadScriptFromFile(Path.Combine(helper.DirectoryPath, "Assets", "luamagic.lua"), "luaMagic");
            //PyLua.callFunction("luaMagic", "doMagic", playedToday);
        }

       
    }
}
