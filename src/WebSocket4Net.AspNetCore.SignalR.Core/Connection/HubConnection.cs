using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net.AspNetCore.SignalR.Core;
using WebSocket4Net.AspNetCore.SignalR.Core.Abstriction;
using WebSocket4Net.AspNetCore.SignalR.Core.Connection;
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol.Messages.Invocation;

namespace WebSocket4Net.AspNetCore.SignalRClient.Connection
{
    public class HubConnection
    {
        public static readonly TimeSpan DefaultServerTimeout = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan DefaultKeepAliveInterval = TimeSpan.FromSeconds(15);

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private ConcurrentDictionary<string, InvocationHandlerList> _handlers = new ConcurrentDictionary<string, InvocationHandlerList>(StringComparer.Ordinal);
        private readonly IMessageParser _messageConverter;
        private readonly Uri _hubUri;
        private long _currentInvocationId = 1;
        private ConcurrentDictionary<string, InvocationRequestCallBack<object>> _sendedMessageCallBacks = new ConcurrentDictionary<string, InvocationRequestCallBack<object>>();
        private readonly IReceivedMessageHandlerProvider _receivedMessageHandlerProvider;
        private readonly Timer _sendedMessageCallBacksCleanerTimer;
        private Timer _sendedPingMessageTimer;

        private long _nextActivationServerTimeout;
        private long _nextActivationSendPing;
        private bool _disposed;
        private bool _isStart;

        private WebSocket4Net.WebSocket _webSocket;


        public event Func<Exception, Task> Closed;

        string GetCurrentInvocationId
        {
            get
            {
                var currentInvocationId = _currentInvocationId;
                _currentInvocationId++;
                return currentInvocationId.ToString();
            }
        }

        public TimeSpan ServerTimeout { get; set; } = DefaultServerTimeout;

        public TimeSpan KeepAliveInterval { get; set; } = DefaultKeepAliveInterval;

        public HubConnection(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
            : this(loggerFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._messageConverter = serviceProvider.GetService(typeof(IMessageParser)) as IMessageParser ?? throw new ArgumentException("找不到默认的 消息转换组件");
            var hubOptions = serviceProvider.GetService(typeof(HubConnectionOptions)) as HubConnectionOptions ?? throw new ArgumentException("找不到默认的 Hub 配置");

            _receivedMessageHandlerProvider = serviceProvider.GetService(typeof(IReceivedMessageHandlerProvider)) as IReceivedMessageHandlerProvider;


            // 定期清理 callback 池
            _sendedMessageCallBacksCleanerTimer = new Timer(async (state) =>
            {
                await WaitConnectionLockAsync();

                try
                {
                    var shouldCleanCallBackInvocationIds = new List<string>();
                    foreach (var sendedMessageCallBack in _sendedMessageCallBacks)
                    {
                        if (sendedMessageCallBack.Value.ExpireTime <= DateTime.UtcNow)
                        {
                            shouldCleanCallBackInvocationIds.Add(sendedMessageCallBack.Key);
                        }
                    }
                    shouldCleanCallBackInvocationIds.ForEach(m =>
                    {
                        _sendedMessageCallBacks.TryRemove(m, out InvocationRequestCallBack<object> callback);
                    });
                }
                finally
                {
                    ReleaseConnectionLock();
                }
                
            }, "", 5 * 1000 * 60, 5 * 1000 * 60);

            _hubUri = hubOptions.Uri;
            var url = _hubUri.AbsoluteUri.Replace("http://", "ws://").Replace("https://", "wss://");

            this._webSocket = new WebSocket(url);

        }


        public HubConnection(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HubConnection>();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            await StartAsyncCore(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            await StopAsyncCore();

        }

        public IDisposable On<TResult>(string methodName, Action<TResult> handler) where TResult : class
        {
            CheckDisposed();

            var invocationHandler = new InvocationHandler(obj =>
           {
               var result = obj as TResult;
               handler(result);
           }, typeof(TResult));
            var invocationList = _handlers.AddOrUpdate(methodName, _ => new InvocationHandlerList(invocationHandler),
                (_, invocations) =>
                {
                    lock (invocations)
                    {
                        invocations.Add(invocationHandler);
                    }
                    return invocations;
                });

            return new Subscription(invocationHandler, invocationList);
        }

        public void Remove(string methodName)
        {
            CheckDisposed();
            _handlers.TryRemove(methodName, out _);
        }

        public async Task SendSuccessfulCompletionAsync(string invocationId, CancellationToken cancellationToken = default)
        {
            var invocationMessage = new SuccessfulCompletion(invocationId);

            _logger.LogInformation("发送调用成功消息");
            await InnerSendHubMessage(invocationMessage, cancellationToken);
        }
        public async Task SendFaildCompletionAsync(string invocationId, string errorMessage, CancellationToken cancellationToken = default)
        {
            var invocationMessage = new FaildCompletion(invocationId, errorMessage);

            _logger.LogInformation("发送调用失败消息");
            await InnerSendHubMessage(invocationMessage, cancellationToken);
        }

        private async Task StartAsyncCore(CancellationToken cancellationToken)
        {

            await WaitConnectionLockAsync();

            _webSocket.OpenAsync().GetAwaiter().GetResult();
            _webSocket.MessageReceived += WebSocket_MessageReceived;
            _logger.LogInformation($"客户端开始监听服务器端返回,hub uri:{_hubUri.AbsoluteUri}");

            // 开始 ping 服务器
            _sendedPingMessageTimer = new Timer(new TimerCallback(RunTimerActions), null, 0, 15000);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                CheckDisposed();
                try
                {
                    await HandshakeAsync(cancellationToken);
                    _isStart = true;
                }
                catch (Exception ex)
                {
                    await StopAsync();
                    throw ex;
                }
            }
            finally
            {
                ReleaseConnectionLock();
            }
        }

        private void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var data = e.Message;
            if (data.Length < 1)
            {
                return; //非 signalR 协议中的返回
            }
            data = data.Substring(0, data.Length - 1);
            if (data == "{}") // 消息格式成功设置返回
            {
                return;
            }
            var messageHandler = _receivedMessageHandlerProvider.GetHandler(data);
            messageHandler.Handler(data, _sendedMessageCallBacks, _handlers, this);
        }

