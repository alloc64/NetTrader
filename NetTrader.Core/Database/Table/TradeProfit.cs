/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using NetTrader.Core.Database.Table;
using NetTrader.Core.JSON;
using Newtonsoft.Json;

namespace NetTrader.Core.Database.Table
{
    public class TradeProfit
    {
        public TradeOrder BuyOrder { get; set; }
        public TradeOrder SellOrder { get; set; }

        public TradeProfit(TradeOrder buyOrder, TradeOrder sellOrder)
        {
            this.BuyOrder = buyOrder;
            this.SellOrder = sellOrder;
        }

        [JsonConverter(typeof(DateFormatConverter), "HH:mm:ss yyyy. MM. dd")]
        public DateTime Timestamp { get { return SellOrder.Timestamp;  }}

        public float Amount { get { return SellOrder.Amount; }}

        public float Profit
        {
            get
            {
                return (SellOrder.Price - BuyOrder.Price) * SellOrder.Amount;
            }
        }

        public override string ToString()
        {
            return string.Format("[TradeProfit: BuyOrder={0}\nSellOrder={1}\nProfit={2}]", BuyOrder, SellOrder, Profit);
        }
    }
}
