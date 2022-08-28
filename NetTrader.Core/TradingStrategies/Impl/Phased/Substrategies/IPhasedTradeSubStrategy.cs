/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;

namespace NetTrader.Core.TradingStrategies.Impl.Phased.Substrategies
{
    public interface IPhasedTradeSubStrategy
    {
        SubStrategy Type { get;  }

        TradeData PredictTradeState(ExchangeMatch match,
                                    float eurMaxPrice,
                                    float eurMinPrice,
                                    float usdCurrentPriceMaxDiff,
                                    float eurCurrentPriceMaxDiff,
                                    float eurStartPriceMaxDiff,
                                    float eurStartPriceMinDiff,
                                    TradeOrder lastProcessedOrder);

        TradeData ValidateTradeState(ExchangeMatch match,
                                     TradeData predictedState,
                                     TradeOrder currentOrder,
                                     TradeOrder lastProcessedOrder);


        Task OnCurrencyBought(TradeOrder order, DateTime timestamp, TradeOrder lastProcessedOrder);

        Task OnCurrencySold(TradeOrder order, DateTime timestamp, TradeOrder lastProcessedOrder);
    }
}
