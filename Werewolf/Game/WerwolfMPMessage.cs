using System;
using System.Collections.Generic;
using System.Linq;

namespace Werewolf.Game
{
    public abstract class WerwolfMPMessage
    {
        public abstract string Type { get; set; }

        public long SendTo { get; set; }

        public long SendFrom { get; set; }

        public string GameID { get; set; }

        public string Callback {get; set;}

        public WerwolfMPMessage()
        {

        }

        public WerwolfMPMessage(long sendTo, long sentFrom, WerwolfGame game, string callbackid = null, Action<string, string> callback = null, Action onDisconnect = null, Action onTimeout = null, int timeout = -1)
        {
            SendFrom = sentFrom;
            SendTo = sendTo;
            GameID = game.GameID;
            if (callbackid != null)
            {
                Callback = callbackid;
                game.AddCallback(new WerwolfCallbackRequest(sendTo, callbackid, callback, onDisconnect, onTimeout, timeout));
            }
        }

        public WerwolfMPMessage(long sendTo, long sentFrom, WerwolfClientGame game, string callbackid = null, Action<string, List<string>> callback = null)
        {
            SendFrom = sentFrom;
            SendTo = sendTo;
            GameID = game.GameID;
        }
    }
}

