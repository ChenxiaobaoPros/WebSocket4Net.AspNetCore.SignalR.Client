using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages.Invocation
{
    public class StreamInvocation : Message
    {
        public StreamInvocation()
        {
            this.Type = 4;
        }
        [JsonProperty("invocationId")]
        public string InvocationId { get; set; }
        [JsonProperty("target")]
        public string Target { get; set; }
        [JsonProperty("arguments")]
        public object[] Arguments { get; set; }
    }
}
