/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Gateways;
using NetTrader.Core.Pipe.Messages;
using NetTrader.Core.Utils;

namespace NetTrader.Core.TargetBasePrice
{
    public class TargetBasePriceCalculator
    {
        private TraderCore core;

        private float timestamp = -1f;

        public TargetBasePriceCalculator(TraderCore core)
        {
            this.core = core;
        }

        public Task ProcessMatchTrade(ExchangeMatch match)
        {
            var basePriceWithOffset = core.BasePrice + core.TBPRecalculationOffset;
                                         
            if(match.Price >= basePriceWithOffset)
            {
                if (timestamp <= 0f)
                {
                    Console.WriteLine("Match price " + match.Price + " is larger or eq than " + basePriceWithOffset + " preparing for TBP recalculation.");
                    timestamp = DateTimeUtils.Timestamp;
                }
            }
            else
            {
                timestamp = -1f;
            }

            if(timestamp > 0f && DateTimeUtils.Timestamp - timestamp > 20 * 60f)
            {
                float price = match.Price - core.TBPRecalculationPriceOffset;

                Console.WriteLine("Automatically settings Target Base Price to " + price);
                core.TradingPipe.SendMessage(new SetTargetBasePriceMessage(price));

                timestamp = -1;
            }

            return Task.FromResult(false);
        }
    }
}
