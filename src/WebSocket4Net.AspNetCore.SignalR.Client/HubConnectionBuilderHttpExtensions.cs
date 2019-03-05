using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.JsonMessageHandlers;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;
using WebSocket4Net.AspNetCore.SignalRClient.MessageConveter;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol;

namespace WebSocket4Net.AspNetCore.SignalR.Client
{
    /// <summary>
    /// Extension methods for <see cref="IHubConnectionBuilder"/>.
    /// </summary>
    public static class HubConnectionBuilderHttpExtensions
    {
        /// <summary>
        /// Configures the <see cref="HubConnection" /> to use HTTP-based transports to connect to the specified URL.
        /// </summary>
        /// <param name="hubConnectionBuilder">The <see cref="IHubConnectionBuilder" /> to configure.</param>
        /// <param name="url">The URL the <see cref="HttpConnection"/> will use.</param>
        /// <returns>The same instance of the <see cref="IHubConnectionBuilder"/> for chaining.</returns>
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

                hubConnectionBuilder.Services.AddSingleton<IMessageParser, JsonMessageParser>();
            }
            return hubConnectionBuilder;
        }


    }
}
