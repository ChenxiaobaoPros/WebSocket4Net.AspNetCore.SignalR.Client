using Microsoft.Extensions.DependencyInjection;
using System;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.JsonMessageHandlers;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;
using WebSocket4Net.AspNetCore.SignalRClient.MessageConveter;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol;

namespace WebSocket4Net.AspNetCore.SignalR.Client
{
    public static class HubConnectionBuilderHttpExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hubConnectionBuilder"></param>
        /// <param name="url">hub 的 url</param>
        /// <param name="protocolOption">协议格式选项</param>
        /// <returns></returns>
        public static HubConnectionBuilder WithUrl(this HubConnectionBuilder hubConnectionBuilder, string url, ProtocolOption protocolOption = ProtocolOption.Json)
        {
            if (hubConnectionBuilder == null)
            {
                throw new ArgumentNullException(nameof(hubConnectionBuilder));
            }
            HubConnectionOptions option = new HubConnectionOptions();
            option.Uri = new Uri(url);
            hubConnectionBuilder.Services.AddSingleton(option);

            if (protocolOption == ProtocolOption.Json)
            {
                hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandlerProvider, JsonReceivedMessageHandlerProvider>();

                hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandler, JsonBasicInvocationMessageHandler>();
                hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandler, JsonCloseMessageHandler>();
                hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandler, JsonCompletionMessageHandler>();
                hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandler, JsonPingMessageHandler>();
                hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandler, JsonStreamingInvocationMessageHandler>();

                hubConnectionBuilder.Services.AddSingleton<IMessageConventer, JsonMessageConventer>();
            }
            return hubConnectionBuilder;
        }


    }
}
