/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.OrderProcessing;

namespace NetTrader.Core.Gateways
{
    public interface ITradeGateway
    {
        Task<List<ExchangeOrder>> GetOpenOrders();
        
        Task<bool> CancelLimitOrder(Guid orderId);

        Task CancelOrders(IEnumerable<TradeOrder> knownOrders);
        Task CancelOrdersOfType(TradeOrder.Type type, List<Guid> knownOrders);

        Task<ExchangeOrder> PlaceMarketBuyOrder(string currencyPair, float amount);
        Task<ExchangeOrder> PlaceMarketSellOrder(string currencyPair, float amount);

        Task<ExchangeOrder> PlaceLimitBuyOrder(string currencyPair, float price, float amount, bool allowTaker, TimeInForce timeInForce);
        Task<ExchangeOrder> PlaceLimitSellOrder(string currencyPair, float price, float amount, bool allowTaker, TimeInForce timeInForce);
    }
}
