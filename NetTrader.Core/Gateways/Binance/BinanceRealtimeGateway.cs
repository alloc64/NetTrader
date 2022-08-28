/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using BinanceExchange.API.Utility;
using BinanceExchange.API.Websockets;
using NetTrader.Core.Database.Extensions;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Pipe.Messages;

namespace NetTrader.Core.Gateways.Binance
{
    public class BinanceRealtimeGateway : BinanceGateway, IRealtimeGateway
    {
        private TraderCore core;

        private bool running = false;

        public BinanceRealtimeGateway(TraderCore core, string currencyPair) : base(currencyPair)
        {
            this.core = core;
        }

        public void Start()
        {
            Task.Run(WorkerMethod);
        }

        private async Task WorkerMethod()
        {
            using (var binanceWebSocketClient = new DisposableBinanceWebSocketClient(client))
            {
                /*
                await binanceWebSocketClient.ConnectToUserDataWebSocket(new UserDataWebSocketMessages()
                {
                    AccountUpdateMessageHandler = (data) => 
                    {
                        Console.WriteLine(data.ToString());
                    },
                    OrderUpdateMessageHandler = (data) => 
                    {
                        Console.WriteLine(data.ToString());

                        var order = core.Database.GetOrder(data.OrderId.ToGuid());

                        switch (data.OrderStatus)
                        {
                            case BinanceExchange.API.Enums.OrderStatus.New:
                                core.ExchangePipe.SendMessage(new OrderStateChangedMessage(order, TradeOrder.State.Open));
                                break;

                            case BinanceExchange.API.Enums.OrderStatus.Filled:
                                core.ExchangePipe.SendMessage(new OrderStateChangedMessage(order, TradeOrder.State.Filled));
                                break;

                            case BinanceExchange.API.Enums.OrderStatus.Cancelled:
                                core.ExchangePipe.SendMessage(new OrderStateChangedMessage(order, TradeOrder.State.Cancelled));
                                break;

                            case BinanceExchange.API.Enums.OrderStatus.PartiallyFilled:
                                core.ExchangePipe.SendMessage(new OrderStateChangedMessage(order, TradeOrder.State.Changed));
                                break;
                        }
                    },
                    TradeUpdateMessageHandler = (data) => 
                    {
                        Console.WriteLine(data.ToString());   
                    }
                });
                */

                binanceWebSocketClient.ConnectToTradesWebSocket(FixCurrencyPair(currencyPair), (match) => 
                {
                    var type = match.WasBuyerMaker ? TradeOrder.Type.Buy : TradeOrder.Type.Sell;

                    core.ExchangePipe.SendMessage(new MatchTradeMessage(new BinanceExchangeMatch(currencyPair, match.Price, type, match.Quantity, match.TradeTime.ToTimestamp())));
                });

                while(running)
                {
                    await Task.Delay(1);
                }
            }
        }
    }
}
