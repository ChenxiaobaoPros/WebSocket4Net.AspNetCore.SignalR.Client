using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages
{
    class SuccessfulCompletion : Completion
    {
        public SuccessfulCompletion(string invocationId,string result="")
        {
            this.InvocationId = invocationId;
            this.Result = result;
        }
        [JsonProperty("result")]
        public object Result { get; set; } = "";
    }
}
