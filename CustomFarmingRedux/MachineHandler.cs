using System;

namespace CustomFarmingRedux
{
    public class MachineHandler
    {
        public Func<StardewValley.Object, string, string, StardewValley.Object> GetOutput { get; set; } = null;
        public Func<StardewValley.Object, string, bool> CheckInput { get; set; } = null;
        public Action ClickAction { get; set; } = null;


        public MachineHandler(Func<StardewValley.Object, string, string, StardewValley.Object> outputHandler = null, Func<StardewValley.Object, string, bool> inputHandler = null, Action clickActionHandler = null)
        {
            GetOutput = outputHandler;
            CheckInput = inputHandler;
            ClickAction = clickActionHandler;
        }
    }
}
