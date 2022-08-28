/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.Utils;

namespace NetTrader.Core.TradingStrategies.Impl.Phased
{
    public class TradeData
    {
        public static readonly TradeData None = new TradeData()
        {
            State = TradeState.None,
            Gain = 0f
        };

        //

        public void Sell(float priceDiversity = 0.01f, float amount = 0f)
        {
            State = TradeState.Sell;
            Price = (float)Math.Round(basePrice + priceDiversity, 2);
            Amount = (float)Math.Round(amount, 6);
        }


        public void Buy(float priceDiversity = 0.01f, float amount = 0f)
        {
            State = TradeState.Buy;
            Price = (float)Math.Round(basePrice - priceDiversity, 2);
            Amount = (float)Math.Round(amount, 6);
        }

        public void IgnoreState()
        {
            State = TradeState.None;
            Gain = 0f;
            Price = 0f;
            Amount = 0f;
        }

        public void Validate(float defaultBuyOrderAmount, float defaultSellOrderAmount)
        {
            switch (State)
            {
                case TradeState.Buy:
                    if (Amount <= 0f)
                        Amount = defaultBuyOrderAmount;
                    break;

                case TradeState.Sell:
                    if (Amount <= 0f)
                        Amount = defaultSellOrderAmount;
                    break;
            }
        }


        //

        public TradeState State { get; private set; }

        public float Amount { get; private set; }

        public float Price { get; private set; }

        public long Time { get; private set; }

        public float Gain { get; set; }

        //

        private float basePrice = 0f;

        //

        private TradeData()
        {

        }

        public TradeData(TradeOrder order) : this(order.Price, order.Timestamp.ToUnixTimestamp())
        {

        }

        public TradeData(ExchangeMatch match) : this(match.Price, match.Time)
        {

        }

        public TradeData(float price, long time)
        {
            this.basePrice = price;
            this.Time = time;
        }

        public override string ToString()
        {
            return string.Format("State={0}, Gain={1}", State, Gain);
        }
    }
}
