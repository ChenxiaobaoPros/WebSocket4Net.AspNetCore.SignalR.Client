using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages
{
    public class Handshake : Message
    {
        public Handshake(string protocol)
        {
            this.Protocol = protocol;
        }
        [JsonProperty("protocol")]
        public string Protocol { get; set; }
        [JsonProperty("version")]
        public int Version { get; set; } = 1;

        [JsonIgnore]
        public new object Headers { get; set; }
    }
}
