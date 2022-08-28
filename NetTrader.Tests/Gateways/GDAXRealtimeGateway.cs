/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core;
using GDAX.NET;

namespace NetTrader.Gateways
{
    public class GDAXRealtimeGateway
    {
        private string currencyPair;

        private RealtimeClient realtimeClient;

        private Action<RealtimeMatch> onMatchReceived;

        public GDAXRealtimeGateway(string currencyPair, Action<RealtimeMatch> onMatchReceived)
        {
            this.currencyPair = currencyPair;
            this.onMatchReceived = onMatchReceived;

            this.realtimeClient = new RealtimeClient("wss://ws-feed.gdax.com", new string[] { currencyPair }, Settings.Instance.GDAX.RequestAuthenticator);
        }

        //

        public void Start()
        {
            Task.Run(WorkerMethod);
        }

        private async Task WorkerMethod()
        {
            await realtimeClient.SubscribeAsync(MessageReceived, OnWebsocketDiedAndReconnected);
        }

        private void MessageReceived(RealtimeMessage message)
        {
            var match = message as RealtimeMatch;

            if (match != null)
            {
                onMatchReceived?.Invoke(match);
            }
        }

        private void OnWebsocketDiedAndReconnected()
        {
            Console.WriteLine("GDAX websocket died and reconnected...");
        }
    }
}
