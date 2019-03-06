# WebSocket4Net.AspNetCore.SignalR.Client
Win7 下  Aspnet Core SignalR 基于 WebSocket4Net的客户端


``` c#
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


            connection.Invoke<UserAndMessage>("SendMessage", new object[] { "user1", "message1" }, (result, exception) =>
            {
                Console.WriteLine($"result:{result}");



            }).GetAwaiter().GetResult();


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
```
