using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol;

namespace WebSocket4Net.AspNetCore.SignalRClient.Connection
{
    public class HubConnectionBuilder
    {
        private bool _hubConnectionBuilt;

        public IServiceCollection Services { get; }

        public HubConnectionBuilder()
        {
            Services = new ServiceCollection();
            Services.AddSingleton<HubConnection>();
            Services.AddLogging();
        }

        public HubConnection Build()
        {
            if (_hubConnectionBuilt)
            {
                throw new InvalidOperationException("同一个 HubConnectionBuilder 只能有一个连接");
            }

            _hubConnectionBuilt = true;

            var serviceProvider = Services.BuildServiceProvider();

            return serviceProvider.GetService<HubConnection>();
        }
    }
}
