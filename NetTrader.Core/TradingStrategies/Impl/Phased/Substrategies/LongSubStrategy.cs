/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.Utils;

namespace NetTrader.Core.TradingStrategies.Impl.Phased.Substrategies
{
    public class LongSubStrategy : IPhasedTradeSubStrategy
    {
        public SubStrategy Type => SubStrategy.Long;

        private PhasedStrategy phasedStrategy;

        private float fixedProfit = 70f;

        private float maxPrice = 0f;

        private long timeFromLastSell = 0;

        //

        public LongSubStrategy(PhasedStrategy phaser)
        {
            this.phasedStrategy = phaser;
        }


        public TradeData PredictTradeState(ExchangeMatch match,
                                           float eurMaxPrice,
                                           float eurMinPrice,
                                           float usdCurrentPriceMaxDiff,
                                           float eurCurrentPriceMaxDiff,
                                           float eurStartPriceMaxDiff,
                                           float eurStartPriceMinDiff,
                                           TradeOrder lastProcessedOrder)
        {
            /*

            //TODO: overit zda tato predikce nebude dobre fungovat na longu
            if(usdCurrentPriceMaxDiff < -0.001f && eurStartPriceMaxDiff > 0.0045f)
            {
                predictedState.Sell();
            }
            else if(usdCurrentPriceMaxDiff > -0.0025f && eurStartPriceMinDiff < -0.005f)
            {
                predictedState.Buy();
            }
            
            */

            if (match.Price > maxPrice)
                maxPrice = match.Price;

            TradeData tradeData = new TradeData(match);

            bool alreadySold = lastProcessedOrder != null && lastProcessedOrder.OrderType == TradeOrder.Type.Sell;

            if (lastProcessedOrder == null || lastProcessedOrder.OrderType == TradeOrder.Type.Sell)
            {
                // okej, tohle detekuje spolehlive rusty
                // tohle muzeme pouzivat pro nakup pred tim nez to vyroste
                // jakmile to vyroste, tak to prodame
                if (usdCurrentPriceMaxDiff > (alreadySold ? 0.0012f : 0.00075f) && eurStartPriceMaxDiff <= 0.005f)
                {
                    tradeData.Buy(priceDiversity:0f);
                    tradeData.Gain = usdCurrentPriceMaxDiff;
                }
            }

            return tradeData;
        }

        public TradeData ValidateTradeState(ExchangeMatch match,
                                            TradeData predictedState,
                                            TradeOrder currentOrder,
                                            TradeOrder lastProcessedOrder)
        {
            /*
            float minBuyTimeout = 60f * 15f;
            float ts = match.Time - timeFromLastSell;

            bool shouldIgnoreBuyUntilSomeTimePasses = timeFromLastSell > 0 && ts < minBuyTimeout;

            if(predictedState.State == TradeState.Buy && shouldIgnoreBuyUntilSomeTimePasses)
            {
                if (lastProcessedOrder != null && lastProcessedOrder.OrderType == TradeOrder.Type.Sell)
                {
                    //diky tomuhle se muze stat, ze missnem nektere bull runy, na druhou stranu mirne to zabranuje tomu chytani se na spickach, kdy uz muze dojit k tomu, ze se to neproda a jsme v kurevske ztrate
                    float minPd = 40f;
                    float pd = match.Price - lastProcessedOrder.Price;

                    if (pd <= -minPd)
                    {
                        Console.WriteLine($"Price diff ({pd}) allows buy signal - (sell: {lastProcessedOrder.Price} - buy: {match.Price})");
                    }
                    else
                    {
                        Console.WriteLine($"Ignoring buy signal ({match.Price}), because time ({ts}) between last sell is smaller than ({minBuyTimeout}) and price difference {-minPd} >= ({pd}) <= {minPd}");
                        predictedState.State = TradeState.None;
                    }
                }
            }
            */

            if(lastProcessedOrder != null
               && lastProcessedOrder.OrderType == TradeOrder.Type.Buy
               && maxPrice > 0f)
            {
                var profit = match.Price - lastProcessedOrder.Price;
                var mpd = match.Price - maxPrice;

                //Console.WriteLine(profit + " - " + mpd);

                if(profit >= 30f && mpd <= -20f) 
                {
                    predictedState.Sell(priceDiversity:-1f);
                }
            }

            if(currentOrder != null && predictedState.State != TradeState.None)
            {
                var pd = Math.Abs(match.Price - currentOrder.Price);

                if (pd <= 0.02)
                {
                    Console.WriteLine($"Ignoring {predictedState.State} signal, because price difference ({pd}) is too small");
                    predictedState.IgnoreState();
                }
            }

            return predictedState;
        }

        #region Events

        public async Task OnCurrencyBought(TradeOrder order, DateTime timestamp, TradeOrder lastProcessedOrder)
        {
            var tradeData = new TradeData(order);
            tradeData.Sell(fixedProfit, phasedStrategy.DefaultSellOrderAmount);

            await phasedStrategy.SellCurrency(tradeData);
        }

        public Task OnCurrencySold(TradeOrder order, DateTime timestamp, TradeOrder lastProcessedOrder)
        {
            maxPrice = 0f;
            timeFromLastSell = timestamp.ToUnixTimestamp();

            var lastSellOrderOneBtcProfit = (float)Math.Round(order.Price - lastProcessedOrder.Price, 5);

            var localProfit = lastSellOrderOneBtcProfit * lastProcessedOrder.Amount;
            var localFee = lastProcessedOrder.GetFee(phasedStrategy.Fee);

            var profitWithFee = localProfit - localFee;
            phasedStrategy.FiatProfit += profitWithFee;

            Console.WriteLine($"Sold {order.Amount} for {order.Price}, profit {profitWithFee}, ({localProfit}, fee {localFee})");

            return Task.FromResult(true);
        }

        #endregion
    }
}
