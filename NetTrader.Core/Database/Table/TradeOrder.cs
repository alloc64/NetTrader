/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using NetTrader.Core.Database.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SQLite.Net.Attributes;

namespace NetTrader.Core.Database.Table
{
    public class TradeOrder
    {
        public enum Type
        {
            Unknown,
            Buy,
            Sell
        }

        public enum State
        {
            Created,
            Received,
            Open,

            /*
            An order has changed. This is the result of self-trade prevention adjusting the order size or available funds. 
            Orders can only decrease in size or funds. change messages are sent anytime an order changes in size; this includes 
            resting orders (open) as well as received but not yet open. change messages are also sent when a new market order
            goes through self trade prevention and the funds for the market order have changed.
            */
            Changed,
            Filled,
            Cancelled,
            /*
            An order that made trade with profit
            */
            Traded
        }

        public TradeOrder()
        {
            
        }

        public TradeOrder(Type orderType, string currencyPair, Tier tier, float price) 
            : this(orderType, currencyPair, tier.Id, price, tier.CurrencyAmount)
        {
        }

        public TradeOrder(Type orderType, string currencyPair, int tierId, float price, float amount) : this()
        {
            this.OrderType = orderType;

            this.CurrencyPair = currencyPair;

            this.TierId = tierId;

            this.Amount = amount;

            this.Price = price;

            this.Timestamp = DateTime.Now;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string CurrencyPair { get; set;  }

        public int TierId { get; set; }

        [Unique]
        public Guid OrderId { get; set; }

        public Guid ParentOrderId { get; set; }

        public float Amount { get; set; }

        public float Price { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Type OrderType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public State OrderState { get; set; }

        public float GetFee(float fee)
        {
            return (Price / 100f) * Amount * fee;
        }

        public override string ToString()
        {
            return string.Format("[TradeOrder: Id={0}, CurrencyPair={1}, TierId={2}, OrderId={3}, ParentOrderId={4}, Amount={5}, Price={6}, OrderType={7}, OrderState={8}]", Id, CurrencyPair, TierId, OrderId, ParentOrderId, Amount, Price, OrderType, OrderState);
        }
    }
}
