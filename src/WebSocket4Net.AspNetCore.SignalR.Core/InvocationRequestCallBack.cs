using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Connection
{
    public class InvocationRequestCallBack<TResult>
    {
        internal static int CallBackTimeOutMinutes = 1;
        public InvocationRequestCallBack(DateTime expireTime, Action<TResult, Exception> callBack, Type returnType)
        {
            this.ExpireTime = expireTime;
            this.Invoke = callBack;
            this.ReturnType = returnType;
        }
        /// <summary>
        /// UTC Time  InvocationRequestCallBack 的过期时间，过期时间一到 此 InvocationRequestCallBack 将会标记为可清除 
        /// </summary>
        public DateTime ExpireTime { get; private set; }
        public Type ReturnType { get; private set; }
        public Action<TResult, Exception> Invoke { get; private set; }

    }
}
