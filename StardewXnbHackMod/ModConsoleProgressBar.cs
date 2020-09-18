using System;
using StardewModdingAPI;
using StardewXnbHack.Framework;

namespace StardewXnbHackMod
{
    /// <summary>Manages a progress bar written to the console.</summary>
    internal class ModConsoleProgressBar : ConsoleProgressBar
    { /*********
        ** Fields
        *********/
        /// <summary>The mod monitor to print to.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The original Console title to restore.</summary>
        private readonly string Title;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">The mod monitor to print to.</param>
        /// <param name="totalSteps">The total number of steps to perform.</param>
        public ModConsoleProgressBar(IMonitor monitor, int totalSteps, string title)
            : base(totalSteps)
        {
            this.Monitor = monitor;
            this.Title = title;
        }

        /// <summary>Print a progress bar to the console.</summary>
        /// <param name="message">The message to print.</param>
        /// <param name="removePrevious">Whether to remove the previously output progress bar.</param>
        public override void Print(string message, bool removePrevious = true)
        {
            int percentage = (int)((this.CurrentStep / (this.TotalSteps * 1m)) * 100);

            Console.Title = ($"StardewXNBHack is unpacking files: [{"".PadRight(percentage / 10, '#')}{"".PadRight(10 - percentage / 10, ' ')} {percentage}%]  {message}");

            if (this.CurrentStep == this.TotalSteps)
                Console.Title = this.Title;
        }
    }
}
