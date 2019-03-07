using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using WebSocket4Net.AspNetCore.SignalR.Core;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.Connection;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol;

namespace WebSocket4Net.AspNetCore.SignalRClient.Connection {
  public class HubConnection : IDisposable {
    // 连接超时
    public static readonly TimeSpan DefaultServerTimeout = TimeSpan.FromSeconds(45);
    // 定时 发送 ping 防止长连接关闭  
    public static readonly TimeSpan DefaultKeepAliveInterval = TimeSpan.FromSeconds(15);
    // 保证同一时间只有一个消息发送
    private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
    // 日志
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    // 绑定的方法集合 给远程调用
    private ConcurrentDictionary<string, InvocationHandlerList> _handlers = new ConcurrentDictionary<string, InvocationHandlerList>(StringComparer.Ordinal);
    // Message 转成字节发送
    private readonly IMessageConventer _messageConverter;
    // 当前 connection 绑定的 HubUri
    private readonly Uri _hubUri;
    // 调用的id,客户端用来查找到指定的请求 回馈
    private long _currentInvocationId = 1;
    // Invoke 的回调集合
    private ConcurrentDictionary<string, InvocationRequestCallBack<object>> _sendedMessageCallBacks = new ConcurrentDictionary<string, InvocationRequestCallBack<object>>();
    // 收到的消息处理者Provider
    private readonly IReceivedMessageHandlerProvider _receivedMessageHandlerProvider;
    private Timer _sendedMessageCallBacksCleanerTimer;
    private Timer _sendedPingMessageTimer;

    private long _nextActivationServerTimeout;
    private long _nextActivationSendPing;
    private bool _disposed;
    private bool _isStart;

    private WebSocket _webSocket;


    public event Func<Exception, Task> Closed;

    string GetCurrentInvocationId {
      get {
        lock (_hubUri) {
          var currentInvocationId = _currentInvocationId;
          _currentInvocationId++;
          return currentInvocationId.ToString();
        }
      }
    }

    public TimeSpan ServerTimeout { get; set; } = DefaultServerTimeout;

    public TimeSpan KeepAliveInterval { get; set; } = DefaultKeepAliveInterval;

    public HubConnection(IServiceProvider serviceProvider, ILogger<HubConnection> logger) {
      _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
      _logger = logger;
      var hubOptions = serviceProvider.GetService(typeof(HubConnectionOptions)) as HubConnectionOptions ?? throw new ArgumentException("找不到默认的 Hub 配置");

      _receivedMessageHandlerProvider = serviceProvider.GetService(typeof(IReceivedMessageHandlerProvider)) as IReceivedMessageHandlerProvider;
      _messageConverter = serviceProvider.GetService(typeof(IMessageConventer)) as IMessageConventer ?? throw new ArgumentException("找不到默认的 消息转换组件");

      // 定期清理 callback 池
      InitRequestedMessageCallBacksCleaner();

      _hubUri = hubOptions.Uri;
      var url = _hubUri.AbsoluteUri.Replace("http://", "ws://").Replace("https://", "wss://");
      if (hubOptions.Headers != null) {
        _webSocket = new WebSocket(url, customHeaderItems: hubOptions.Headers.ToList());
      } else {
        _webSocket = new WebSocket(url);
      }

    }

