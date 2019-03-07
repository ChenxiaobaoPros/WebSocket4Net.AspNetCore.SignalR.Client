using System;
using System.Collections.Generic;
using System.Text;
using WebSocket4Net.AspNetCore.SignalRClient.Protocol;

namespace WebSocket4Net.AspNetCore.SignalRClient.Connection {
  public class HubConnectionOptions {
    public HubConnectionOptions(Uri uri) {
      this.Uri = uri;
    }
    public Uri Uri { get; private set; }
    public ProtocolOption ProtocolOption { get; set; } = ProtocolOption.Json;
    public IDictionary<string, string> Headers { get; set; }
  }
}
