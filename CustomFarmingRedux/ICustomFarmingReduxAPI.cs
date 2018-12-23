using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomFarmingRedux
{
    public interface ICustomFarmingReduxAPI : ICustomContentAPI
    {
        RecipeBlueprint findRecipe(CustomMachineBlueprint blueprint, List<Item> items);
        Item maxed(Item obj);

        /// <summary>Returns the name, Texture and required stack of all machines that can use the specified item</summary>
        /// <param name="item">The item to be used as material.</param>
        List<Tuple<string, Texture2D, int, int>> getMachinesForItem(StardewValley.Object item);

        /// <summary>Returns the respective machine and draw specs of a custom object dummy item</summary>
        /// <param name="dummy">The dummy item that would be replaced by the custom item</param>
        Tuple<Item, Texture2D, Rectangle, Color> getRealItemAndTexture(StardewValley.Object dummy);

        /// <summary>Get whether a given item is a custom object or machine from Custom Farming Redux.</summary>
        /// <param name="item">The item instance.</param>
        bool isCustom(Item item);

        /// <summary>Get the spritesheet texture for a custom object or machine (if applicable).</summary>
        /// <param name="item">The item instance.</param>
        Texture2D getSpritesheet(Item item);

        /// <summary>Get the spritesheet source area for a custom object or machine (if applicable).</summary>
        /// <param name="item">The item instance.</param>
        Rectangle? getSpriteSourceArea(Item item);

        /// <summary>Add an Output Handler to a machine</summary>
        /// <param name="machineId">Id of the machine that this should handle</param>
        /// <param name="outputHandler">The Output Handler that returns the output Func(StardewValley.Object dropIn, StardewValley.Object machine, string machineid, string recipeName)</param>
        void setOutputHandler(string machineId, Func<StardewValley.Object, StardewValley.Object, string, string, StardewValley.Object> outputHandler);

        /// <summary>Add a Check Input Handler to a machine</summary>
        /// <param name="machineId">Id of the machine that this should handle</param>
        /// <param name="inputHandler">The Input Handler that returns whether or not to accept an input Func(StardewValley.Object dropIn, StardewValley.Object machine, string machineid)</param>
        void setInputHandler(string machineId, Func<StardewValley.Object, StardewValley.Object, string, bool> inputHandler);

        /// <summary>Add Click Action Handler to a machine</summary>
        /// <param name="machineId">Id of the machine that this should handle</param>
        /// <param name="clickHandler">The Action invoked when clicking the machine Action(StardewValley.Object machine)</param>
        void setClickHandler(string machineId, Action<StardewValley.Object> clickHandler);
    }
}
