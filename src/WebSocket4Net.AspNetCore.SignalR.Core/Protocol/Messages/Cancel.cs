using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages
{
   public class Cancel : Message
    {
        public Cancel()
        {
            this.Type = 5;
        }
        [JsonProperty("invocationId")]
        public string InvocationId { get; set; }
    }
}
