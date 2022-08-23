using PlatoTK;
using StardewModdingAPI;
using StardewValley.Objects;

namespace Arcade2048
{
    public class Arcade2048Mod : Mod
    {

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            //string sdata = "2048/0/-300/Crafting -9/Play '2048 by Platonymous' at home!/true/true/0/2048";

            if (Helper.ModRegistry.GetApi<PlatoTK.APIs.ISerializerAPI>("Platonymous.Toolkit") is PlatoTK.APIs.ISerializerAPI pytk)
            {
                pytk.AddPostDeserialization(ModManifest, (o) =>
                {
                    var data = pytk.ParseDataString(o);

                    if (o is Chest c && data.ContainsKey("@Type") && data["@Type"].Contains("Machine2048"))
                    {
                        return Machine2048.GetNew(c);
                    }

                    return o;
                });
            }

            Helper.GetPlatoHelper().Presets.RegisterArcade(
                id: "2048",
                name: "2048",
                objectName: "2048 Arcade Machine",
                start: () => Machine2048.start(Helper),
                sprite: Helper.ModContent.GetInternalAssetName("assets/arcade.png").Name,
                iconForMobilePhone: Helper.ModContent.GetInternalAssetName("assets/mobile_app_icon.png").Name
            );
        }
    }
}
