using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.Connection;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages.Invocation;

namespace WebSocket4Net.AspNetCore.SignalR.Core.JsonMessageHandlers
{
    public class JsonBasicInvocationMessageHandler : IReceivedMessageHandler
    {
        public int MessageTypeId { get => 1; }

        public async Task Handler(string message, ConcurrentDictionary<string, InvocationRequestCallBack<object>> callBacks, ConcurrentDictionary<string, InvocationHandlerList> invocationHandlers, HubConnection hubConnection)
        {
            var BasicInvocationMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<BasicInvocation>(message);
            if (BasicInvocationMessage.Arguments.Length > 1)
            {
                await hubConnection.SendFaildCompletionAsync(BasicInvocationMessage.InvocationId, "多个参数的远程调用暂不支持");
            }
            if (invocationHandlers.TryGetValue(BasicInvocationMessage.Target, out InvocationHandlerList invocationHandlerList))
            {
                var handlers = invocationHandlerList.GetHandlers();
                foreach (var handler in handlers)
                {
                    try
                    {
                        var modelJson = Newtonsoft.Json.JsonConvert.SerializeObject(BasicInvocationMessage.Arguments[0]);
                        var model = Newtonsoft.Json.JsonConvert.DeserializeObject(modelJson, handler.ReturnType);
                        await handler.InvokeAsync(model);
                    }
                    catch (Exception ex)
                    {
                        await hubConnection.SendFaildCompletionAsync(BasicInvocationMessage.InvocationId, ex.Message);
                    }
                }
                if (!string.IsNullOrEmpty(BasicInvocationMessage.InvocationId))
                {
                    await hubConnection.SendSuccessfulCompletionAsync(BasicInvocationMessage.InvocationId);
                }
            }
        }
    }
}
