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

            connection.Closed += async (ex) =>
            {
                Console.WriteLine(ex.Message);
                //重试几次
                await connection.RestartAsync();
            };

            Timer timer = new Timer(obj =>
            {
                connection.Invoke<UserAndMessage>("SendMessage", new object[] { "user1", "message1" }, (result, exception) =>
                {
                    Console.WriteLine($"result:{result}");



                }).GetAwaiter().GetResult();
            }, "", 0, 5 * 60 * 1000);

            Console.ReadKey();

        public class UserAndMessage
        {
            public string User { get; set; }
            public string Message { get; set; }
        }
```
