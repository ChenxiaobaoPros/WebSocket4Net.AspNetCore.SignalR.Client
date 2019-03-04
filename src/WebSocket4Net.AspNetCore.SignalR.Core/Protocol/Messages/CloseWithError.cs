using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages
{
    class CloseWithError: Close
    {   
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
