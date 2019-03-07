using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebSocket4Net.AspNetCore.SignalR.Client;
using WebSocket4Net.AspNetCore.SignalRClient.Connection;

namespace ClientApp {
  public class Program {
    static void Main(string[] args) {
      var header = new Dictionary<string, string>();
      header.Add("Sample", "Token");

      var connection = new HubConnectionBuilder()
          .WithUrl("http://127.0.0.1:5000/chatHub", option => {
            option.Headers = header;

          })
          .ConfigService(service => {
            service.AddLogging(config => {
              config.AddConsole();
            });
          })
          .Build();
      connection.StartAsync().GetAwaiter().GetResult();

      connection.On<UserAndMessage>("ReceiveMessage", model => {
        Console.WriteLine($"user:{model.User},mes:{model.Message}");
      });

      connection.Closed += async (ex) => {
        Console.WriteLine(ex.Message);
        //重试几次
        await connection.RestartAsync();
      };
      connection.Send("SendMessage", new object[] { "user1", "message1" }).GetAwaiter().GetResult();
      Timer timer = new Timer(obj => {
        connection.Invoke<UserAndMessage>("SendMessage", new object[] { "user1", "message1" }, (result, exception) => {
          Console.WriteLine($"result:{result}");



        }).GetAwaiter().GetResult();
      }, "", 0, 5 * 60 * 1000);

      Console.ReadKey();
    }

    public class UserAndMessage {
      public string User { get; set; }
      public string Message { get; set; }
    }
  }
}