    ~HubConnection() {
      Dispose(false);
    }
    public void InitRequestedMessageCallBacksCleaner() {
      _sendedMessageCallBacksCleanerTimer = new Timer(async (state) => {
        // 清理时 停止 消息发送 或等待正在发送的消息发送完成
        await WaitConnectionLockAsync();

        try {
          _logger.LogInformation("InvocationRequestCallBack 定期清理启动");

          var shouldCleanCallBackInvocationIds = new List<string>();
          foreach (var sendedMessageCallBack in _sendedMessageCallBacks) {
            if (sendedMessageCallBack.Value.ExpireTime <= DateTime.UtcNow) {
              shouldCleanCallBackInvocationIds.Add(sendedMessageCallBack.Key);
            }
          }
          shouldCleanCallBackInvocationIds.ForEach(m => {
            _sendedMessageCallBacks.TryRemove(m, out InvocationRequestCallBack<object> callback);
            _logger.LogError($"InvocationId:{m}的 请求回调从回调池里删除,请求未在{InvocationRequestCallBack<object>.CallBackTimeOutMinutes}分钟内响应");
          });
          _logger.LogInformation($"InvocationRequestCallBack 定期清理结束,清理对象:{shouldCleanCallBackInvocationIds.Count}个");
        } finally {
          ReleaseConnectionLock();
        }

      }, "", 5 * 1000 * 60, 5 * 1000 * 60);
      _logger.LogInformation("InvocationRequestCallBack 定期清理设置成功");
    }
    public async Task StartAsync(CancellationToken cancellationToken = default) {
      if (_isStart) {
        return;
      }
      await StartAsyncCore(cancellationToken);
    }
    private async Task StartAsyncCore(CancellationToken cancellationToken) {
      CheckDisposed();
      await WaitConnectionLockAsync();

      try {
        cancellationToken.ThrowIfCancellationRequested();

        _webSocket.OpenAsync().GetAwaiter().GetResult();
        _webSocket.MessageReceived += WebSocket_MessageReceived;
        _logger.LogInformation($"客户端开始监听服务器端返回,hub uri:{_hubUri.AbsoluteUri}");

        _sendedPingMessageTimer = new Timer(new TimerCallback(RunTimerActions), null, DefaultKeepAliveInterval.Seconds * 1000, (DefaultKeepAliveInterval.Seconds - 2) * 1000);

        _logger.LogInformation("定期长连接维护设置成功");
        try {
          _isStart = true;
          await HandshakeAsync(cancellationToken);
        } catch (Exception ex) {
          await StopAsync();
          throw ex;
        }
      } finally {
        ReleaseConnectionLock();
      }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default) {
      _logger.LogInformation("开始销毁 Hub连接资源");
      CheckDisposed();
      await StopAsyncCore();
    }
    public async Task CloseAsync(string message, Exception exception = null, CancellationToken cancellationToken = default) {
      CheckDisposed();
      _logger.LogInformation("开始关闭Hub 连接");
      if (Closed.GetInvocationList().Length != 0) {
        if (exception != null) {
          await Closed.Invoke(new Exception(message, exception));
        } else {
          await Closed.Invoke(new Exception(message));
        }
      } else {

      }
    }
    public async Task RestartAsync(CancellationToken cancellationToken = default) {
      CheckDisposed();
      _logger.LogInformation("开始重启Hub 连接");
      cancellationToken.ThrowIfCancellationRequested();


      try {
        _webSocket.OpenAsync().GetAwaiter().GetResult();
        await HandshakeAsync(cancellationToken);
      } catch (Exception ex) {
        await CloseAsync($"重启连接失败,message:{ex.Message}", ex);
      }

    }

    public IDisposable On<TResult>(string methodName, Action<TResult> handler) where TResult : class {
      CheckDisposed();

      var invocationHandler = new InvocationHandler(obj => {
        var result = obj as TResult;
        handler(result);
      }, typeof(TResult));
      var invocationList = _handlers.AddOrUpdate(methodName, _ => new InvocationHandlerList(invocationHandler),
          (_, invocations) => {
            lock (invocations) {
              invocations.Add(invocationHandler);
            }
            return invocations;
          });

      return new Subscription(invocationHandler, invocationList);
    }
    /// <summary>
    /// remove on 
    /// </summary>
    /// <param name="methodName"></param>
    public void Remove(string methodName) {
      CheckDisposed();
      _handlers.TryRemove(methodName, out _);
    }

    internal async Task SendSuccessfulCompletionAsync(string invocationId, CancellationToken cancellationToken = default) {
      var invocationMessage = new SuccessfulCompletion(invocationId);

      _logger.LogInformation("开始发送成功完成消息");
      await InnerSendHubMessage(invocationMessage, cancellationToken);
    }
    internal async Task SendFaildCompletionAsync(string invocationId, string errorMessage, CancellationToken cancellationToken = default) {
      var invocationMessage = new FaildCompletion(invocationId, errorMessage);

      _logger.LogInformation("开始 发送失败完成消息");
      await InnerSendHubMessage(invocationMessage, cancellationToken);
    }

