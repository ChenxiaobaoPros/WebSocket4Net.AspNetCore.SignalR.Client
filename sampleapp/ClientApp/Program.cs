using System;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;
using WebSocket4Net.AspNetCore.SignalR.Client;
using System.Threading;
using System.Threading.Tasks;

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

  
            //connection.Closed += Connection_Closed;

            //connection.Invoke<bool>("SendMessage", new object[] { "user1", "message1" }, (result, exception) =>
            //{
            //    Console.WriteLine($"result:{result}");



            //}).GetAwaiter().GetResult();

            Console.ReadKey();
        }

        //private static async System.Threading.Tasks.Task Connection_Closed(Exception arg)
        //{
        //    await Task.CompletedTask;
        //    Console.WriteLine(arg.Message);
        //}

        public class UserAndMessage
        {
            public string User { get; set; }
            public string Message { get; set; }
        }
    }
}
