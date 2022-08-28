/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Pipe.Messages;

namespace NetTrader.Core.OrderProcessing
{
    public class ProductionOrderProcessor : IOrderProcessor
    {
        private TraderCore core;

        public ProductionOrderProcessor(TraderCore core)
        {
            this.core = core;
        }

        //

        public async Task<TradeOrder> BuyCurrency(string currencyPair, int tierId, float price, float amount, long timestamp)
        {
            Console.WriteLine("Trying to set BUY order " + amount + " at price " + price);

            var msg = new BuyCurrencyAtPrice(currencyPair,
                                             tierId: tierId,
                                             price: price,
                                             amount: amount,
                                             allowTaker: false);

            msg.IgnoreOrdersWatchDog = true;
            var result = await msg.ProcessAsync(core);

            return result ? msg.BuyOrder : null;
        }

        public async Task<TradeOrder> SellCurrency(string currencyPair, int tierId, float price, float amount, TradeOrder parentBuyOrder, long timestamp)
        {
            Console.WriteLine("Trying to set SELL order " + amount + " at price " + price);

            var msg = new SellCurrencyAtPrice(currencyPair,
                                              tierId: tierId,
                                              price: price,
                                              amount: amount,
                                              parentBuyOrder: parentBuyOrder,
                                              allowTaker: false);

            msg.IgnoreOrdersWatchDog = true;
            var result = await msg.ProcessAsync(core);

            return result ? msg.SellOrder : null;
        }

        public Task<bool> CancelOrder(Guid orderId)
        {
            return new CancelOrderMessage(orderId).ProcessAsync(core);
        }

        public Task<bool> CancelAllPendingOrders()
        {
            return new CancelOpenedOrdersMessage().ProcessAsync(core);
        }
    }

}