    private void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e) {
      var data = e.Message;
      _logger.LogInformation($"收到服务端消息:{data}");
      if (data.Length < 1) {
        return; //非 signalR 协议中的返回
      }
      data = data.Substring(0, data.Length - 1);
      if (data == "{}") // 消息格式成功设置返回
      {
        return;
      }
      var separator = new byte[1] { 0x1e };
      var separatorStr = Encoding.UTF8.GetString(separator);
      var messages = data.Split(separatorStr[0]);
      foreach (var message in messages) {
        if (!string.IsNullOrEmpty(message)) {
          try {
            var messageHandler = _receivedMessageHandlerProvider.GetHandler(message);
            messageHandler.Handler(message, _sendedMessageCallBacks, _handlers, this);
          } catch {
            continue;
          }
        }
      }
    }

    private async Task StopAsyncCore() {
      await Task.CompletedTask;
      Dispose(true);
    }
    public async Task Invoke<TResult>(string methodName, object[] args, Action<TResult, Exception> callBack, CancellationToken cancellationToken = default) {
      CheckDisposed();
      await WaitConnectionLockAsync();
      try {
        await InvokeCore(methodName, callBack, args, cancellationToken);
      } finally {
        ReleaseConnectionLock();
      }
    }
    public async Task Invoke(string methodName, object[] args, CancellationToken cancellationToken = default) {
      CheckDisposed();
      await WaitConnectionLockAsync();
      try {
        Action<object, Exception> callBack = null;

        await InvokeCore(methodName, callBack, args, cancellationToken);
      } finally {
        ReleaseConnectionLock();
      }
    }

    private async Task InvokeCore<TResult>(string methodName, Action<TResult, Exception> callBack, object[] args, CancellationToken cancellationToken = default) {
      var currentInvocationId = GetCurrentInvocationId;
      if (_sendedMessageCallBacks.TryGetValue(currentInvocationId, out InvocationRequestCallBack<object> invocationRequestCallBack)) {
        throw new Exception($"HubUrl:{_hubUri.AbsoluteUri},currentInvocationId:{currentInvocationId} 在 InvocationRequestCallBack Dic中 已存在");
      }
      Action<object, Exception> invoke = (obj, ex) => {
        var result = (TResult)obj;
        if (result == null) {
          ex = new Exception("方法Invoke 中的返回值 与服务器实际返回的值类型不匹配");
        }
        callBack(result, ex);
      };
      if (callBack == null) {
        invoke = null;
      }
      _sendedMessageCallBacks.TryAdd(currentInvocationId, new InvocationRequestCallBack<object>(DateTime.UtcNow.AddMinutes(InvocationRequestCallBack<object>.CallBackTimeOutMinutes), invoke, typeof(TResult)));
      await InnerInvokeCore(methodName, currentInvocationId, args, cancellationToken);
    }

    private async Task InnerInvokeCore(string methodName, string invocationId, object[] args, CancellationToken cancellationToken = default) {
      Message invocationMessage = null;
      if (string.IsNullOrEmpty(invocationId)) {
        invocationMessage = new NonBlockingInvocation(methodName, args);
      } else {
        invocationMessage = new BasicInvocation(invocationId, methodName, args);
      }
      _logger.LogInformation("开始 发送远程调用消息");
      await InnerSendHubMessage(invocationMessage, cancellationToken);
    }

    private async Task InnerSendHubMessage(Message hubMessage, CancellationToken cancellationToken = default) {
      await Task.CompletedTask;

      CheckDisposed();
      CheckStarted();
      CheckConnectionActive();

      var data = _messageConverter.GetBytes(hubMessage);
      _logger.LogInformation(" 发送数据");
      try {
        _webSocket.Send(data, 0, data.Length);
        ResetSendPing();
        ResetTimeout();
      } catch (Exception ex) {
        _logger.LogError(ex, "发送数据失败");
        throw ex;
      }
    }

