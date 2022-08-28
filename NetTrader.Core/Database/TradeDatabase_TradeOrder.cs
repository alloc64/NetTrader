/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.Database
{
    public partial class TradeDatabase
    {
        public TradeOrder GetOpenOrderInTier(TradeOrder.Type type, int tierId)
        {
            return connection.Table<TradeOrder>().Where((o) => o.OrderType == type &&
                                                        o.TierId == tierId &&
                                                        (o.OrderState != TradeOrder.State.Filled && o.OrderState != TradeOrder.State.Traded && o.OrderState != TradeOrder.State.Cancelled)).FirstOrDefault();
        }

        public TradeOrder GetOrder(Guid orderId)
        {
            return connection.Table<TradeOrder>().Where((o) => o.OrderId == orderId).FirstOrDefault();
        }

        public void InsertOrder(TradeOrder order)
        {
            connection.Insert(order);
        }

        public void UpdateOrder(TradeOrder order)
        {
            connection.Update(order);
        }

        public IEnumerable<TradeOrder> GetOrders()
        {
            return connection.Table<TradeOrder>();
        }

        public IEnumerable<TradeOrder> GetOrdersOfType(TradeOrder.Type type)
        {
            return connection.Table<TradeOrder>().Where(o => o.OrderType == type);
        }

        public IEnumerable<TradeOrder> GetOpenOrdersOfType(TradeOrder.Type type)
        {
            return connection.Table<TradeOrder>().Where((o) => o.OrderType == type &&
                                                        o.OrderState != TradeOrder.State.Filled &&
                                                        o.OrderState != TradeOrder.State.Traded &&
                                                        o.OrderState != TradeOrder.State.Cancelled);
        }

        public IEnumerable<TradeOrder> GetOpenOrders()
        {
            return connection.Table<TradeOrder>().Where((o) => o.OrderState != TradeOrder.State.Filled &&
                                                        o.OrderState != TradeOrder.State.Traded &&
                                                        o.OrderState != TradeOrder.State.Cancelled);
        }

        public IEnumerable<TradeOrder> GetFilledOrders(TradeOrder.Type type)
        {
            return connection.Table<TradeOrder>().Where((o) => o.OrderState == TradeOrder.State.Filled &&
                                                        o.OrderType == type);
        }


        public IEnumerable<TradeProfit> GetProfits()
        {
            var buyOrders = connection.Table<TradeOrder>().Where((o) => o.OrderType == TradeOrder.Type.Buy 
                                                                 && (o.OrderState == TradeOrder.State.Filled || o.OrderState == TradeOrder.State.Traded));
            List<TradeProfit> profitList = new List<TradeProfit>();

            foreach(var buyOrder in buyOrders)
            {
                var sellOrder = connection.Table<TradeOrder>().Where(o => o.ParentOrderId == buyOrder.OrderId 
                                                                     && (o.OrderState == TradeOrder.State.Filled || o.OrderState == TradeOrder.State.Traded)).FirstOrDefault();

                if(sellOrder != null)
                {
                    profitList.Add(new TradeProfit(buyOrder, sellOrder));
                }
            }

            return profitList;
        }

        public void CancelOrder(Guid orderId)
        {
            var order = GetOrder(orderId);

            if (order != null)
            {
                order.OrderState = TradeOrder.State.Cancelled;
                connection.Update(order);
            }
        }
    }
}
