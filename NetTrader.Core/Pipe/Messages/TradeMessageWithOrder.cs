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
    public abstract class TradeMessageWithOrder : IPipeMessage
    {
        public readonly TradeOrder Order;
        public readonly DateTime Timestamp;

        public int RetryCount => 5;

        public TradeMessageWithOrder(TradeOrder order, DateTime timestamp)
        {
            this.Order = order;
            this.Timestamp = timestamp;
        }

        public abstract Task<bool> ProcessAsync(TraderCore core);
    }
}
