/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Threading.Tasks;
using NetTrader.Core.Pipe.Interfaces;
using NetTrader.Core.Server.Messages;

namespace NetTrader.Core.Pipe.Messages
{
    public class GetTargetBasePrice : IPipeMessage
    {
        public class PriceInfo
        {
            public float BasePrice { get; set; }

            public float TBPRecalculationOffset { get; set; }

            public float TBPRecalculationPriceOffset { get; set;  }

            public float StopLossThreshold { get; set; }

            public float StopLossBasePriceOffset { get; set; }
        }

        public int RetryCount => 5;

        public Task<bool> ProcessAsync(TraderCore core)
        {
            var priceInfo = new PriceInfo()
            {
                BasePrice = core.BasePrice,
                TBPRecalculationOffset = core.TBPRecalculationOffset,
                TBPRecalculationPriceOffset = core.TBPRecalculationPriceOffset,
                StopLossThreshold = core.StopLossThreshold,
                StopLossBasePriceOffset = core.StopLossBasePriceOffset
            };

            core.Server.SendMessage(new ServerMessage(ServerMessage.Type.GetTargetBasePrice, priceInfo));

            return Task.FromResult(true);
        }
    }
}
