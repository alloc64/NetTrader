/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using NetTrader.Core.Database.Table;
using GDAX.NET;

namespace NetTrader.Core.Gateways
{
    public class GDAXExchangeOrder : ExchangeOrder
    {
        public GDAXExchangeOrder(Order order) : base(Exchange.GDAX)
        {
            Id = order.id;
            Type = order.side == "buy" ? TradeOrder.Type.Buy : TradeOrder.Type.Sell;
            CurrencyPair = order.product_id;
            Price = order.price;
            Amount = order.size;

            switch(order.status)
            {
                case "pending":
                    State = TradeOrder.State.Created;
                    break;

                case "open":
                    State = TradeOrder.State.Open;
                    break;

                case "settled":
                case "done":
                    State = TradeOrder.State.Filled;
                    break;

                case "rejected":
                    State = TradeOrder.State.Cancelled;
                    break;
            }

            Succeded = State != TradeOrder.State.Cancelled;
        }
    }
}
