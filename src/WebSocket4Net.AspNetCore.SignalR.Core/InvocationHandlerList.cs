using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Connection
{
    public class InvocationHandlerList
    {
        private readonly List<InvocationHandler> _invocationHandlers;
        private InvocationHandler[] _copiedHandlers;

        internal InvocationHandlerList(InvocationHandler handler)
        {
            _invocationHandlers = new List<InvocationHandler>() { handler };
        }

        internal InvocationHandler[] GetHandlers()
        {
            var handlers = _copiedHandlers;
            if (handlers == null)
            {
                lock (_invocationHandlers)
                {
                    if (_copiedHandlers == null)
                    {
                        _copiedHandlers = _invocationHandlers.ToArray();
                    }
                    handlers = _copiedHandlers;
                }
            }
            return handlers;
        }

        internal void Add(InvocationHandler handler)
        {
            lock (_invocationHandlers)
            {
                _invocationHandlers.Add(handler);
                _copiedHandlers = null;
            }
        }

        internal void Remove(InvocationHandler handler)
        {
            lock (_invocationHandlers)
            {
                if (_invocationHandlers.Remove(handler))
                {
                    _copiedHandlers = null;
                }
            }
        }
    }

    class InvocationHandler
    {
        private readonly Action<object> _callback;
        public Type ReturnType { get; private set; }

        public InvocationHandler(Action<object> callback, Type returnType)
        {
            _callback = callback;
            this.ReturnType = returnType;
        }

        public async Task InvokeAsync(object parameter)
        {
            await Task.CompletedTask;
            _callback(parameter);
        }
    }
}
