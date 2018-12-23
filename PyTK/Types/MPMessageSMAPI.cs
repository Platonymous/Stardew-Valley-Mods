using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyTK.Types
{ 
    class MPMessageSMAPI
    {
        public int DataType { get; set; }
        public int MessageType { get; set; }
        public int AsInt { get; set; }
        public string AsString { get; set; }
        public bool AsBool { get; set; }
        public long AsLong { get; set; }
        public double AsDouble { get; set; }

        public MPMessageSMAPI()
        {

        }

        public MPMessageSMAPI(int dataType, int messageType, object message)
        {
            DataType = dataType;
            MessageType = messageType;

            switch (dataType)
            {
                case 0: AsInt = (int)message; break;
                case 1: AsString = (string)message; break;
                case 2: AsBool = (bool)message; break;
                case 3: AsLong = (long)message; break;
                case 4: AsDouble = (double)message; break;
                default: AsString = message.ToString(); break;
            }
        }

    }
}
