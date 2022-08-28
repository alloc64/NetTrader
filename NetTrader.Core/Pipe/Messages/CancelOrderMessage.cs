/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Pipe.Interfaces;

namespace NetTrader.Core.Pipe.Messages
{
    public class CancelOrderMessage : IPipeMessage
    {
        public int RetryCount => 1;

        private Guid orderId;

        //

        public CancelOrderMessage(string orderId) : this(new Guid(orderId))
        {
        }

        public CancelOrderMessage(Guid orderId)
        {
            this.orderId = orderId;
        }

        public async Task<bool> ProcessAsync(TraderCore core)
        {
            var database = core.Database;

            Guid? parentBuyOrderId = null;

            var order = database.GetOrder(orderId);

            if (order != null)
            {
                CancelOrder(database, order);

                if(order.OrderType == TradeOrder.Type.Sell)
                {
                    var parentBuyOrder = database.GetOrder(order.ParentOrderId);

                    if(parentBuyOrder != null)
                    {
                        parentBuyOrderId = parentBuyOrder.OrderId;
                        CancelParentBuyOrder(database, parentBuyOrder);
                    }
                }
            }

            await core.TradeGateway.CancelLimitOrder(orderId);

            if(parentBuyOrderId.HasValue)
                await core.TradeGateway.CancelLimitOrder(parentBuyOrderId.Value);
            
            return true;
        }

        private void CancelOrder(TradeDatabase database, TradeOrder order)
        {
            order.OrderState = TradeOrder.State.Cancelled;
            database.UpdateOrder(order);
        }

        protected virtual void CancelParentBuyOrder(TradeDatabase database, TradeOrder order)
        {
        }
    }
}
