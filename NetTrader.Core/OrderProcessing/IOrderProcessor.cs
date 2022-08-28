/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.OrderProcessing
{
    public interface IOrderProcessor
    {
        Task<TradeOrder> BuyCurrency(string currencyPair, int tierId, float price, float amount, long timestamp);

        Task<TradeOrder> SellCurrency(string currencyPair, int tierId, float price, float amount, TradeOrder parentBuyOrder, long timestamp);

        Task<bool> CancelOrder(Guid orderId);

        Task<bool> CancelAllPendingOrders();
    }

}
