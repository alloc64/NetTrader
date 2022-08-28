/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.OrderProcessing;
using NetTrader.Core.TradingStrategies;
using NetTrader.Core.TradingStrategies.Impl.Tiered.Tiers;
using NetTrader.Core.TradingStrategies.Impl.Tiers;
using NetTrader.Core.Utils;

namespace NetTrader.Strategies.Impl.StopLoss
{
    public class StopLossStrategyTest : TradingStrategyBase
    {
        private class PendingOrder
        {
            public float BuyPrice { get; set; }
            public float SellPrice { get; set; }
            public TradeOrder Order { get; set; }
        }

        private class CurrencyAsset
        {
            public float Price { get; set; }
        }

        private string currencyPair = "BTC-EUR";

        private float TBP = 5540f;
        private float BTC_budget = 0.01f;
        private float EUR_budget = 100f;

        private float maxProfit = 0.01f;
        private float maxLoss = 0.02f;
        private float step = 12f * 100f;

        private StopLossBacktestProcessor processor;

        private PendingOrder pendingOrder;

        private Dictionary<int, CurrencyAsset> boughtAssets = new Dictionary<int, CurrencyAsset>();

        //

        public StopLossStrategyTest(StopLossBacktestProcessor processor)
        {
            this.processor = processor;
        }

        private void PlaceOrder(TradeOrder.Type type)
        {
            int i = 1;

            var buyPrice = TBP - ((i * step) * maxLoss);
            var sellPrice = TBP - ((i * step) * maxProfit);

            var p = type == TradeOrder.Type.Buy ? buyPrice : sellPrice;

            Console.WriteLine("Place order: " + type + " for " + p);
            //Console.WriteLine("buyPrice: " + buyPrice + " sellPrice " + sellPrice);

            var order = processor.SubmitOrder(currencyPair, p, 0.01f, type, DateTimeUtils.Timestamp);

            pendingOrder = new PendingOrder()
            {
                Order = order,
                BuyPrice = buyPrice,
                SellPrice = sellPrice
            };
        }

        #region 

        public override void RegisterGateways(Exchange exchangeType)
        {
        }

        public override Task<bool> RestartTrading()
        {
            processor.CancelAllOrders();

            PlaceOrder(TradeOrder.Type.Buy);

            return Task.FromResult(true);
        }

        #endregion

        public override Task<bool> OnMatchOccured(ExchangeMatch match)
        {
            return Task.FromResult(true);
        }

        public override async Task<bool> OnCurrencyBought(DateTime timestamp, TradeOrder order)
        {
            await base.OnCurrencyBought(timestamp, order);

            PlaceOrder(TradeOrder.Type.Sell);

            return true;
        }

        public override async Task<bool> OnCurrencySold(DateTime timestamp, TradeOrder order)
        {
            await base.OnCurrencySold(timestamp, order);

            if (pendingOrder != null)
            {
                FiatProfit += (order.Price - pendingOrder.Order.Price) * order.Amount;
            }

            PlaceOrder(TradeOrder.Type.Buy);

            return true;
        }
    }
}
