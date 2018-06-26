using PyTK.Lua;
using System.IO;

namespace HarpOfYobaRedux
{
    class LuaMagic : IMagic
    {
        public LuaMagic()
        {

        }


        public void doMagic(bool playedToday)
        {
            PyLua.loadScriptFromFile(Path.Combine(HarpOfYobaReduxMod.helper.DirectoryPath, "Assets", "luamagic.lua"), "luaMagic");
            PyLua.callFunction("luaMagic", "doMagic", playedToday);
        }

       
    }
}
