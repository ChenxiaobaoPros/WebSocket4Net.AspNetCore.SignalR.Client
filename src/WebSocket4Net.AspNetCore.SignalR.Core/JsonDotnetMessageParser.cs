using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalRClient.MessageConveter
{
    public class JsonDotnetMessageParser : IMessageParser
    {
        public string ProtocolName { get => "json"; }

        public byte[] GetBytes(Message message)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            return (new List<byte>(Encoding.UTF8.GetBytes(json)) { 0x1e }).ToArray();
        }
        
    }
}
