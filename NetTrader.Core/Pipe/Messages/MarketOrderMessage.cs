/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.Pipe.Interfaces;

namespace NetTrader.Core.Pipe.Messages
{
    public class MarketOrderMessage : IPipeMessage
    {
        public int RetryCount => 1;

        private Guid orderId;

        public MarketOrderMessage(string payload)
        {
            Guid.TryParse(payload, out orderId);
        }

        public async Task<bool> ProcessAsync(TraderCore core)
        {
            var order = core.Database.GetOrder(orderId);

            if (order == null)
            {
                Console.WriteLine("Failed to process MarketOrderMessage, because order == null.");
                return true;
            }

            ExchangeOrder exchangeOrder = null;

            switch (order.OrderType)
            {
                case TradeOrder.Type.Buy:

                    exchangeOrder = await core.TradeGateway.PlaceMarketBuyOrder(order.CurrencyPair, order.Amount);

                    break;

                case TradeOrder.Type.Sell:

                    await new CancelOrderMessage(order.OrderId).ProcessAsync(core);

                    await Task.Delay(2000);

                    exchangeOrder = await core.TradeGateway.PlaceMarketSellOrder(order.CurrencyPair, order.Amount);

                    break;
            }

            bool success = exchangeOrder != null && exchangeOrder.Succeded;

            if (success)
            {
                order.Price = exchangeOrder.Price;
                core.Database.UpdateOrder(order);

                core.TradingPipe.SendMessage(new OrderStateChangedMessage(order, DateTime.Now, TradeOrder.State.Filled));
            }

            return success;
        }
    }
}