    public async Task Send(string methodName, object[] args, CancellationToken cancellationToken = default) {
      CheckDisposed();
      await WaitConnectionLockAsync();
      try {
        CheckDisposed();

        await InnerInvokeCore(methodName, null, args, cancellationToken);
      } finally {
        ReleaseConnectionLock();
      }
    }

    private void CheckDisposed() {
      if (_disposed) {
        throw new ObjectDisposedException(nameof(HubConnection));
      }
    }
    private void CheckStarted() {
      if (!_isStart) {
        throw new InvalidOperationException("请先开始连接");
      }
    }

    private async Task HandshakeAsync(CancellationToken cancellationToken) {
      await Task.CompletedTask;
      var handshakeRequest = new Handshake(_messageConverter.ProtocolName);

      try {
        _logger.LogInformation($"开始发送 协议格式,{nameof(_messageConverter.ProtocolName)}:{_messageConverter.ProtocolName}");
        await InnerSendHubMessage(handshakeRequest);
      } catch (Exception ex) {
        _logger.LogError(ex, $"发送 协议格式 失败");
        throw ex;
      }
    }

    private void ResetSendPing() {
      Volatile.Write(ref _nextActivationSendPing, (DateTime.UtcNow + KeepAliveInterval).Ticks);
    }

    private void ResetTimeout() {
      Volatile.Write(ref _nextActivationServerTimeout, (DateTime.UtcNow + ServerTimeout).Ticks);
    }

    private void RunTimerActions(object obj) {
      _logger.LogInformation("长连接维护开始");
      if (Volatile.Read(ref _nextActivationServerTimeout) != 0 && DateTime.UtcNow.Ticks > Volatile.Read(ref _nextActivationServerTimeout)) {
        OnServerTimeout();
      }

      if (DateTime.UtcNow.Ticks > Volatile.Read(ref _nextActivationSendPing)) {
        PingServer().GetAwaiter().GetResult();
      }

    }

    private void OnServerTimeout() {
      _logger.LogError("服务器响应超时");

      CloseAsync("服务器响应超时").GetAwaiter().GetResult();
    }

    private bool IsConnectionAvailable() {
      return _webSocket.State == WebSocketState.Connecting || _webSocket.State == WebSocketState.Open;
    }
    private async Task PingServer() {
      await WaitConnectionLockAsync();
      try {
        if (_disposed || _webSocket == null || !IsConnectionAvailable()) {
          await CloseAsync("连接已关闭或者连接已被释放");
        }
        _logger.LogInformation("开始发送Ping 给服务器");

        await InnerSendHubMessage(Ping.Instance);

        ResetSendPing();
        ResetTimeout();

      } catch (Exception ex) {
        _logger.LogError(ex, "发送 ping 消息 给服务器端 失败");
      } finally {
        ReleaseConnectionLock();
      }
    }

    private void CheckConnectionActive() {
      if (_webSocket == null || !IsConnectionAvailable()) {
        throw new Exception($"当前连接失败或者连接已经被已经关闭");
      }
    }

    private Task WaitConnectionLockAsync(string memberName = null, string filePath = null, int lineNumber = 0) {
      return _connectionLock.WaitAsync();
    }

    private void ReleaseConnectionLock(string memberName = null,
         string filePath = null, int lineNumber = 0) {
      _connectionLock.Release();
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing) {
      if (_disposed) {
        return;
      }
      WaitConnectionLockAsync().GetAwaiter().GetResult();
      try {
        if (disposing) {
          _sendedMessageCallBacksCleanerTimer.Dispose();

          if (_sendedPingMessageTimer != null) {
            _sendedPingMessageTimer.Dispose();
          }
          //(_serviceProvider as IDisposable)?.Dispose();
        }
        _webSocket.Dispose();

        _disposed = true;
      } finally {
        ReleaseConnectionLock();
      }
    }
  }
}
