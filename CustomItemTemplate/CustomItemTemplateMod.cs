using StardewModdingAPI;

namespace CustomitemTemplate
{
    public class CustomItemTemplateMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            CustomItem.init(helper);
        }       
    }
}
