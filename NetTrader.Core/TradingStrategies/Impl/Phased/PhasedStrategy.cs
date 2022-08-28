/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Extensions;
using NetTrader.Core.Gateways;
using NetTrader.Core.OrderProcessing;
using NetTrader.Core.TradingStrategies.Impl.Phased.Substrategies;
using NetTrader.Core.Utils;
using Trady.Analysis;

namespace NetTrader.Core.TradingStrategies.Impl.Phased
{
    public class PhasedStrategy : TradingStrategyBase
    {
        public enum Trend
        {
            Stagnation = 0,
            Fall = 1,
            Rise = 2,
        }

        //

        private IPhasedTradeSubStrategy strategy;

        //

        private Stopwatch sw = new Stopwatch();

        #region Stats & Fees

        public readonly float Fee = 0.0f;

        #endregion

        #region Order state

        private readonly string currencyPair = "BTC-EUR";

        public float DefaultBuyOrderAmount { get; private set; }
        public float DefaultSellOrderAmount { get; private set; }

        private TradeOrder currentOrder;

        public TradeOrder LastProcessedOrder { private get; set; }

        //private float lastPriceChangeTimestamp = -1f;

        //

        #endregion

        #region Sampling

        private readonly int TradingMatchSamplesCount = 10; // jeden sample = 10 sekund
        private readonly int TrendMatchSamplesCount = 20; // jeden sample = 10 sekund

        private List<ExchangeMatchInfo> tradingMatchSamples;
        private List<ExchangeMatchInfo> trendMatchSamples;

        public readonly List<ExchangeMatchInfo> Trades = new List<ExchangeMatchInfo>();

        #endregion


        /*
        private long lastTrendChangeTimestamp = -1;
        */

        //

        public PhasedStrategy(TraderCore core, float buyOrderAmount, float sellOrderAmount) : base(core)
        {
            Initialize(buyOrderAmount, sellOrderAmount);
        }

        public PhasedStrategy(IOrderProcessor orderProcessor, float buyOrderAmount, float sellOrderAmount) : base(orderProcessor)
        {
            Initialize(buyOrderAmount, sellOrderAmount);
        }

        private void Initialize(float buyOrderAmount, float sellOrderAmount)
        {
            DefaultBuyOrderAmount = buyOrderAmount;
            DefaultSellOrderAmount = sellOrderAmount;

            Console.WriteLine($"Starting with {DefaultBuyOrderAmount} buy and {DefaultSellOrderAmount} sell orders...");

            tradingMatchSamples = new List<ExchangeMatchInfo>(TradingMatchSamplesCount);
            trendMatchSamples = new List<ExchangeMatchInfo>();

            SetStrategy(SubStrategy.Short);

            Task.Run(InputReceiver);
        }


        public override void RegisterGateways(Exchange exchangeType)
        {
            switch (exchangeType)
            {
                case Exchange.GDAX:
                    core.RealtimeGateways.Add(new Gateways.GDAX.GDAXRealtimeGateway(core, core.CurrencyPair));
                    core.RealtimeGateways.Add(new Gateways.GDAX.GDAXRealtimeGateway(core, "BTC-USD"));
                    core.AccountGateway = new Gateways.GDAX.GDAXAccountGateway();
                    core.TradeGateway = new Gateways.GDAX.GDAXTradeGateway();
                    break;

                default:
                    throw new InvalidOperationException("Exchange value not implemented. Bot cannot continue.");
            }
        }

        public override Task<bool> RestartTrading()
        {
            return Task.FromResult(true);
        }


        #region Trend prediction

        public TradeData PredictTradeState(ExchangeMatch trade)
        {
            //sw.Start();
            var tradeData = PredictTradeState(0, TradingMatchSamplesCount - 2, trade);
            //sw.Stop();

            //Console.WriteLine("PredictTradeState duration {0}ms", sw.ElapsedMilliseconds);

            return tradeData;
        }

