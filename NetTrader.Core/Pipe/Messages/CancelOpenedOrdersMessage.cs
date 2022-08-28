/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Pipe.Interfaces;

namespace NetTrader.Core.Pipe.Messages
{
    public class CancelOpenedOrdersMessage : IPipeMessage
    {
        public int RetryCount => 5;

        private TradeOrder.Type? orderType;

        public CancelOpenedOrdersMessage()
        {
            
        }

        public CancelOpenedOrdersMessage(TradeOrder.Type orderType)
        {
            this.orderType = orderType;
        }

        public virtual async Task<bool> ProcessAsync(TraderCore core)
        {
            core.OrdersWatchDog.Clear();

            var gateway = core.TradeGateway;
            var database = core.Database;

            //TODO: vybrat sell orders z DB a porovnat, zda nemame neco leaknute - pokud neexistuje objednavka na burze, tak ji cancelneme

            var openOrders = orderType.HasValue ? database.GetOpenOrdersOfType(orderType.Value) : database.GetOpenOrders();

            await gateway.CancelOrders(openOrders);

            if (openOrders != null)
            {
                foreach(var dbOrder in openOrders)
                {
                    if(dbOrder != null)
                    {
                        dbOrder.OrderState = TradeOrder.State.Cancelled;

                        database.UpdateOrder(dbOrder);
                    }
                }
            }

            return true;
        }
    }
}
