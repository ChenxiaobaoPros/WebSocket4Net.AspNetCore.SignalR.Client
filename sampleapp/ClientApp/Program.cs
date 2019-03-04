using System;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;
using WebSocket4Net.AspNetCore.SignalR.Client;

namespace ClientApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://127.0.0.1:5000/chatHub")
                .Build();
            connection.StartAsync().GetAwaiter().GetResult();

            //// 监听请求
            //connection.On<UserAndMessage>("ReceiveMessage", model =>
            //   {
            //       Console.WriteLine($"user:{model.User},mes:{model.Message}");
            //   });

            //// 发送Non-block Invacation
            //connection.Send("SendMessage", new object[] { "user1", "message1" }).GetAwaiter().GetResult();
            Console.ReadKey();
        }
        public class UserAndMessage
        {
            public string User { get; set; }
            public string Message { get; set; }
        }
    }
}