        private TradeData PredictTradeState(int startIndex, int currentIndex, ExchangeMatch trade)
        {
            for (int i = 0; i < currentIndex + 1; i++)
            {
                if (i >= tradingMatchSamples.Count - 1)
                    return TradeData.None;

                if (!tradingMatchSamples[i].IsValid)
                    return TradeData.None;
            }

            float usdCurrentPrice = tradingMatchSamples[currentIndex].USDMatch.Price;
            float eurCurrentPrice = tradingMatchSamples[currentIndex].EURMatch.Price;

            float usdStartPrice = tradingMatchSamples[startIndex].USDMatch.Price;
            float eurStartPrice = tradingMatchSamples[startIndex].EURMatch.Price;

            float usdMaxPrice = float.MinValue;
            float usdMinPrice = float.MaxValue;
            float eurMaxPrice = float.MinValue;
            float eurMinPrice = float.MaxValue;

            for (int i = startIndex; i < currentIndex; i++)
            {
                var c = tradingMatchSamples[i];

                if (!c.IsValid)
                    continue;

                if (c.USDMatch.Price > usdMaxPrice)
                    usdMaxPrice = c.USDMatch.Price;

                if (c.USDMatch.Price < usdMinPrice)
                    usdMinPrice = c.USDMatch.Price;

                if (c.EURMatch.Price > eurMaxPrice)
                    eurMaxPrice = c.EURMatch.Price;

                if (c.EURMatch.Price < eurMinPrice)
                    eurMinPrice = c.EURMatch.Price;
            }

            // maximalni cena porovnana k aktualni cene
            float usdCurrentPriceMaxDiff = 1f - (usdMaxPrice / usdCurrentPrice);
            float eurCurrentPriceMaxDiff = 1f - (eurMaxPrice / eurCurrentPrice);

            // maximalni cena porovnana k cene (currentIndex - historyOffset), tj 3 minuty zpet
            float usdStartPriceMaxDiff = 1f - (usdMaxPrice / usdStartPrice);
            float usdStartPriceMinDiff = 1f - (usdMinPrice / usdStartPrice);

            float eurStartPriceMaxDiff = (eurMaxPrice / eurStartPrice) - 1f;
            float eurStartPriceMinDiff = (eurMinPrice / eurStartPrice) - 1f;

            TradeData tradeData = strategy.PredictTradeState(trade,
                                                             eurMaxPrice,
                                                             eurMinPrice,
                                                             usdCurrentPriceMaxDiff,
                                                             eurCurrentPriceMaxDiff,
                                                             eurStartPriceMaxDiff,
                                                             eurStartPriceMinDiff,
                                                             LastProcessedOrder);


            return ValidateTradeState(trade, tradeData);
        }

        #endregion

        #region Trade Validation

        private TradeData ValidateTradeState(ExchangeMatch trade,
                                             TradeData tradeData)
        {
            var result = strategy.ValidateTradeState(trade,
                                               tradeData,
                                               currentOrder,
                                               LastProcessedOrder);

            result.Validate(DefaultBuyOrderAmount, DefaultSellOrderAmount);

            return result;
        }

        #endregion

        #region Trend Analysis

        private Trend lastTrend = Trend.Stagnation;
        private long lastStrategyChangeTimestamp;

        public Trend AnalyzeTrend(ExchangeMatch match, bool useEurValues = true)
        {
            //TODO: analyza trendu stale neni idealni
            if (lastStrategyChangeTimestamp < 0)
                lastStrategyChangeTimestamp = match.Time;

            var d = match.Time - lastStrategyChangeTimestamp;

            //if (d > 10 * 60)
            {
                var trend = AnalyzeTrendSegment(0, TrendMatchSamplesCount - 2, useEurValues);

                if (trend != lastTrend)
                {
                    lastTrend = trend;
                    /*
                    switch (trend)
                    {
                        case Trend.Fall:

                            if (LastProcessedOrder != null &&
                                LastProcessedOrder.OrderType == TradeOrder.Type.Buy)
                            {
                                SetStrategy(SubStrategy.Short);
                                lastStrategyChangeTimestamp = match.Time;
                            }

                            break;

                        case Trend.Rise:

                            if (LastProcessedOrder != null &&
                                LastProcessedOrder.OrderType == TradeOrder.Type.Sell)
                            {
                                SetStrategy(SubStrategy.Long);
                                lastStrategyChangeTimestamp = match.Time;
                            }

                            break;
                    }*/
                }
            }

            return lastTrend;
        }

