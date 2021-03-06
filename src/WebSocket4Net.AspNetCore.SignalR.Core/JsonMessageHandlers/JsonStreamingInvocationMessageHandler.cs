using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.Connection;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;

namespace WebSocket4Net.AspNetCore.SignalR.Core.JsonMessageHandlers {
  public class JsonStreamingInvocationMessageHandler : IReceivedMessageHandler {
    private readonly ILogger _logger;
    public JsonStreamingInvocationMessageHandler(ILogger<JsonStreamingInvocationMessageHandler> logger) {
      _logger = logger;
    }
    public int MessageTypeId => 1;

    public async Task Handler(string message, ConcurrentDictionary<string, InvocationRequestCallBack<object>> requestCallBacks, ConcurrentDictionary<string, InvocationHandlerList> invocationHandlers, HubConnection hubConnection) {
      await Task.CompletedTask;
      _logger.LogError("暂不支持 StreamingInvocation");
    }
  }
}
