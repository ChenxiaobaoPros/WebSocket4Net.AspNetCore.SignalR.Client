using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.Connection;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalR.Core.JsonMessageHandlers
{
    public class JsonCloseMessageHandler : IReceivedMessageHandler
    {
        private readonly ILogger _logger;
        public JsonCloseMessageHandler(ILogger<JsonCloseMessageHandler> logger)
        {
            _logger = logger;
        }
        public int MessageTypeId { get => 7; }

        public async Task Handler(string message, ConcurrentDictionary<string, InvocationRequestCallBack<object>> callBacks, ConcurrentDictionary<string, InvocationHandlerList> invocationHandlers, HubConnection hubConnection)
        {
            var BasicInvocationMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<CloseWithError>(message);
            if (string.IsNullOrEmpty(BasicInvocationMessage.Error))
            {
                _logger.LogError("服务器关闭了连接");
            }
            else
            {
                _logger.LogError($"服务器关闭了连接,message:{BasicInvocationMessage.Error}");
            }
            await hubConnection.StopAsync();
        }
    }
}