        private Trend AnalyzeTrendSegment(int startIndex, int currentIndex, bool useEurValues)
        {
            for (int i = 0; i < currentIndex + 1; i++)
            {
                if (i >= trendMatchSamples.Count - 1 || !trendMatchSamples[i].IsValid)
                    return Trend.Stagnation;
            }

            float usdCurrentPrice = trendMatchSamples[currentIndex].USDMatch.Price;
            float eurCurrentPrice = trendMatchSamples[currentIndex].EURMatch.Price;

            float usdMaxPrice = float.MinValue;
            float eurMaxPrice = float.MinValue;
            float eurMinPrice = float.MaxValue;

            for (int i = startIndex; i < currentIndex; i++)
            {
                var c = trendMatchSamples[i];

                if (!c.IsValid)
                    continue;

                if (c.USDMatch.Price > usdMaxPrice)
                    usdMaxPrice = c.USDMatch.Price;

                if (c.EURMatch.Price > eurMaxPrice)
                    eurMaxPrice = c.EURMatch.Price;

                if (c.EURMatch.Price < eurMinPrice)
                    eurMinPrice = c.EURMatch.Price;
            }

            // maximalni cena porovnana k aktualni cene
            float usdCurrentPriceMaxDiff = 1f - (usdMaxPrice / usdCurrentPrice);
            float eurCurrentPriceMaxDiff = 1f - (eurMaxPrice / eurCurrentPrice);

            float pd = eurMinPrice / eurMaxPrice;

            //Console.WriteLine(pd);

            //if (pd > 0.99f && pd <= 1.0f)
            //    return Trend.Stagnation;

            float sampler = useEurValues ? eurCurrentPriceMaxDiff : usdCurrentPriceMaxDiff;

            if (sampler < -0.002f)
                return Trend.Fall;
            else if (sampler > -0.0001f)
                return Trend.Rise;

            return Trend.Stagnation;
        }


        #endregion

        #region Events

        private bool gatheredEnoughSamples = false;

        public void ProcessRealtimeMatchTrade(ExchangeMatch match)
        {
            int timeIndex = (int)Math.Round(((double)match.Time) / 10.0) * 10;

            ProcessTrendMatchTrade(timeIndex, match);

            ExchangeMatchInfo matchInfo = GetTradeSampleMatchInfo(timeIndex);

            if (matchInfo == null)
            {
                matchInfo = new ExchangeMatchInfo(timeIndex);

                CleanUpSamples(tradingMatchSamples, TradingMatchSamplesCount);
                tradingMatchSamples.Add(matchInfo);

                if (!gatheredEnoughSamples)
                {
                    if (tradingMatchSamples.Count >= TradingMatchSamplesCount)
                    {
                        Console.WriteLine($"Gathered enough trading samples ({TradingMatchSamplesCount})");
                        gatheredEnoughSamples = true;
                    }
                    else
                    {
                        Console.WriteLine($"Gathered trading sample ({tradingMatchSamples.Count})");
                    }
                }
            }

            matchInfo.ProcessMatch(match);
        }

        private void ProcessTrendMatchTrade(int timeIndex, ExchangeMatch match)
        {
            ExchangeMatchInfo matchInfo = GetTrendSampleMatchInfo(timeIndex);

            if (matchInfo == null)
            {
                matchInfo = new ExchangeMatchInfo(timeIndex);

                CleanUpSamples(trendMatchSamples, TrendMatchSamplesCount);
                trendMatchSamples.Add(matchInfo);
            }

            matchInfo.ProcessMatch(match);
        }

        public void ProcessMatchTrade(ExchangeMatch match)
        {
            int timeIndex = (int)Math.Round(((double)match.Time) / 10.0) * 10;

            ExchangeMatchInfo matchInfo = GetExchangeMatchInfo(timeIndex);

            if (matchInfo == null)
            {
                matchInfo = new ExchangeMatchInfo(timeIndex);
                Trades.Add(matchInfo);
            }

            matchInfo.ProcessMatch(match);
        }

        private void CleanUpSamples(List<ExchangeMatchInfo> samples, int maxSamples)
        {
            for (int i = 0; i < samples.Count; i++)
            {
                if (!samples[i].IsValid)
                    samples.RemoveAt(i);
            }

            if (samples.Count >= maxSamples)
                samples.RemoveAt(0);
        }

        #endregion

        public void ResetSampler()
        {
            tradingMatchSamples.Clear();
            trendMatchSamples.Clear();
        }

        private void SetStrategy(SubStrategy newStrategySubType)
        {
            if (strategy == null || newStrategySubType != strategy.Type)
            {
                Console.WriteLine("Switching strategy to " + newStrategySubType);

                LastProcessedOrder = null;

                switch (newStrategySubType)
                {
                    case SubStrategy.Long:
                        strategy = new LongSubStrategy(this);
                        break;
                    case SubStrategy.Short:
                        strategy = new ShortSubStrategy(this);
                        break;
                }
            }
        }

        #region ExchangeMatchInfo

        private ExchangeMatchInfo GetExchangeMatchInfo(long time)
        {
            for (int i = 0; i < Trades.Count; i++)
            {
                if (Trades[i].TimeIndex == time)
                    return Trades[i];
            }

            return null;
        }

        private ExchangeMatchInfo GetTradeSampleMatchInfo(long time)
        {
            for (int i = 0; i < tradingMatchSamples.Count; i++)
            {
                if (tradingMatchSamples[i].TimeIndex == time)
                    return tradingMatchSamples[i];
            }

            return null;
        }

