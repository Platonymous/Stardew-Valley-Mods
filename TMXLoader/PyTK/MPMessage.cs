using StardewValley;
using System;

namespace TMXLoader
{
    internal enum MPDataType
    {
        INT = 0,
        STRING = 1,
        BOOL = 2,
        LONG = 3,
        DOUBLE = 4
    }

    public class MPMessage
    {
        public string address { get; set; }
        public Farmer sender { get; set; }
        internal MPDataType dataType { get; set; }
        public object message { get; set; }
        public int type { get; set; }
        public long receiver { get; set; }
        
        public MPMessage()
        {

        }

        public MPMessage(string address, Farmer sender, object message, int type = 0, long toFarmer = -1)
        {
            this.address = address;
            this.sender = sender;
            this.message = message;
            this.type = type;
            dataType = getDataType(message);
            receiver = toFarmer;
        }

        internal MPDataType getDataType(object message)
        {
            if (message is string)
                return MPDataType.STRING;

            if (message is bool)
                return MPDataType.BOOL;

            if (message is int)
            {
                message = (Int32)message;
                return MPDataType.INT;
            }

            if (message is long)
            {
                message = (Int64) message;
                return MPDataType.LONG;
            }
            if (message is double || message is float f || message is decimal)
            {
                message = (double)message;
                return MPDataType.DOUBLE;
            }

            message = message.ToString();
            return MPDataType.STRING;

        }
    }
}
