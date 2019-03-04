using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class CompletionWithBothErrorAndResult : Completion
    {
        [JsonProperty("result")]
        public object Result { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
