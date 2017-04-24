using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using CustomFarming;
using Pathoschild.Stardew.Automate;
using System.Collections.Generic;
using Pathoschild.Stardew.Automate.Framework;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System.Linq;

namespace CFAutomate
{
    public class CFAutomateMod : Mod
    {

        private IDictionary<GameLocation, MachineMetadata[]> Machines = new Dictionary<GameLocation, MachineMetadata[]>();
        private bool IsReady => Game1.hasLoadedGame && this.Machines.Any();

        public override void Entry(IModHelper helper)
        {
            LocationEvents.LocationsChanged += LocationEvents_LocationsChanged;
            LocationEvents.LocationObjectsChanged += LocationEvents_LocationObjectsChanged;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;

        } 

        public static bool TryPush(Chest[] chests, Item item)
        {
            if (item == null)
                return false;

            foreach (Chest chest in chests)
            {
                if (chest.addItem(item) == null)
                    return true;
            }
            return false;
        }

        private void ProcessMachines(MachineMetadata[] machines)
        {
            foreach (MachineMetadata metadata in machines)
            {
                IMachine machine = metadata.Machine;

                switch (machine.GetState())
                {
                    case MachineState.Empty:
                        
                            machine.Pull(metadata.Connected);
  
                        break;

                    case MachineState.Done:
                        
                            if (TryPush(metadata.Connected, machine.GetOutput()))
                                machine.Reset(true);
                      
                            break;
                }
            }
        }

        private void addSimpleMachinesToAutomation()
        {


            foreach (simpleMachine obj in simpleMachine.allMachines)
            {

                if (obj.produce == null) { continue; }

                if (obj.Environment.objects.ContainsKey(obj.Position) && obj.Environment.objects[obj.Position] == obj)
                {
                    List<Chest> connectedChests = new List<Chest>();

                    foreach (Vector2 connectedTile in Utility.getSurroundingTileLocationsArray(obj.Position))
                    {
                        if (obj.Environment.objects.ContainsKey(connectedTile) && obj.Environment.objects[connectedTile] is Chest)
                        {
                            connectedChests.Add((Chest)obj.Environment.objects[connectedTile]);

                        }

                    }

                    if (connectedChests.Count == 0) { continue; }

                    MachineMetadata newMachine = new MachineMetadata(obj.Environment, connectedChests.ToArray(), new CustomFarmingMachine(obj));

                    if (this.Machines.ContainsKey(obj.Environment))
                    {
                        List<MachineMetadata> machineData = new List<MachineMetadata>(this.Machines[obj.Environment]);
                        machineData.Add(newMachine);
                        this.Machines[obj.Environment] = machineData.ToArray();
                    }
                    else
                    {
                        this.Machines.Add(obj.Environment, new MachineMetadata[] { newMachine });
                    }
                }

            }
        }


        private void LocationEvents_LocationObjectsChanged(object sender, EventArgsLocationObjectsChanged e)
        {
            addSimpleMachinesToAutomation();
        }

        private void LocationEvents_LocationsChanged(object sender, EventArgsGameLocationsChanged e)
        {
            addSimpleMachinesToAutomation();
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            if (!this.IsReady)
                return;

            foreach (MachineMetadata[] machines in this.Machines.Values)
                this.ProcessMachines(machines);
        }
    }
}
