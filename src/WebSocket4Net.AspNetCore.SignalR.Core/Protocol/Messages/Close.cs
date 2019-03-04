using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages
{
    class Close : Message
    {
        public Close()
        {
            this.Type = 7;
        }
        [JsonProperty("invocationId")]
        public string InvocationId { get; set; }
    }
}
