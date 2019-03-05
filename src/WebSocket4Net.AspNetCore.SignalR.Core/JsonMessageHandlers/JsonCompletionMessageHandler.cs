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
        public int MessageTypeId { get => 3; }

        public async Task Handler(string message, ConcurrentDictionary<string, InvocationRequestCallBack<object>> callBacks, ConcurrentDictionary<string, InvocationHandlerList> invocationHandlers, HubConnection hubConnection)
        {
            await Task.CompletedTask;
            var mes = Newtonsoft.Json.JsonConvert.DeserializeObject<CompletionWithBothErrorAndResult>(message);
            if (!callBacks.TryRemove(mes.InvocationId, out InvocationRequestCallBack<object> callback))
            {
                throw new Exception($"InvocationId={mes.InvocationId} 在当前 回调池里不存在"); //warn
            }
            // 无回调
            if (callback.Invoke == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(mes.Error))
            {
                callback.Invoke(mes.Result, new Exception(mes.Error));
                return;
            }
            callback.Invoke(mes.Result, null);
        }
    }
}
