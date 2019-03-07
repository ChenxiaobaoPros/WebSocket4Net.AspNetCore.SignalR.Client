using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.Connection;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;

namespace WebSocket4Net.AspNetCore.SignalR.Core.JsonMessageHandlers
{
    public class JsonCloseMessageHandler : IReceivedMessageHandler
    {
        private readonly ILogger _logger;
        public JsonCloseMessageHandler(ILogger<JsonCloseMessageHandler> logger)
        {
            _logger = logger;
        }
    public int MessageTypeId => 7;

    public async Task Handler(string message, ConcurrentDictionary<string, InvocationRequestCallBack<object>> requestCallBacks, ConcurrentDictionary<string, InvocationHandlerList> invocationHandlers, HubConnection hubConnection)
        {
            _logger.LogDebug($"开始处理CloseMessage, Message:{message}");
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var BasicInvocationMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<CloseWithError>(message, settings);
            var error = "服务器关闭了连接";
            if (!string.IsNullOrEmpty(BasicInvocationMessage.Error))
            {
                error = $"服务器关闭了连接,message:{BasicInvocationMessage.Error}";
            }

            _logger.LogError(error);

            await hubConnection.CloseAsync(error);
        }
    }
}
