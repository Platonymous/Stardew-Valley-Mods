using StardewValley;

namespace PyTK.Types
{
    public class MPStringMessage
    {
        public string address { get; set; }
        public Farmer sender { get; set; }
        public string message { get; set; }

        public MPStringMessage(string address, Farmer sender, string message)
        {
            this.address = address;
            this.sender = sender;
            this.message = message;
        }
    }
}
