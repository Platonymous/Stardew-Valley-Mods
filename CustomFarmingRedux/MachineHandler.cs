using System;

namespace CustomFarmingRedux
{
    public class MachineHandler
    {
        public Func<StardewValley.Object, StardewValley.Object, string, string, StardewValley.Object> GetOutput { get; set; } = null;
        public Func<StardewValley.Object, StardewValley.Object, string, bool> CheckInput { get; set; } = null;
        public Action<StardewValley.Object> ClickAction { get; set; } = null;


        public MachineHandler(Func<StardewValley.Object, StardewValley.Object, string, string, StardewValley.Object> outputHandler = null, Func<StardewValley.Object, StardewValley.Object, string, bool> inputHandler = null, Action<StardewValley.Object> clickActionHandler = null)
        {
            GetOutput = outputHandler;
            CheckInput = inputHandler;
            ClickAction = clickActionHandler;
        }
    }
}
