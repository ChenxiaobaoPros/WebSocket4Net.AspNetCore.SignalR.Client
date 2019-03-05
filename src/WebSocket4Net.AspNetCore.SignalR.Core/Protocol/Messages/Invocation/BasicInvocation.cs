

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class BasicInvocation : Message
    {
        public BasicInvocation()
        {

        }
        public BasicInvocation(string invocationId, string target, object[] arguments)
        {
            this.Type = 1;
            this.InvocationId = invocationId;
            this.Arguments = arguments;
            this.Target = target;
        }
        public string InvocationId { get; set; }
        public string Target { get; set; }
        public object[] Arguments { get; set; }
    }
}
