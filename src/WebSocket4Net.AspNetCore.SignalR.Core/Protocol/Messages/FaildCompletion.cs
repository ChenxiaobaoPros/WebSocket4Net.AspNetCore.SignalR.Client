using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages
{
    class FaildCompletion : Completion
    {
        public FaildCompletion(string invocationId,string errorMessage)
        {
            this.InvocationId = invocationId;
            this.Error = errorMessage;
        }
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
