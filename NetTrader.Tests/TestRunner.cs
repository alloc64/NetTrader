/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System.Threading.Tasks;
using NetTrader.Core;

namespace NetTrader.Tests
{
    public class TestRunner
    {
        //private BacktestProcessorBase backTestProcessor = new Strategies.Phased.PhasedStrategyBackTestProcessor();
        private BacktestProcessorBase backTestProcessor = new Strategies.Impl.StopLoss.StopLossBacktestProcessor();

        static TestRunner()
        {
            Settings.Load(@"../../../etc/settings.json");
        }

        public TestRunner()
        {
        }

        public async Task Start()
        {
            await backTestProcessor.Start();
            backTestProcessor.DumpProfits();
        }
    }
}
