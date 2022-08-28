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
using System.Text;
using System.Threading.Tasks;
using NetTrader.Core;
using NetTrader.Core.Candles;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.Gateways.GDAX;
using NetTrader.Core.Pipe.Messages;
using NetTrader.Core.TradingStrategies;
using NetTrader.Core.TradingStrategies.Impl;
using NetTrader.Core.TradingStrategies.Impl.Phased;
using NetTrader.Core.Utils;
using NetTrader.OrderProcessing;
using NetTrader.Tests;

namespace NetTrader.Strategies.Phased
{
    public class PhasedStrategyBackTestProcessor : BacktestProcessorBase
    {
        private Gateways.GDAXProductGateway gateway = new Gateways.GDAXProductGateway();

        private NumberFormatInfo nfi;

        private PhasedStrategy phasedStrategy;

        private bool ShowTrends = false;

        private bool DebugTrades = false;

        //

        public PhasedStrategyBackTestProcessor() : base(currencyPair: "BTC-EUR",
                                                        fiatFunds: 50,
                                                        coinFunds: 0.5f,
                                                        fee: 0f)
        {
            this.nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            phasedStrategy = new PhasedStrategy(new TestsOrderProcessor(this),
                                                buyOrderAmount: 0.5f,
                                                sellOrderAmount: 0.5f);

            SetStrategy(phasedStrategy);
        }

        public override async Task Start()
        {
            await base.Start();

            bool useRealtimeData = false;

            if (!useRealtimeData)
            {
                var fromDate = DateTime.Now.AddHours(0);
                var toDate = DateTime.Now.AddHours(-12);

                await LoadCachedTrades("BTC-USD", fromDate, toDate);
                await LoadCachedTrades("BTC-EUR", fromDate, toDate);

                phasedStrategy.ResetSampler();

                //TODO: agregace po 10s muze delat problemy, 
                //je tedy otazka zda to nesypat ze dvou separatnich vlaken ktery budou tyto data posilat do fronty ktera si to zpracuje...
                var trades = phasedStrategy.Trades;

                using (StreamWriter file = new StreamWriter("../../HTML/PhasedStrategy/history.js"))
                {
                    file.WriteLine("var historyDataPoints = [");

                    //for (int i = 0; i < trades.Count; i++)
                    for (int i = trades.Count - 1; i >= 0; i--)
                    {
                        var t = trades[i];

                        if (t.IsValid)
                        {
                            file.Write("[");
                            file.Write(string.Format("\"{0}\",\"\",", t.Time.ToDateTime().ToString()));

                            file.Write(string.Format("{0}, \"USD price: {1}\",",
                                                     t.USDMatch.PriceNormalized.ToString(nfi),
                                                     t.USDMatch.Price.ToString(nfi)));

                            file.Write(string.Format("{0}, \"EUR price: {1} {2}\"",
                                                     t.EURMatch.PriceNormalized.ToString(nfi),
                                                     t.EURMatch.Price.ToString(nfi),
                                                     t.EURMatch.Time.ToDateTime()));

                            await ProcessTick(t.USDMatch);

                            string trendValues = ProcessTrend(t.EURMatch);

                            int buy = 0, sell = 0;

                            if (DebugTrades)
                            {
                                await ProcessTick(t.EURMatch);
                                
                                TradeData tradeState = phasedStrategy.PredictTradeState(t.EURMatch);

                                if (tradeState.State == TradeState.Buy)
                                    buy++;
                                else if (tradeState.State == TradeState.Sell)
                                    sell++;
                            }
                            else
                            {
                                var tradeStates = await ProcessTick(t.EURMatch);

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
                            }

                            if (buy > 0)
                            {
                                file.Write(string.Format(", {0}, \"{1} buy trades for {2} profit {3} EUR, {4} BTC\", null, null, {5}",
                                                         t.EURMatch.PriceNormalized.ToString(nfi),
                                                         buy,
                                                         t.EURMatch.Price.ToString(nfi),
                                                         strategy.FiatProfit.ToString("F6"),
                                                         strategy.CoinProfit.ToString("F6"),
                                                         trendValues));
                            }
                            else if (sell > 0)
                            {
                                file.Write(string.Format(", null, null, {0}, \"{1} sell trades for {2} profit {3} EUR, {4} BTC\", {5}",
                                                         t.EURMatch.PriceNormalized.ToString(nfi),
                                                         sell,
                                                         t.EURMatch.Price.ToString(nfi),
                                                         strategy.FiatProfit.ToString("F6"),
                                                         strategy.CoinProfit.ToString("F6"),
                                                         trendValues));
                            }
                            else
                            {
                                file.Write(string.Format(", null, null, null, null, {0}", trendValues));
                            }

                            file.Write("],");
                        }
                    }

                    file.Write("]");
                }
            }
            else
            {
                new Gateways.GDAXRealtimeGateway("BTC-EUR", async m => await ProcessRealtimeMatch("BTC-EUR", m)).Start();
                new Gateways.GDAXRealtimeGateway("BTC-USD", async m => await ProcessRealtimeMatch("BTC-USD", m)).Start();

                while (true)
                    await Task.Delay(100);
            }
        }

