/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using NetTrader.Core.Database;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.Pipe.Messages
{
    public class RemoveOrderMessage : CancelOrderMessage
    {
        public RemoveOrderMessage(string orderId) : base(orderId)
        {
        }

        protected override void CancelParentBuyOrder(TradeDatabase database, TradeOrder order)
        {
            order.OrderState = TradeOrder.State.Cancelled;
            database.UpdateOrder(order);
        }
    }
}
