using System;
using System.Collections.Generic;
using System.Linq;
using CustomFarmingRedux;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate;
using PyTK;
using StardewValley;

namespace CFAutomate.Framework
{
    /// <summary>An automated wrapper for a CFR machine.</summary>
    internal class AutomatedMachine : IMachine
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying CFR machine.</summary>
        private readonly CustomMachine Machine;


        /*********
        ** Accessors
        *********/
        /// <summary>The location which contains the machine.</summary>
        public GameLocation Location { get; }

        /// <summary>The tile area covered by the machine.</summary>
        public Rectangle TileArea { get; }

        /// <summary>A unique ID for the machine type.</summary>
        /// <remarks>This value should be identical for two machines if they have the exact same behavior and input logic. For example, if one machine in a group can't process input due to missing items, Automate will skip any other empty machines of that type in the same group since it assumes they need the same inputs.</remarks>
        public string MachineTypeID => this.Machine.id;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="machine">The underlying CFR machine.</param>
        /// <param name="location">The location containing the machine.</param>
        public AutomatedMachine(CustomMachine machine, GameLocation location)
        {
            this.Machine = machine;
            this.Location = location;
            this.TileArea = new Rectangle((int)machine.TileLocation.X, (int)machine.TileLocation.Y, 1, 1);
        }

        /// <summary>Get the machine's processing state.</summary>
        public MachineState GetState()
        {
            if (this.Machine.heldObject.Value == null)
                return MachineState.Empty;

            return this.Machine.readyForHarvest.Value
                ? MachineState.Done
                : MachineState.Processing;
        }

        // <summary>Get the output item.</summary>
        public ITrackedStack GetOutput()
        {
            return new TrackedItem(this.Machine.heldObject.Value, onEmpty: item => this.Machine.clear());
        }

        /// <summary>Provide input to the machine.</summary>
        /// <param name="input">The available items.</param>
        /// <returns>Returns whether the machine started processing an item.</returns>
        public bool SetInput(IStorage input)
        {
            if (!Machine.blueprint.production.Exists(p => p.materials != null && p.materials.Count > 0))
                return false;

            if (Machine.blueprint.conditionaldropin && !Machine.checkedToday)
            {
                Machine.meetsConditions = PyUtils.CheckEventConditions(Machine.conditions, this);
                Machine.checkedToday = true;
            }

            if (Machine.blueprint.conditionaldropin && !Machine.meetsConditions)
                return false;

            List<IConsumable> consumables = new List<IConsumable>();
            IConsumable starterItem = null;

            if (Machine.blueprint.starter is IngredientBlueprint ib && Machine.starterRecipe is RecipeBlueprint srb && !lookForIngredient(srb, ib, input, out starterItem))
                return false;

            if (starterItem is IConsumable)
                consumables.Add(starterItem);

            List<RecipeBlueprint> checkedRecipes = new List<RecipeBlueprint>();
            List<IngredientBlueprint> checkedIngredients = new List<IngredientBlueprint>();

            RecipeBlueprint recipe = null;
            StardewValley.Object dropIn = null;

            foreach (ITrackedStack item in input.GetItems())
            {
                if (item.Sample is StardewValley.Object obj && Machine.findRecipeForItemType(obj) is RecipeBlueprint rb && !checkedRecipes.Contains(rb))
                {
                    checkedRecipes.Add(rb);
                    List<IConsumable> iMaterials = new List<IConsumable>();
                    bool hasAll = true;

                    foreach (IngredientBlueprint ing in rb.materials)
                    {
                        IConsumable nextItem = null;
                        if (!checkedIngredients.Contains(ing) && lookForIngredient(rb, ing, input, out nextItem))
                            iMaterials.Add(nextItem);
                        else
                        {
                            if (!checkedIngredients.Contains(ing))
                                checkedIngredients.Add(ing);
                            hasAll = false;
                            break;
                        }
                    }

                    if (hasAll)
                    {
                        recipe = rb;
                        dropIn = obj;
                        consumables.AddRange(iMaterials);
                        break;
                    }
                }
            }

            if (recipe is RecipeBlueprint foundRecipe)
            {
                List<Item> materials = new List<Item>();
                foreach (IConsumable con in consumables)
                    materials.Add(con.Take());
                Machine.startProduction(dropIn, recipe, new List<IList<Item>>() { materials });
                return true;
            }

            return false;
        }

        public bool lookForIngredient(RecipeBlueprint srb, IngredientBlueprint ib, IStorage input, out IConsumable consumable)
        {
            return input.TryGetIngredient((t) =>
            {
                var item = t.Sample;
                item.Stack = t.Count;
                if (item is StardewValley.Object obj)
                    return srb.fitsIngredient(item, srb.materials);
                return false;
            }, ib.stack, out consumable);
        }
    }
}