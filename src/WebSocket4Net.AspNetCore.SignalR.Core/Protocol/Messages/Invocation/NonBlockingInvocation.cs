using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages.Invocation
{
    public class NonBlockingInvocation : Message
    {
        public NonBlockingInvocation(string targrt, object[] arguments)
        {
            this.Type = 1;
            this.Arguments = arguments;
            this.Target = targrt;
        }
        [JsonProperty("target")]
        public string Target { get; set; }
        [JsonProperty("arguments")]
        public object[] Arguments { get; set; }
    }
}
