using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages
{
    public class Completion : Message
    {
        public Completion()
        {
            this.Type = 3;
        }
        [JsonProperty("invocationId")]
        public string InvocationId { get; set; }
    }
}