        private async Task StopAsyncCore()
        {
            await WaitConnectionLockAsync();

            if (_disposed)
            {
                return;
            }
            CheckDisposed();

            (_serviceProvider as IDisposable)?.Dispose();
            _disposed = true;
            _webSocket.MessageReceived -= WebSocket_MessageReceived;
            _sendedMessageCallBacks = null;
            _handlers = null;
            ReleaseConnectionLock();
            _connectionLock.Dispose();
            _webSocket.Dispose();
            _sendedMessageCallBacksCleanerTimer.Dispose();
            _webSocket = null;
            _sendedPingMessageTimer.Dispose();

        }
        public async Task Invoke<TResult>(string methodName, object[] args, Action<TResult, Exception> callBack, CancellationToken cancellationToken = default) where TResult : class
        {
            await InvokeCore(methodName, callBack, args, cancellationToken);
        }
        public async Task Invoke(string methodName, object[] args, CancellationToken cancellationToken)
        {
            CheckDisposed();
            await WaitConnectionLockAsync();
            Action<object, Exception> callBack = (obj, ex) => { };

            await InvokeCore(methodName, callBack, args, cancellationToken);
        }

        private async Task InvokeCore<TResult>(string methodName, Action<TResult, Exception> callBack, object[] args, CancellationToken cancellationToken = default) where TResult : class
        {
            try
            {
                CheckDisposed();

                if (_sendedMessageCallBacks.TryGetValue(methodName, out InvocationRequestCallBack<object> invocationRequestCallBack))
                {
                    throw new Exception($"HubUrl:{_hubUri.AbsoluteUri},方法:{methodName}");
                }
                var currentInvocationId = GetCurrentInvocationId;
                _sendedMessageCallBacks.TryAdd(currentInvocationId, new InvocationRequestCallBack<object>(currentInvocationId, DateTime.UtcNow.AddMinutes(1), (obj, ex) =>
                {
                    var result = obj as TResult;
                    callBack(result, ex);
                }
                ));
                await InnerInvokeCore(methodName, GetCurrentInvocationId, args, cancellationToken);
            }
            finally
            {
                ReleaseConnectionLock();
            }
        }

        private async Task InnerInvokeCore(string methodName, string invocationId, object[] args, CancellationToken cancellationToken = default)
        {
            Message invocationMessage = null;
            if (string.IsNullOrEmpty(invocationId))
            {
                invocationMessage = new NonBlockingInvocation(methodName, args);
            }
            else
            {
                invocationMessage = new BasicInvocation(invocationId, methodName, args);
            }
            _logger.LogInformation("开始 发送远程调用消息");
            await InnerSendHubMessage(invocationMessage, cancellationToken);
        }



