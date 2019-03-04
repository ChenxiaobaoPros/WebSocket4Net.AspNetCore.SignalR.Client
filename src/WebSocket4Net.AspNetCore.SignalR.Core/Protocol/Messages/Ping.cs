using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages
{
    public class Ping : Message
    {
        public static Ping Instance = new Ping();
        public Ping()
        {
            this.Type = 6;
        }
        [JsonIgnore]
        public new object Headers { get; set; }
    }
}
