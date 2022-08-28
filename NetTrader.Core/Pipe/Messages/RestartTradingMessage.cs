/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Threading.Tasks;
using NetTrader.Core.Pipe.Interfaces;

namespace NetTrader.Core.Pipe.Messages
{
    public class RestartTradingMessage : IPipeMessage
    {
        public int RetryCount => 5;

        public Task<bool> ProcessAsync(TraderCore core)
        {
            core.RestartTrading();

            return Task.FromResult(true);
        }
    }
}
