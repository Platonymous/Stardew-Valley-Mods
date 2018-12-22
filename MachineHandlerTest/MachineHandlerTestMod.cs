using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace MachineHandlerTest
{
    public interface IHandlerAPI
    {
        void setOutputHandler(string machineId, Func<StardewValley.Object, string, string, StardewValley.Object> outputHandler);
        void setInputHandler(string machineId, Func<StardewValley.Object, string, bool> inputHandler);
        void setClickHandler(string machineId, Action clickHandler);
    }

    public class MachineHandlerTestMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            IHandlerAPI api = this.Helper.ModRegistry.GetApi<IHandlerAPI>("Platonymous.CustomFarming");

            //Change all outputs to Crab Pot
            api.setOutputHandler("Platonymous.NewMachines.NewMachines.json.0", (o, m, r) =>
            {
                Monitor.Log("Serving Cran Pot");
                return new CrabPot(Vector2.Zero, 1);
            });

            //Prevent the machine from accepting regular milk
            api.setInputHandler("Platonymous.NewMachines.NewMachines.json.0", (o, m) =>
            {
                return o.ParentSheetIndex != 184;
            });

            //Post log when clicked
            api.setClickHandler("Platonymous.NewMachines.NewMachines.json.0", () => Monitor.Log("Clicked Butter Churn",LogLevel.Info));
        }
    }
}
