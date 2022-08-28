/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Pipe.Interfaces;

namespace NetTrader.Core.Pipe.Messages
{
    public class OrderStateChangedMessage : IPipeMessage
    {
        public int RetryCount => 5;

        private TradeOrder order;
        private TradeOrder.State state;
        private DateTime timestamp;

        public OrderStateChangedMessage(TradeOrder order, DateTime timestamp, TradeOrder.State state)
        {
            this.order = order;
            this.timestamp = timestamp;
            this.state = state;
        }

        public async Task<bool> ProcessAsync(TraderCore core)
        {
            order.OrderState = state;
            core.Database.UpdateOrder(order);

            core.Server.SendMessage(new Server.Messages.OrderStateChangedMessage(order));

            switch (state)
            {
                case TradeOrder.State.Received:
                    OrderReceived(core);
                    break;
                case TradeOrder.State.Open:
                    OrderOpen(core);
                    break;
                case TradeOrder.State.Changed:
                    OrderChanged(core);
                    break;
                case TradeOrder.State.Filled:
                    OrderFilled(core);
                    break;
                case TradeOrder.State.Cancelled:
                    OrderCanceled(core);
                    break;
            }

            return await Task.FromResult(true);
        }

        #region State Changes

        private void OrderCanceled(TraderCore core)
        {

        }

        private void OrderFilled(TraderCore core)
        {
            switch (order.OrderType)
            {
                /*
                podarilo se nam nakoupit
                */
                case TradeOrder.Type.Buy:
                    core.TradingPipe.SendMessage(new CurrencyBoughtMessage(order, timestamp));
                    break;

                /*
                podarilo se nam prodat
                pokud dojde k uspesnymu prodeji, tak
                markne parent buy order jako Traded aby ji bot znovu nepokousel prodat
                */
                case TradeOrder.Type.Sell:
                    core.TradingPipe.SendMessage(new CurrencySoldMessage(order, timestamp));
                    break;
            }
        }

        private void OrderChanged(TraderCore core)
        {

        }

        private void OrderOpen(TraderCore core)
        {

        }

        private void OrderReceived(TraderCore core)
        {

        }

        #endregion

        public override string ToString()
        {
            return string.Format("[OrderStateChangedMessage: order={0}, state={1}]", order, state);
        }
    }
}
