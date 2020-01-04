using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyTK.APIs
{
    /// <summary>An arbitrary class to handle token logic.</summary>
    internal class CustomObjectToken
    {
        /*********
        ** Fields
        *********/

        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the token is ready to use.</summary>
        public bool IsReady()
        {
            return Context.IsWorldReady;
        }

        /// <summary>Update the token value.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public bool UpdateContext()
        {
            if (PyTK.PyTKMod.UpdateCustomObjects)
                return true;
            else
                PyTKMod.UpdateCustomObjects = false;

            return false;
        }

        /// <summary>Get the token value.</summary>
        /// <param name="input">The input argument passed to the token, if any.</param>
        public IEnumerable<string> GetValue(string input)
        {
            string[] request = input.Split(':');
            yield return
                (request.Length >= 2) ?
                PyTK.PyUtils.getItem(request[0], -1, request[1]) is StardewValley.Object obj ? obj.ParentSheetIndex.ToString() : "" :
                PyTK.PyUtils.getItem("Object", -1, request[0]) is StardewValley.Object obj2 ? obj2.ParentSheetIndex.ToString() : "";
        }

    }
}