        private ExchangeMatchInfo GetTrendSampleMatchInfo(long time)
        {
            for (int i = 0; i < trendMatchSamples.Count; i++)
            {
                if (trendMatchSamples[i].TimeIndex == time)
                    return trendMatchSamples[i];
            }

            return null;
        }

        #endregion

        #region Trading Events

        public override async Task<bool> OnCurrencyBought(DateTime timestamp, TradeOrder order)
        {
            await base.OnCurrencyBought(timestamp, order);

            if (order == null)
            {
                Console.WriteLine("OnCurrencyBought, but no order returned, a problem smells here...");
                return true;
            }

            Console.WriteLine("Bought " + order.Amount + ", with price " + order.Price);

            await strategy.OnCurrencyBought(order, timestamp, LastProcessedOrder);

            LastProcessedOrder = order;

            currentOrder = null;

            return true;
        }

        public override async Task<bool> OnCurrencySold(DateTime timestamp, TradeOrder order)
        {
            await base.OnCurrencySold(timestamp, order);

            if (order == null)
            {
                Console.WriteLine("OnCurrencySold, but no order returned, a problem smells here...");
                return true;
            }

            if (LastProcessedOrder != null)
            {
                await strategy.OnCurrencySold(order, timestamp, LastProcessedOrder);

                LastProcessedOrder = order;
            }
            else
            {
                Console.WriteLine("OnCurrencySold - LastBuyOrder == null, something fishy here..");
            }

            currentOrder = null;

            return true;
        }

        public override async Task<bool> OnMatchOccured(ExchangeMatch match)
        {
            ProcessRealtimeMatchTrade(match);

            if (match.CurrencyPair == "BTC-EUR")
                await ProcessEURMatch(match);

            return true;
        }

        private async Task ProcessEURMatch(ExchangeMatch match)
        {
            //await CheckAndCancelOldOrders(match);

            TradeData tradeData = PredictTradeState(match);

            Console.Title = "Last trade: " + match.Price + " - " + tradeData;

            switch (tradeData.State)
            {
                case TradeState.Buy:

                    await BuyCurrency(tradeData);

                    break;

                case TradeState.Sell:

                    await SellCurrency(tradeData);

                    break;
            }
        }

        /*
        private async Task CheckAndCancelOldOrders(ExchangeMatch match)
        {
            if (currentOrder == null)
                return;
            
            var priceDifference = Math.Abs(match.Price - currentOrder.Price);
            var matchTime = match.Time;

            if (priceDifference >= 0.001f &&
               (lastPriceChangeTimestamp < 0 || (matchTime - lastPriceChangeTimestamp) >= 5f))
            {
                await CancelCurrentOrder();

                lastPriceChangeTimestamp = matchTime;
            }
        }
        */

        private async Task CancelCurrentOrder()
        {
            if (currentOrder == null)
            {
                Console.WriteLine("Failed to cancel orders. There is no current order...");
                return;
            }

            bool success = await orderProcessor.CancelOrder(currentOrder.OrderId);

            if (success)
                currentOrder = null;
        }

        #endregion

        #region Order processing

        public async Task BuyCurrency(TradeData tradeData)
        {
            await CancelCurrentOrder();

            TradeOrder order = await orderProcessor.BuyCurrency(currencyPair,
                                                            0,
                                                            tradeData.Price,
                                                            tradeData.Amount,
                                                            tradeData.Time);

            if (order != null)
                Console.WriteLine($"Buy order {tradeData.Price} / {tradeData.Amount} set...");
            else
            {
                var currentMatch = core?.LastMatch;

                float correctedPrice = tradeData.Price - 0.01f;

                if (currentMatch != null)
                    correctedPrice = currentMatch.Price - 0.01f;

                correctedPrice = (float)Math.Round(correctedPrice, 2);

                Console.WriteLine($"Buy order failed, trying with {correctedPrice}");
                order = await orderProcessor.BuyCurrency(currencyPair,
                                                                0,
                                                                correctedPrice,
                                                                tradeData.Amount,
                                                                tradeData.Time);
            }

            if (order != null)
                currentOrder = order;
        }

        public async Task SellCurrency(TradeData tradeData)
        {
            await CancelCurrentOrder();

            TradeOrder order = await orderProcessor.SellCurrency(currencyPair,
                                                             0,
                                                             tradeData.Price,
                                                             tradeData.Amount,
                                                             LastProcessedOrder,
                                                             tradeData.Time);

            if (order != null)
                Console.WriteLine($"Sell order {tradeData.Price} / {tradeData.Amount} set...");
            else
            {
                Console.WriteLine($"Sell order failed, waiting for another signal");
            }

            if (order != null)
                currentOrder = order;
        }

