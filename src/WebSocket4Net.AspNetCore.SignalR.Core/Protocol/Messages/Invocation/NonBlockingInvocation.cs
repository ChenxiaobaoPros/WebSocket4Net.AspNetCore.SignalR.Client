using Newtonsoft.Json;


namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class NonBlockingInvocation : Message
    {
        public NonBlockingInvocation(string targrt, object[] arguments)
        {
            this.Type = 1;
            this.Arguments = arguments;
            this.Target = targrt;
        }
        public string Target { get; set; }
        public object[] Arguments { get; set; }
    }
}
