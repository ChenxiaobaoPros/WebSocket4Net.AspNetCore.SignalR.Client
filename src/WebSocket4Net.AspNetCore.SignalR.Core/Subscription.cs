using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalR.Core.Connection;

namespace WebSocket4Net.AspNetCore.SignalR.Core
{
    internal class Subscription : IDisposable
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
