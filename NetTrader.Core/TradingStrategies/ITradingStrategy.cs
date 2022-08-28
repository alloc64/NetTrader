/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.Pipe.Messages;

namespace NetTrader.Core.TradingStrategies
{
    public interface ITradingStrategy
    {
        float FiatProfit { get; set; }

        float CoinProfit { get; set; }

        //

        void RegisterGateways(Exchange exchangeType);

        Task<bool> RestartTrading();

        Task<bool> OnMatchOccured(ExchangeMatch match);

        Task<bool> OnCurrencyBought(DateTime timestamp, TradeOrder order);

        Task<bool> OnCurrencySold(DateTime timestamp, TradeOrder order);

        string DumpProfits();
    }
}
