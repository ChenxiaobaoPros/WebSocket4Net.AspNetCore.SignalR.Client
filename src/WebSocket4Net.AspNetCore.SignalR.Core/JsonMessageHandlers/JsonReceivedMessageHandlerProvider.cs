using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
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
        public JsonReceivedMessageHandlerProvider(IServiceProvider serviceProvider)
        {
            _receivedMessageHandlers = Array.ConvertAll(serviceProvider.GetServices(typeof(IReceivedMessageHandler)).ToArray(), ins => ins as IReceivedMessageHandler).ToList();
        }
        public IReceivedMessageHandler GetHandler(string message)
        {
            try
            {
                var mes = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(message);

                return _receivedMessageHandlers.FirstOrDefault(m => m.MessageTypeId == mes.Type) ?? throw new Exception($"找不到消息类型匹配的消息处理器,handlerTypeId:{mes.Type}");
            }
            catch (Exception ex)
            {
                throw new NotSupportedException($"接收到的消息格式暂不支持,message:{message}");
            }

        }
    }
}
