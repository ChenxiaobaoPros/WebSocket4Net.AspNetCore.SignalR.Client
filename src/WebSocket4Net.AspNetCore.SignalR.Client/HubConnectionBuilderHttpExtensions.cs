using System;
using Microsoft.Extensions.DependencyInjection;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.JsonMessageHandlers;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;
using WebSocket4Net.AspNetCore.SignalRClient.MessageConveter;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol;

namespace WebSocket4Net.AspNetCore.SignalR.Client {
  public static class HubConnectionBuilderHttpExtensions {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hubConnectionBuilder"></param>
    /// <param name="url">hub 的 url</param>
    /// <param name="protocolOption">协议格式选项</param>
    /// <returns></returns>
    public static HubConnectionBuilder WithUrl(this HubConnectionBuilder hubConnectionBuilder, string url, ProtocolOption protocolOption = ProtocolOption.Json) {
      if (hubConnectionBuilder == null) {
        throw new ArgumentNullException(nameof(hubConnectionBuilder));
      }
      var option = new HubConnectionOptions(new Uri(url));
      WithUrlCore(hubConnectionBuilder, option);
      return hubConnectionBuilder;
    }
    public static HubConnectionBuilder WithUrl(this HubConnectionBuilder hubConnectionBuilder, string url, Action<HubConnectionOptions> optionConfig) {
      if (hubConnectionBuilder == null) {
        throw new ArgumentNullException(nameof(hubConnectionBuilder));
      }
      var option = new HubConnectionOptions(new Uri(url));
      optionConfig(option);
      WithUrlCore(hubConnectionBuilder, option);
      return hubConnectionBuilder;
    }

    private static void WithUrlCore(HubConnectionBuilder hubConnectionBuilder, HubConnectionOptions option) {
      hubConnectionBuilder.Services.AddSingleton(option);

      if (option.ProtocolOption == ProtocolOption.Json) {
        hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandlerProvider, JsonReceivedMessageHandlerProvider>();

        hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandler, JsonBasicInvocationMessageHandler>();
        hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandler, JsonCloseMessageHandler>();
        hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandler, JsonCompletionMessageHandler>();
        hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandler, JsonPingMessageHandler>();
        hubConnectionBuilder.Services.AddSingleton<IReceivedMessageHandler, JsonStreamingInvocationMessageHandler>();

        hubConnectionBuilder.Services.AddSingleton<IMessageConventer, JsonMessageConventer>();
      } else {
        throw new NotSupportedException("暂不支持json 之外的协议格式");
      }
    }

    public static HubConnectionBuilder ConfigService(this HubConnectionBuilder hubConnectionBuilder, Action<IServiceCollection> config) {
      config(hubConnectionBuilder.Services);
      return hubConnectionBuilder;
    }
  }
}