        private async Task InnerSendHubMessage(Message hubMessage, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            CheckStarted();
            CheckConnectionActive();

            var data = _messageConverter.GetBytes(hubMessage);
            _logger.LogInformation(" 发送数据");
            try
            {
                _webSocket.Send(data, 0, data.Length);
                ResetSendPing();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送数据失败");
                throw ex;
            }
        }

        public async Task Send(string methodName, object[] args, CancellationToken cancellationToken = default)
        {

            CheckDisposed();
            await WaitConnectionLockAsync();

            try
            {
                CheckDisposed();

                await InnerInvokeCore(methodName, null, args, cancellationToken);
            }
            finally
            {
                ReleaseConnectionLock();
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HubConnection));
            }
        }
        private void CheckStarted()
        {
            if (!_isStart)
            {
                throw new InvalidOperationException("请先开始连接");
            }
        }

        private async Task HandshakeAsync(CancellationToken cancellationToken)
        {
            CheckConnectionActive();

            await Task.CompletedTask;
            var handshakeRequest = new Handshake(_messageConverter.ProtocolName);

            var data = _messageConverter.GetBytes(handshakeRequest);
            try
            {
                _logger.LogInformation($"开始发送 协议格式,{nameof(_messageConverter.ProtocolName)}:{_messageConverter.ProtocolName}");
                _webSocket.Send(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送 协议格式 失败");
            }
        }

        public void ResetSendPing()
        {
            Volatile.Write(ref _nextActivationSendPing, (DateTime.UtcNow + KeepAliveInterval).Ticks);
        }

        public void ResetTimeout()
        {
            Volatile.Write(ref _nextActivationServerTimeout, (DateTime.UtcNow + ServerTimeout).Ticks);
        }

        private void RunTimerActions(object obj)
        {
            if (Volatile.Read(ref _nextActivationServerTimeout) != 0 && DateTime.UtcNow.Ticks > Volatile.Read(ref _nextActivationServerTimeout))
            {
                OnServerTimeout();
            }

            if (DateTime.UtcNow.Ticks > Volatile.Read(ref _nextActivationSendPing))
            {
                PingServer().GetAwaiter().GetResult();
            }

        }

        private void OnServerTimeout()
        {
            _logger.LogError("服务器响应超时");
        }

        private bool IsConnectionAvailable()
        {
            return _webSocket.State == WebSocketState.Connecting || _webSocket.State == WebSocketState.Open;
        }
        private async Task PingServer()
        {
            await WaitConnectionLockAsync();
            if (_disposed  || _webSocket == null || !IsConnectionAvailable())
            {
                return;
            }
            try
            {
                await InnerSendHubMessage(Ping.Instance);
                ResetSendPing();
                ResetTimeout();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送 ping 消息 给服务器端 失败");
            }

            finally
            {
                ReleaseConnectionLock();
            }
        }

        private async Task RunClosedEvent(Func<Exception, Task> closed, Exception closeException)
        {
            // Dispatch to the thread pool before we invoke the user callback
            //await AwaitableThreadPool.Yield();

            try
            {
                // Log.InvokingClosedEventHandler(_logger);
                await closed.Invoke(closeException);
            }
            catch (Exception ex)
            {
                //Log.ErrorDuringClosedEvent(_logger, ex);
            }
        }

        private void CheckConnectionActive()
        {
            if (_webSocket == null || !IsConnectionAvailable())
            {
                throw new Exception($"当前连接失败或者连接已经被已经关闭");
            }
        }

        private Task WaitConnectionLockAsync(string memberName = null, string filePath = null, int lineNumber = 0)
        {
            return _connectionLock.WaitAsync();
        }

        private void ReleaseConnectionLock(string memberName = null,
             string filePath = null, int lineNumber = 0)
        {
            //Log.ReleasingConnectionLock(_logger, memberName, filePath, lineNumber);
            _connectionLock.Release();
        }

        private class Subscription : IDisposable
        {
            private readonly InvocationHandler _handler;
            private readonly InvocationHandlerList _handlerList;

            public Subscription(InvocationHandler handler, InvocationHandlerList handlerList)
            {
                _handler = handler;
                _handlerList = handlerList;
            }

            public void Dispose()
            {
                _handlerList.Remove(_handler);
            }
        }

    }
}
