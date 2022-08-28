/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetTrader.Core;
using NetTrader.Core.Gateways.GDAX;
using NetTrader.Core.TradingStrategies.Impl.Phased;
using NetTrader.Core.Utils;
using NetTrader.Tests;

namespace NetTrader.Strategies.Tiered
{
    public class TieredStrategyBacktestProcessor : BacktestProcessorBase
    {
        private Gateways.GDAXProductGateway gateway = new Gateways.GDAXProductGateway();

        private NumberFormatInfo nfi;

        //

        public TieredStrategyBacktestProcessor() : base("BTC-EUR", Settings.Instance.Generic.Budget, 0f, 0f)
        {
            this.nfi = new NumberFormatInfo()
            {
                NumberDecimalSeparator = "."
            };

            SetStrategy(new TieredStrategyTest(this));
        }

        public override async Task Start()
        {
            await base.Start();

            var trades = await LoadCachedTrades("BTC-EUR", DateTime.Now.AddHours(0), DateTime.Now.AddHours(-12));

            using (StreamWriter file = new StreamWriter("../../HTML/history.js"))
            {
                file.WriteLine("var historyDataPoints = [");

                int count = trades.Count;

                for (int i = 0; i < count; i++)
                {
                    var match = trades[i];

                    file.Write("[");
                    file.Write(string.Format("\"{0}\", {1}", match.Time.ToDateTime().ToString(), match.Price.ToString(nfi)));
                     
                    var tradeStates = await ProcessTick(match);

                    int buy = 0, sell = 0;
                    if (tradeStates.Count > 0)
                    {
                        foreach (var state in tradeStates)
                        {
                            if (state == TradeState.Buy)
                                buy++;
                            else if (state == TradeState.Sell)
                                sell++;
                        }
                    }

                    if (buy > 0)
                    {
                        file.Write(string.Format(", {0}, \"buy trades {1}\"", match.Price.ToString(nfi), buy));
                    }
                    else if (sell > 0)
                    {
                        file.Write(string.Format(", {0}, \"sell trades {1}\"", match.Price.ToString(nfi), sell));
                    }
                    else
                    {
                        file.Write(", null, null");
                    }

                    file.WriteLine("],");
                }

                file.WriteLine("]");
            }
        }

        private async Task<List<GDAXExchangeMatch>> LoadCachedTrades(string currencyPair, DateTime fromDate, DateTime toDate)
        {
            string path = "../../Cached/test_1_" + currencyPair + ".csv";

            List<GDAXExchangeMatch> matchTrades = null;
            bool exists = File.Exists(path);

            if (!exists)
            {
                Console.WriteLine("Retrieving historic data online for " + currencyPair);
                matchTrades = await gateway.GetHistoricMatchTrades(currencyPair, fromDate, toDate);

                gateway.DumpExchangeMatchesToCSV(path, matchTrades);
            }
            else
                matchTrades = gateway.GetHistoricTradesFromCSV(path);

            if (matchTrades == null)
                matchTrades = new List<GDAXExchangeMatch>();

            return matchTrades;
        }
    }
}