        private string ProcessTrend(ExchangeMatch match)
        {
            string trendValues = "null, null";

            var trendState = phasedStrategy.AnalyzeTrend(match);

            if (ShowTrends)
            {
                switch (trendState)
                {
                    case PhasedStrategy.Trend.Rise:
                        trendValues = string.Format("{0}, null", match.PriceNormalized.ToString(nfi));
                        break;

                    case PhasedStrategy.Trend.Fall:
                        trendValues = string.Format("null, {0}", match.PriceNormalized.ToString(nfi));
                        break;
                }
            }

            return trendValues;
        }

        private async Task ProcessRealtimeMatch(string currencyPair, GDAX.NET.RealtimeMatch realtimeMatch)
        {
            var match = GDAXExchangeMatch.FromRealtimeMatch(currencyPair, realtimeMatch);
            phasedStrategy.ProcessRealtimeMatchTrade(match);

            if (currencyPair == "BTC-EUR")
            {
                await ProcessTick(match);
            }
        }

        private async Task<List<GDAXExchangeMatch>> LoadCachedTrades(string currencyPair, DateTime fromDate, DateTime toDate)
        {
            string path = "../../Cached/12h_strong_downtrend_" + currencyPair + ".csv";

            List<GDAXExchangeMatch> matchTrades = null;
            bool exists = File.Exists(path);

            if (!exists)
            {
                Console.WriteLine("Retrieving historic data online for " + currencyPair);
                //matchTrades = await gateway.GetHistoricMatchTrades(currencyPair, DateTime.Parse("3/6/2018"), DateTime.Parse("3/1/2018"));
                //matchTrades = await gateway.GetHistoricMatchTrades(currencyPair, DateTime.Parse("2/7/2018"), DateTime.Parse("2/1/2018"));
                matchTrades = await gateway.GetHistoricMatchTrades(currencyPair, fromDate, toDate);
                //matchTrades = await gateway.GetHistoricMatchTrades(currencyPair, DateTime.Parse("3/9/2018 7:50"), DateTime.Parse("3/8/2018 22:00"));

                gateway.DumpExchangeMatchesToCSV(path, matchTrades);
            }
            else
                matchTrades = gateway.GetHistoricTradesFromCSV(path);

            if (matchTrades == null)
                matchTrades = new List<GDAXExchangeMatch>();

            phasedStrategy.ResetSampler();

            float priceMax = float.MinValue;

            for (int i = 0; i < matchTrades.Count; i++)
            {
                var match = matchTrades[i];

                if (match.Price > priceMax)
                    priceMax = match.Price;
            }

            for (int i = 0; i < matchTrades.Count; i++)
            {
                var match = matchTrades[i];
                match.PriceNormalized = match.Price / priceMax;

                phasedStrategy.ProcessMatchTrade(match);
            }

            return matchTrades;
        }
    }
}
