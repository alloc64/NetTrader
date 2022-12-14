/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.Pipe.Messages
{
    public class CurrencySoldMessage : TradeMessageWithOrder
    {
        public CurrencySoldMessage(TradeOrder order, DateTime timestamp) : base(order, timestamp)
        {
        }

        public override async Task<bool> ProcessAsync(TraderCore core)
        {
            return await core.Strategy.OnCurrencySold(Timestamp, Order);
        }

        public override string ToString()
        {
            return string.Format("[CurrencySoldMessage: order={0}, RetryCount={1}]", Order, RetryCount);
        }
    }
}
