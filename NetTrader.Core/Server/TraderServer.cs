/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Pipe.Messages;
using NetTrader.Core.Server.Messages;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace NetTrader.Core.Server
{
    public class TraderServer
    {
        private bool running;

        private Queue<object> commandQueue = new Queue<object>();

        private WebsocketCommunicationChannel communicationChannel;

        private TraderCore core;

        public TraderServer(TraderCore core)
        {
            this.core = core;
        }

        private async Task Run()
        {
            running = true;

            int serverPort = Settings.Instance.Generic.ServerPort;

            Console.WriteLine("Initializing Websocket server on port " + serverPort);

            bool isServerSecured = Settings.Instance.Generic.IsSSLEnabled;

            var wssv = new WebSocketServer(serverPort, isServerSecured);

            if (isServerSecured)
            {
                wssv.SslConfiguration.ServerCertificate = new X509Certificate2(Settings.Instance.Generic.CertificatePath, Settings.Instance.Generic.CertificatePassword);

                wssv.AuthenticationSchemes = AuthenticationSchemes.Basic;
                wssv.Realm = "Authentication required";
                wssv.UserCredentialsFinder = id =>
                {
                    var name = id.Name;

                    var login = Settings.Instance.Generic.UserLogin;

                    return name == login ? new NetworkCredential(name, Settings.Instance.Generic.UserPassword, Settings.Instance.Generic.ServerDomain) : null; 
                };
            }

            wssv.Log.Level = LogLevel.Error;
            wssv.WaitTime = TimeSpan.FromSeconds(2);
            wssv.ReuseAddress = true;

            communicationChannel = new WebsocketCommunicationChannel(this);

            wssv.AddWebSocketService<WebsocketCommunicationChannel>("/channel", () => communicationChannel);

            wssv.Start();

            if (wssv.IsListening)
            {
                Console.WriteLine("Server listening on port {0}", wssv.Port);

                foreach (var path in wssv.WebSocketServices.Paths)
                    Console.WriteLine(path);
            }
            else
            {
                Console.WriteLine("Failed to initialize server on port " + serverPort);
            }

            //

            while (running)
            {
                while (commandQueue.Count > 0)
                {
                    object cmd = commandQueue.Dequeue();

                    communicationChannel.Broadcast(cmd);
                }

                await Task.Delay(20);
            }

            wssv.Stop();
        }

        public void Start()
        {
            Task.Run(Run);
        }

        public void OnMessageReceived(MessageEventArgs e)
        {
            Console.WriteLine("Message received: " + e.Data);

            var message = JsonConvert.DeserializeObject<ServerMessage>(e.Data);

            if (message == null)
                return;

            switch (message.MessageType)
            {
                case ServerMessage.Type.GetTargetBasePrice:
                    core.TradingPipe.SendMessage(new GetTargetBasePrice());
                    break;

                case ServerMessage.Type.SetTargetBasePrice:
                    float parsedPrice = 0f;

                    if(ParseFloat(message.Payload, out parsedPrice, "TargetBasePrice"))
                        core.TradingPipe.SendMessage(new SetTargetBasePriceMessage(parsedPrice));
                    break;

                case ServerMessage.Type.SetTBPRecalculationOffsetMessage:
                    float recalculationOffset = 0f;

                    if (ParseFloat(message.Payload, out recalculationOffset, "TBPRecalculationOffset"))
                        core.TBPRecalculationOffset = recalculationOffset;
                    break;

                case ServerMessage.Type.SetTBPRecalculationPriceOffsetMessage:
                    float recalculationPriceOffset = 0f;

                    if (ParseFloat(message.Payload, out recalculationPriceOffset, "TBPRecalculationPriceOffset"))
                        core.TBPRecalculationPriceOffset = recalculationPriceOffset;
                    break;

                case ServerMessage.Type.SetStopLossThreshold:
                    float threshold = 0f;

                    if (ParseFloat(message.Payload, out threshold, "StopLossThreshold"))
                        core.StopLossThreshold = threshold; 
                    break;

                case ServerMessage.Type.SetStopLossBasePriceOffset:
                    float offset = 0f;

                    if (ParseFloat(message.Payload, out offset, "StopLossBasePriceOffset"))
                        core.StopLossBasePriceOffset = offset; 
                    break;

                case ServerMessage.Type.GetOrders:
                    core.ExchangePipe.SendMessage(new GetOpenOrdersMessage());
                    break;

                case ServerMessage.Type.MarketOrder:
                    if (message.Payload != null)
                        core.ExchangePipe.SendMessage(new MarketOrderMessage(message.Payload?.ToString()));
                    break;

                case ServerMessage.Type.CancelOrder:
                    if (message.Payload != null)
                        core.ExchangePipe.SendMessage(new CancelOrderMessage(message.Payload?.ToString()));
                    break;

                case ServerMessage.Type.RemoveOrder:
                    if (message.Payload != null)
                        core.ExchangePipe.SendMessage(new RemoveOrderMessage(message.Payload?.ToString()));
                    break;

                case ServerMessage.Type.RestartTrading:
                    core.TradingPipe.SendMessage(new RestartTradingMessage());
                    break;

                case ServerMessage.Type.CancelAllOrders:
                    core.ExchangePipe.SendMessage(new CancelOpenedOrdersMessage());
                    break;

                case ServerMessage.Type.GetProfitsMessage:
                    core.TradingPipe.SendMessage(new GetProfitsMessage());
                    break;

                case ServerMessage.Type.GetAccountBalanceMessage:
                    core.ExchangePipe.SendMessage(new GetAccountBalanceMessage());
                    break;

                case ServerMessage.Type.StopLossMessage:
                    core.ExchangePipe.SendMessage(new StopLossMessage());
                    break;
            }
        }

        public void Stop()
        {
            running = false;
        }

        //

        #region Command Queue

        public void SendMessage(ServerMessage message)
        {
            commandQueue.Enqueue(message);
        }

        #endregion

        #region Parsers

        private bool ParseFloat(object value, out float parsedValue, string variable)
        {
            parsedValue = 0f;

            if (value == null || !float.TryParse(value.ToString(), out parsedValue))
            {
                core.Server.SendMessage(new ConsoleMessage("Failed to parse " + variable + ": " + value));
                return false;
            }

            return true;
        }

        #endregion
    }
}
