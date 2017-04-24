using System;
using Pathoschild.Stardew.Automate;
using Pathoschild.Stardew.Automate.Framework;
using CustomFarming;
using StardewValley;
using StardewValley.Objects;


namespace CFAutomate
{
    internal class CustomFarmingMachine : IMachine
    {

        private simpleMachine Machine;

        public CustomFarmingMachine()
        {

        }

        public CustomFarmingMachine(simpleMachine machine)
        {
            this.Machine = machine;
        }


        public Item GetOutput()
        {
            return Machine.deliverProduceForAutomation();
        }

        public MachineState GetState()
        {
            if (this.Machine.heldObject == null)
                return MachineState.Empty;

            return this.Machine.readyForHarvest
                ? MachineState.Done
                : MachineState.Processing;
        }

        public bool Pull(Chest[] chests)
        {
            return Machine.pullFromChestForAutomation(chests);
        }

        public void Reset(bool outputTaken)
        {
            this.Machine.resetForAutomation();
        }
    }
}
