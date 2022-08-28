/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Pipe.Interfaces;

namespace NetTrader.Core.Pipe.Messages
{
    public class StopLossMessage : IPipeMessage
    {
        public int RetryCount => 1;

        public async Task<bool> ProcessAsync(TraderCore core)
        {
            if(core.LastMatch == null)
            {
                Console.WriteLine("Cannot trigger stop loss, because of none last match info.");
                return true;
            }

            await core.StopLossProcessor.TriggerStopLoss(core.LastMatch);

            return true;
        }
    }
}
