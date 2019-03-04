using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages.Invocation;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Abstriction
{
    public interface IMessageParser
    {
        string ProtocolName { get; }
        byte[] GetBytes(Message message);
    }
}
