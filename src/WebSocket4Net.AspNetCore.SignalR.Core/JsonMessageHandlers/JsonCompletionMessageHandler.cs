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
    public class JsonCompletionMessageHandler : IReceivedMessageHandler
    {
        private readonly ILogger _logger;
        public JsonCompletionMessageHandler(ILogger<JsonCompletionMessageHandler> logger)
        {
            _logger = logger;
        }
    public int MessageTypeId => 3;

    public async Task Handler(string message, ConcurrentDictionary<string, InvocationRequestCallBack<object>> requestCallBacks, ConcurrentDictionary<string, InvocationHandlerList> invocationHandlers, HubConnection hubConnection)
        {
            _logger.LogInformation($"开始处理CompletionMessage, Message:{message}");
            await Task.CompletedTask;
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var mes = Newtonsoft.Json.JsonConvert.DeserializeObject<CompletionWithBothErrorAndResult>(message, settings);
            if (!requestCallBacks.TryRemove(mes.InvocationId, out InvocationRequestCallBack<object> callback))
            {
                throw new Exception($"InvocationId={mes.InvocationId} 在当前 回调池里不存在"); //warn
            }
            // 无回调
            if (callback.Invoke == null)
            {
                return;
            }
            try
            {
                var modelJson = JsonConvert.SerializeObject(mes.Result, settings);
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject(modelJson, callback.ReturnType, settings);
                if (!string.IsNullOrEmpty(mes.Error))
                {
                    callback.Invoke(model, new Exception(mes.Error));
                    return;
                }
                callback.Invoke(model, null);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"回调失败");
            }         
        }
    }
}
