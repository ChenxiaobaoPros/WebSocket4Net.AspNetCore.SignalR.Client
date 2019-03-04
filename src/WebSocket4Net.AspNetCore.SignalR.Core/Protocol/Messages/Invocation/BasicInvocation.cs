using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages.Invocation
{
    public class BasicInvocation : Message
    {
        public BasicInvocation()
        {

        }
        public BasicInvocation(string invocationId, string target, object[] arguments)
        {
            this.Type = 1;
        }
        [JsonProperty("invocationId")]
        public string InvocationId { get; set; }
        [JsonProperty("target")]
        public string Target { get; set; }
        [JsonProperty("arguments")]
        public object[] Arguments { get; set; }
    }
}
