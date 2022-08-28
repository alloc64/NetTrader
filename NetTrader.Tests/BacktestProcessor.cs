/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core.Gateways;
using NetTrader.Core.TradingStrategies;

namespace NetTrader.Tests
{
    public abstract class BacktestProcessor : BacktestProcessorBase
    {
        private List<ExchangeMatch> historyMatches = new List<ExchangeMatch>();

        public BacktestProcessor(string currencyPair, 
                                 float fiatFunds, 
                                 float coinFunds, 
                                 float fee) 
            : base(currencyPair, fiatFunds, coinFunds, fee)
        {
        }


        public async Task Start(List<ExchangeMatch> historyMatches)
        {
            this.historyMatches = historyMatches;

            foreach(var m in historyMatches)
            {
                await ProcessTick(m);
            }
        }
    }
}
