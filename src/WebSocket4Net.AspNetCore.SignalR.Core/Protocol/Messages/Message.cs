using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class Message
    {
        [JsonProperty("type")]
        public int Type { get; set; } = 6;
        [JsonProperty("headers")]
        public object Headers { get; set; } = new object();
    }
}
