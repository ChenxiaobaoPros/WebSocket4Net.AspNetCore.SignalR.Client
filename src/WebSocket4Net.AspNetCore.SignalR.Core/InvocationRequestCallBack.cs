using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Connection
{
    public class InvocationRequestCallBack<TResult>
    {
        public InvocationRequestCallBack( DateTime expireTime, Action<TResult, Exception> callBack)
        {
            this.ExpireTime = expireTime;
            this.Invoke = callBack;
        }
        /// <summary>
        /// UTC Time
        /// </summary>
        public DateTime ExpireTime { get;private set; }
        public Action<TResult, Exception> Invoke { get;private set; }
  
    }
}
