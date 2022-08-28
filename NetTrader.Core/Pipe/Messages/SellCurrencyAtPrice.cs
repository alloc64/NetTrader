/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;

namespace NetTrader.Core.Pipe.Messages
{
    public class SellCurrencyAtPrice : TradeCurrencyPairMessage
    {
        private float price;

        private int tierId = -1;

        private float amount;

        private bool allowTaker;

        protected TradeOrder parentBuyOrder;

        //

        public TradeOrder SellOrder { get; private set; }

        //

        protected SellCurrencyAtPrice(string currencyPair, TradeOrder parentBuyOrder) : base(currencyPair)
        {
            this.parentBuyOrder = parentBuyOrder;
        }

        public SellCurrencyAtPrice(string currencyPair, 
                                   int tierId, 
                                   float price, 
                                   float amount, 
                                   TradeOrder parentBuyOrder, 
                                   bool allowTaker) 
            : base(currencyPair)
        {
            this.price = price;
            this.tierId = tierId;
            this.amount = amount;
            this.parentBuyOrder = parentBuyOrder;
            this.allowTaker = allowTaker;
        }

        public SellCurrencyAtPrice(string currencyPair, 
                                   float price, 
                                   float amount, 
                                   TradeOrder parentBuyOrder) 
            : base(currencyPair)
        {
            this.price = price;
            this.amount = amount;
            this.parentBuyOrder = parentBuyOrder;
        }

        public override async Task<bool> ProcessAsync(TraderCore core)
        {
            this.SellOrder = await PlaceSellOrder(core,
                                                 parentBuyOrder == null ? Guid.Empty : parentBuyOrder.OrderId,
                                                 parentBuyOrder == null ? tierId : parentBuyOrder.TierId,
                                                 price,
                                                 amount);

            return SellOrder != null;
        }

        protected async Task<TradeOrder> PlaceSellOrder(TraderCore core,
                                                        Guid parentBuyOrderId,
                                                        int tierId,
                                                        float finalPrice,
                                                        float amount)
        {
            ExchangeOrder exchangeOrder = null;

            exchangeOrder = await core.TradeGateway.PlaceLimitSellOrder(CurrencyPair, finalPrice, amount, allowTaker, TimeInForce);

            bool success = exchangeOrder != null && exchangeOrder.Succeded;

            if (success)
            {
                TradeOrder sellOrder = new TradeOrder(TradeOrder.Type.Sell, CurrencyPair, tierId, finalPrice, amount)
                {
                    ParentOrderId = parentBuyOrderId
                };

                sellOrder.OrderId = exchangeOrder.Id;
                sellOrder.OrderState = TradeOrder.State.Open;

                core.Database.InsertOrder(sellOrder);

                core.Server.SendMessage(new Server.Messages.OrderStateChangedMessage(sellOrder));

                if (exchangeOrder.State == TradeOrder.State.Filled)
                    core.TradingPipe.SendMessage(new OrderStateChangedMessage(sellOrder, sellOrder.Timestamp, TradeOrder.State.Filled));

                return sellOrder;
            }
            else
            {
                Console.WriteLine("Failed to set sell order " + CurrencyPair + " for price: " + finalPrice + ", amount: " + amount);

                if(!IgnoreOrdersWatchDog)
                    core.OrdersWatchDog.EnqueueOrderMessage(this);
            }

            return null;
        }
    }
}
