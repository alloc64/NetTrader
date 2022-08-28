/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.OrderProcessing;
using NetTrader.Tests;

namespace NetTrader.OrderProcessing
{
    public class TestsOrderProcessor : IOrderProcessor
    {
        private BacktestProcessorBase backtestProcessor;

        public TestsOrderProcessor(BacktestProcessorBase backtestProcessor)
        {
            this.backtestProcessor = backtestProcessor;
        }

        public Task<TradeOrder> BuyCurrency(string currencyPair, int tierId, float price, float amount, long timestamp)
        {
            var order = backtestProcessor.SubmitOrder(currencyPair, price, amount, TradeOrder.Type.Buy, timestamp);
            return Task.FromResult(order);
        }

        public Task<TradeOrder> SellCurrency(string currencyPair, int tierId, float price, float amount, TradeOrder parentBuyOrder, long timestamp)
        {
            var order = backtestProcessor.SubmitOrder(currencyPair, price, amount, TradeOrder.Type.Sell, timestamp);
            return Task.FromResult(order);
        }

        public Task<bool> CancelOrder(Guid orderId)
        {
            backtestProcessor.CancelOrder(orderId);
            return Task.FromResult(true);
        }

        public Task<bool> CancelAllPendingOrders()
        {
            backtestProcessor.CancelAllOrders();
            return Task.FromResult(true);
        }
    }
}
