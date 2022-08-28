using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace GDAX.NET
{
    public class RealtimeClient
    {
        private string _websocketFeedUri;

		private WebSocket webSocket;

		private string[] productIds;

		private RequestAuthenticator requestAuthenticator;

		private Action<RealtimeMessage> messageReceivedCallback;

		private Action onReconnect;

		private bool running = true;
		private bool stopped = false;

		private long lastMessageTimestamp = -1;
        
		public RealtimeClient(string websocketFeedUri, string[] productIds, RequestAuthenticator requestAuthenticator)
        {
            _websocketFeedUri = websocketFeedUri;
            this.productIds = productIds;
			this.requestAuthenticator = requestAuthenticator;
        }

		public async Task SubscribeAsync(Action<RealtimeMessage> messageReceived, Action onReconnect)
        {
            if (messageReceived == null)
				throw new ArgumentNullException(nameof(messageReceived));
			
			this.messageReceivedCallback = messageReceived;

			if(onReconnect != null)
				this.onReconnect = onReconnect;

			await ConnectToSocket();
        }

		private async Task ConnectToSocket()
		{
			running = true;

			using (webSocket = new WebSocket(_websocketFeedUri))
			{
				var token = requestAuthenticator.GetAuthenticationToken("GET", "/users/self", "");

				webSocket.OnOpen += (sender, e) =>
				{
					webSocket.Send(JsonConvert.SerializeObject(new
					{
						type = "subscribe",
						product_ids = productIds,
						signature = token.Signature,
						key = token.Key,
						passphrase = token.Passphrase,
						timestamp = token.Timestamp
					}));
				};

				webSocket.OnMessage += (sender, e) =>
				{
					lastMessageTimestamp = DateTimeUtilities.Timestamp;

					this.messageReceivedCallback(ParseMessage(e.Data));
				};

				webSocket.OnError += (sender, e) =>
				{
					Console.WriteLine("Socket error: " + e.Message);
					running = false;
				};

				webSocket.OnClose += (sender, e) =>
				{
					Console.WriteLine("Socket closed: " + e.Reason);
					running = false;
				};

				webSocket.Log.Level = LogLevel.Warn;

				webSocket.ConnectAsync();

				while (running)
				{
					if (lastMessageTimestamp != -1 && (DateTimeUtilities.Timestamp - lastMessageTimestamp) > 1 * 60)
					{
						running = false;
						lastMessageTimestamp = -1;
					}
					else
					{
						await Task.Delay(100);
					}
				}

				await Task.Delay(5000);

				if (!stopped)
				{
					try
					{
						Console.WriteLine("Websocket is dead, trying to reconnect.");

						try
						{
							if (webSocket != null)
								webSocket.Close();

							webSocket = null;
						}
						catch (Exception e)
						{
							Console.WriteLine(e.ToString());
						}

						await ConnectToSocket();
					}
					catch (Exception e)
					{
						Console.WriteLine(e.ToString());
					}
				}
			}
		}

        private RealtimeMessage ParseMessage(string jsonResponse)
        {
            var jToken = JToken.Parse(jsonResponse);
            var type = jToken["type"]?.Value<string>();

            RealtimeMessage realtimeMessage = null;

            switch (type)
            {
                case "received":
                    realtimeMessage = JsonConvert.DeserializeObject<RealtimeReceived>(jsonResponse);
                    break;
                case "open":
                    realtimeMessage = JsonConvert.DeserializeObject<RealtimeOpen>(jsonResponse);
                    break;
                case "done":
                    realtimeMessage = JsonConvert.DeserializeObject<RealtimeDone>(jsonResponse);
                    break;
                case "match":
                    realtimeMessage = JsonConvert.DeserializeObject<RealtimeMatch>(jsonResponse);
                    break;
                case "change":
                    realtimeMessage = JsonConvert.DeserializeObject<RealtimeChange>(jsonResponse);
                    break;
                case "error":
                    var error = JsonConvert.DeserializeObject<RealtimeError>(jsonResponse);
                    throw new Exception(error.message + "\n" + error.reason);
                default:
                    break;
            }

            return realtimeMessage;
        }

		public void Stop()
		{
			running = false;
			stopped = true;
		}
	}
}