        #endregion

        #region KW input

        private const string shortCommand = "short";
        private const string longCommand = "long";
        private const string buyCommand = "buy";
        private const string sellsimCommand = "sellsim";
        private const string sellCommand = "sell";
        private const string cancelCommand = "cancel";
        private const string profitsCommand = "profits";
        private const string exitCommand = "exit";
        private const string helpCommand = "help";

        private Dictionary<string, string> commands = new Dictionary<string, string>
        {
            { shortCommand,         "Switches sub strategy to shorting (bear run)." },
            { longCommand,          "Switches sub strategy to long (bull run)." },
            { buyCommand,           "Forces buy at current price - may incur slippage" },
            { sellsimCommand,       "Simulates sell command and outputs profits or losses." },
            { sellCommand,          "Forces sell out at current price." },
            { cancelCommand,        "Cancel last order." },
            { profitsCommand,       "Prints current profits or losses and other stats." },
            { exitCommand,          "Quits bot." },
            { helpCommand,          "Prints this help." },
        };

        private async Task InputReceiver()
        {
            if (Constants.IsRunningInTestMode)
                return;
            
            while (true)
            {
                var input = Console.ReadLine();

                switch (input)
                {
                    case shortCommand:
                        SetStrategy(SubStrategy.Short);
                        break;

                    case longCommand:
                        SetStrategy(SubStrategy.Long);
                        break;

                    case buyCommand:
                        await ForceBuy();
                        break;

                    case sellsimCommand:
                        SimulateSell();
                        break;

                    case sellCommand:
                        await ForceSell();
                        break;

                    case cancelCommand:
                        await CancelCurrentOrder();
                        Console.WriteLine("Orders cancelled..");
                        break;

                    case profitsCommand:
                        Console.WriteLine(DumpProfits());
                        break;

                    case exitCommand:
                        Environment.Exit(0);
                        break;

                    case helpCommand:
                        foreach (var kvp in commands)
                            Console.WriteLine($"{kvp.Key} - {kvp.Value}");
                        break;

                    default:
                        Console.WriteLine("Unknown command " + input);
                        break;
                }

                await Task.Delay(100);
            }
        }

        #endregion

        private async Task ForceBuy()
        {
            ExchangeMatch match = core?.LastMatch;

            if (match == null)
            {
                Console.WriteLine("Cannot execute buy, no last match info");
                return;
            }

            if (LastProcessedOrder != null && LastProcessedOrder.OrderType == TradeOrder.Type.Buy)
            {
                Console.WriteLine("You already have one processed buy order, cannot process next one. Execute sell command and then buy.");
            }
            else
            {
                var tradeData = new TradeData(match);
                tradeData.Buy();

                Console.WriteLine($"Forcing Buy at ideal price {tradeData.Price} (may incur slippage)");

                tradeData = ValidateTradeState(match, tradeData);

                await BuyCurrency(tradeData);
            }
        }

        private void SimulateSell()
        {
            ExchangeMatch match = core?.LastMatch;

            if (match == null)
            {
                Console.WriteLine("Cannot execute sell, no last match info");
                return;
            }

            if (LastProcessedOrder != null && LastProcessedOrder.OrderType == TradeOrder.Type.Buy)
            {
                var lm = core?.LastMatch;

                if (lm != null)
                {
                    var profit = LastProcessedOrder.Price - lm.Price;

                    Console.WriteLine($"Sell of {LastProcessedOrder.Amount} at price {lm.Price} would result in  {(profit > 0 ?"loss" : "profit")} {profit} EUR ");
                }
                else
                {
                    Console.WriteLine("Current match price is unknown, cannot process sell simulation.");
                }
            }
            else
            {
                Console.WriteLine("No current Buy order in memory. Can't process sell simulation.");
            }
        }

        private async Task ForceSell()
        {
            ExchangeMatch match = core?.LastMatch;

            if(match == null)
            {
                Console.WriteLine("Cannot execute sell, no last match info");
                return;
            }

            if (LastProcessedOrder != null && LastProcessedOrder.OrderType == TradeOrder.Type.Buy)
            {
                var tradeData = new TradeData(match);
                tradeData.Sell();

                tradeData = ValidateTradeState(match, new TradeData(match));

                Console.WriteLine($"Forcing Sell at ideal price {tradeData.Price} (may incur slippage)");

                await SellCurrency(tradeData);
            }
            else
            {
                Console.WriteLine("No current Buy order in memory. Can't process sell signal.");
            }
        }
    }
}
