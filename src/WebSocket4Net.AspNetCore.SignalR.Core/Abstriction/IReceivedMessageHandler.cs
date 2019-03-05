using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using WebSocket4Net.AspNetCore.SignalR.Core.Connection;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Abstriction
{
    public interface IReceivedMessageHandler
    {
        int MessageTypeId { get; }
        Task Handler(string message, ConcurrentDictionary<string, InvocationRequestCallBack<object>> requestCallBacks, ConcurrentDictionary<string, InvocationHandlerList> invocationHandlers, HubConnection hubConnection);
    }
}
