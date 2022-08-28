/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Response;
using BinanceExchange.API.Utility;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.Gateways.Binance
{
    public class BinanceExchangeOrder : ExchangeOrder
    {
        private CreateOrderResponse response;

        public BinanceExchangeOrder(OrderResponse order) : base(Exchange.Binance)
        {
            Id = order.OrderId.ToGuid();
            Type = order.Side == OrderSide.Buy ? TradeOrder.Type.Buy : TradeOrder.Type.Sell;
            CurrencyPair = order.Symbol;
            Price = (float)order.Price;
            Amount = (float)order.OriginalQuantity;
            Succeded = order.Status != OrderStatus.Rejected;
        }

        public BinanceExchangeOrder(CreateOrderResponse order) : base(Exchange.Binance)
        {
            Id = order.OrderId.ToGuid();
            Type = order.Side == OrderSide.Buy ? TradeOrder.Type.Buy : TradeOrder.Type.Sell;
            CurrencyPair = order.Symbol;
            Price = (float)order.Price;
            Amount = (float)order.OriginalQuantity;
            Succeded = order.Status != OrderStatus.Rejected;
        }
    }
}
