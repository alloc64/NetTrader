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
    public class BuyCurrencyAtPrice : TradeCurrencyPairMessage
    {
        private int tierId;

        protected float price;

        private float amount;

        private bool allowTaker;

        //

        public TradeOrder BuyOrder { get; private set; }

        //

        public BuyCurrencyAtPrice(string currencyPair) : base(currencyPair)
        {
            
        }

        public BuyCurrencyAtPrice(string currencyPair, int tierId, float price, float amount, bool allowTaker) : base(currencyPair)
        {
            this.tierId = tierId;
            this.price = price;
            this.amount = amount;
            this.allowTaker = allowTaker;
        }

        public override async Task<bool> ProcessAsync(TraderCore core)
        {
            this.BuyOrder = await PlaceBuyOrder(core, tierId, amount);

            return BuyOrder != null;
        }

        protected async Task<TradeOrder> PlaceBuyOrder(TraderCore core,
                                                       int tierId,
                                                       float currencyAmount)
        {
            ExchangeOrder exchangeOrder = await core.TradeGateway.PlaceLimitBuyOrder(CurrencyPair, price, currencyAmount, allowTaker, TimeInForce);

            bool success = exchangeOrder != null && exchangeOrder.Succeded;

            if (success)
            {
                TradeOrder buyOrder = new TradeOrder(TradeOrder.Type.Buy, CurrencyPair, tierId, price, currencyAmount);

                buyOrder.OrderId = exchangeOrder.Id;
                buyOrder.OrderState = TradeOrder.State.Open;

                core.Database.InsertOrder(buyOrder);

                core.Server.SendMessage(new Server.Messages.OrderStateChangedMessage(buyOrder));

                if (exchangeOrder.State == TradeOrder.State.Filled)
                    core.TradingPipe.SendMessage(new OrderStateChangedMessage(buyOrder, buyOrder.Timestamp, TradeOrder.State.Filled));

                return buyOrder;
            }
            else
            {
                Console.WriteLine("Failed to set buy order " + CurrencyPair + " for price: " + price + ", amount: " + amount);

                if (!IgnoreOrdersWatchDog)
                    core.OrdersWatchDog.EnqueueOrderMessage(this);
            }

            return null;
        }
    }
}
