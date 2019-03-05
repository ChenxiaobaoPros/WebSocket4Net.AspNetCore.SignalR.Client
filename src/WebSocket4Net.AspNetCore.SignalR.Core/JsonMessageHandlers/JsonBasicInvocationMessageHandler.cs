using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.Connection;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;

namespace WebSocket4Net.AspNetCore.SignalR.Core.JsonMessageHandlers
{
    public class JsonBasicInvocationMessageHandler : IReceivedMessageHandler
    {
        private readonly ILogger _logger;
        public JsonBasicInvocationMessageHandler(ILogger<JsonBasicInvocationMessageHandler> logger)
        {
            _logger = logger;
        }
        public int MessageTypeId { get => 1; }

        public async Task Handler(string message, ConcurrentDictionary<string, InvocationRequestCallBack<object>> requestCallBacks, ConcurrentDictionary<string, InvocationHandlerList> invocationHandlers, HubConnection hubConnection)
        {
            _logger.LogInformation($"开始处理BasicInvocation, Message:{message}");
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var BasicInvocationMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<BasicInvocation>(message, settings);
            if (BasicInvocationMessage.Arguments.Length > 1)
            {
                _logger.LogError($"多个参数的远程调用暂不支持,message:{message}");
                await hubConnection.SendFaildCompletionAsync(BasicInvocationMessage.InvocationId, "多个参数的远程调用暂不支持");
                return;
            }
            if (invocationHandlers.TryGetValue(BasicInvocationMessage.Target, out InvocationHandlerList invocationHandlerList))
            {
                var handlers = invocationHandlerList.GetHandlers();
                try
                {
                    foreach (var handler in handlers)
                    {
                        var modelJson = Newtonsoft.Json.JsonConvert.SerializeObject(BasicInvocationMessage.Arguments[0]);
                        var model = Newtonsoft.Json.JsonConvert.DeserializeObject(modelJson, handler.ReturnType);
                        await handler.InvokeAsync(model);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"message:{message}");
                    if (!string.IsNullOrEmpty(BasicInvocationMessage.InvocationId))
                    {
                        await hubConnection.SendFaildCompletionAsync(BasicInvocationMessage.InvocationId, ex.Message);
                        return;
                    }
                }           
            }
            if (!string.IsNullOrEmpty(BasicInvocationMessage.InvocationId))
            {
                await hubConnection.SendSuccessfulCompletionAsync(BasicInvocationMessage.InvocationId);
                return;
            }

            _logger.LogWarning($"没有找到匹配的 InvocationHandler,message:{message}");
        }
    }
}

