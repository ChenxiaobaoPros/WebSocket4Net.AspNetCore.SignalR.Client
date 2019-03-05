using System;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;
using WebSocket4Net.AspNetCore.SignalR.Client;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClientApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://127.0.0.1:5000/chatHub")
                .ConfigService(service =>
                {
                    service.AddLogging(config =>
                    {
                        config.AddConsole();
                    });
                })
                .Build();
            connection.StartAsync().GetAwaiter().GetResult();

            connection.On<UserAndMessage>("ReceiveMessage", model =>
            {
                Console.WriteLine($"user:{model.User},mes:{model.Message}");
            });

            connection.Closed += Connection_Closed;

            Timer timer = new Timer(obj =>
            {
                connection.Invoke<bool>("SendMessage", new object[] { "user1", "message1" }, (result, exception) =>
                {
                    Console.WriteLine($"result:{result}");



                }).GetAwaiter().GetResult();
            }, "", 0, 5 * 60 * 1000);

            Console.ReadKey();
        }

        private static async System.Threading.Tasks.Task Connection_Closed(Exception arg)
        {
            await Task.CompletedTask;
            Console.WriteLine(arg.Message);
        }

        public class UserAndMessage
        {
            public string User { get; set; }
            public string Message { get; set; }
        }
    }
}
