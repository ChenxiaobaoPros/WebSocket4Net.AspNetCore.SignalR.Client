using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol;

namespace WebSocket4Net.AspNetCore.SignalRClient.Connection
{
    public class HubConnectionOptions
    {
        public Uri Uri { get; set; }
        public ProtocolOption ProtocolOption { get; set; } = ProtocolOption.Json;
    }
}
