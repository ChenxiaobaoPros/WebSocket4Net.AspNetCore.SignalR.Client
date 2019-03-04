using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages
{
    public class StreamItem : Message
    {
        public StreamItem()
        {
            this.Type = 1;
        }
        [JsonProperty("invocationId")]
        public string InvocationId { get; set; }
        [JsonProperty("item")]
        public int Item { get; set; }
    }
}
