using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalR.Core.JsonMessageHandlers
{
    public class JsonReceivedMessageHandlerProvider : IReceivedMessageHandlerProvider
    {
        private readonly List<IReceivedMessageHandler> _receivedMessageHandlers;
        private readonly ILogger _logger;
        public JsonReceivedMessageHandlerProvider(IServiceProvider serviceProvider, ILogger<JsonReceivedMessageHandlerProvider> logger)
        {
            _receivedMessageHandlers = Array.ConvertAll(serviceProvider.GetServices(typeof(IReceivedMessageHandler)).ToArray(), ins => ins as IReceivedMessageHandler).ToList();
            _logger = logger;
        }
        public IReceivedMessageHandler GetHandler(string message)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var msg = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(message, settings);
                var handler = _receivedMessageHandlers.FirstOrDefault(m => m.MessageTypeId == msg.Type);
                if (handler == null)
                {
                    _logger.LogError($"找不到消息类型匹配的消息处理器,handlerTypeId:{msg.Type}");
                }
                return handler;
            }
            catch (Exception ex)
            {
                var error = $"接收到的消息格式暂不支持,message:{message}";
                _logger.LogError(ex, error);
                throw new NotSupportedException(error, ex);
            }
        }
    }
}
