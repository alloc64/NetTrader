/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Database.Extensions;
using GDAX.NET;
using NetTrader.Core.Pipe.Messages;

namespace NetTrader.Core.Gateways.GDAX
{
    public class GDAXRealtimeGateway : IRealtimeGateway
    {
        private TraderCore core;

        private string currencyPair;

        private RealtimeClient realtimeClient;

        public GDAXRealtimeGateway(TraderCore core, string currencyPair)
        {
            this.core = core;

            this.currencyPair = currencyPair;

            this.realtimeClient = new RealtimeClient("wss://ws-feed.gdax.com", new string[] { currencyPair }, Settings.Instance.GDAX.RequestAuthenticator);
        }

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
                core.ExchangePipe.SendMessage(new MatchTradeMessage(GDAXExchangeMatch.FromRealtimeMatch(currencyPair, match)));
            }
            else if (message is RealtimeMessageWithOrderId)
            {
                var rmwo = message as RealtimeMessageWithOrderId;

                var order = core.Database.GetOrder(rmwo.order_id);

                if (order != null)
                {
                    switch (message.type)
                    {
                        /*
                        A valid order has been received and is now active. This message is emitted for every single valid order as soon as the matching 
                        engine receives it whether it fills immediately or not.

                        The received message does not indicate a resting order on the order book. It simply indicates a new incoming order which as been 
                        accepted by the matching engine for processing. Received orders may cause match message to follow if they are able to begin being 
                        filled (taker behavior). Self-trade prevention may also trigger change messages to follow if the order size needs to be adjusted. 
                        Orders which are not fully filled or canceled due to self-trade prevention result in an open message and become resting orders on the order book.

                        Market orders (indicated by the order_type field) may have an optional funds field which indicates how much quote currency will 
                        be used to buy or sell. For example, a funds field of 100.00 for the BTC-USD product would indicate a purchase of up to 100.00 USD worth of bitcoin.
                        */
                        case "received":

                            core.ExchangePipe.SendMessage(new OrderStateChangedMessage(order, rmwo.time, TradeOrder.State.Received));

                            break;

                        /*
                        The order is now open on the order book. This message will only be sent for orders which are not fully filled immediately. 
                        remaining_size will indicate how much of the order is unfilled and going on the book.
                        */
                        case "open":

                            core.ExchangePipe.SendMessage(new OrderStateChangedMessage(order, rmwo.time, TradeOrder.State.Open));

                            break;

                        /*
                        The order is no longer on the order book. Sent for all orders for which there was a received message. This message can result from an 
                        order being canceled or filled. There will be no more messages for this order_id after a done message. remaining_size indicates how 
                        much of the order went unfilled; this will be 0 for filled orders.

                        market orders will not have a remaining_size or price field as they are never on the open order book at a given price.
                        */
                        case "done":

                            var doneMsg = message as RealtimeDone;

                            bool isFilled = doneMsg == null || !doneMsg.IsCancelled;

                            core.ExchangePipe.SendMessage(new OrderStateChangedMessage(order, rmwo.time, isFilled ? TradeOrder.State.Filled : TradeOrder.State.Cancelled));

                            break;

                        /*
                        An order has changed. This is the result of self-trade prevention adjusting the order size or available funds. 
                        Orders can only decrease in size or funds. change messages are sent anytime an order changes in size; this includes 
                        resting orders (open) as well as received but not yet open. change messages are also sent when a new market order
                        goes through self trade prevention and the funds for the market order have changed.
                        */
                        case "change":
                            core.ExchangePipe.SendMessage(new OrderStateChangedMessage(order, rmwo.time, TradeOrder.State.Changed));
                            break;
                    }
                }
            }
            
        }

        private void OnWebsocketDiedAndReconnected()
        {
            Console.WriteLine("GDAX websocket died and reconnected...");

            core.RestartTrading();
        }
    }
}
