using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Connection
{
    public class InvocationRequestCallBack<TResult>
    {
        public InvocationRequestCallBack(string invocationId, DateTime expireTime, Action<TResult, Exception> callBack)
        {
            this.InvocationId = invocationId;
            this.ExpireTime = expireTime;
            this.CallBack = callBack;
        }
        public string InvocationId { get;private set; }
        /// <summary>
        /// UTC Time
        /// </summary>
        public DateTime ExpireTime { get;private set; }
        public Action<TResult, Exception> CallBack { get;private set; }
  
    }
}
