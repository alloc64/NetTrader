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
    public class ShortSubStrategy : IPhasedTradeSubStrategy
    {
        public SubStrategy Type => SubStrategy.Short;

        private PhasedStrategy phasedStrategy;

        private float fixedProfit = 40f;

        private float minPrice = float.MaxValue;

        //

        public ShortSubStrategy(PhasedStrategy phasedStrategy)
        {
            this.phasedStrategy = phasedStrategy;

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
            TradeData predictedState = new TradeData(match);

            if (usdCurrentPriceMaxDiff < -0.001f && eurStartPriceMaxDiff > 0.0045f)
            {
                predictedState.Sell();
            }
            else
            {
                float pd = 1f;

                var amount = phasedStrategy.DefaultBuyOrderAmount;

                if (lastProcessedOrder != null)
                {
                    if (lastProcessedOrder.OrderType == TradeOrder.Type.Sell)
                        pd = lastProcessedOrder.Price / match.Price;
                    else
                        pd = match.Price / lastProcessedOrder.Price;

                }

                if (pd > 1f && usdCurrentPriceMaxDiff > -0.0025f && eurStartPriceMinDiff < -0.005f)
                {
                    amount = pd * phasedStrategy.DefaultBuyOrderAmount;

                    predictedState.Buy(amount: amount);
                }
            }

            /*
            var eurPd = eurMinPrice / eurMaxPrice;

            bool isStagnating = eurPd >= 0.99998f;

            if (lastProcessedOrder != null)
            {
                var amount = phasedStrategy.DefaultBuyOrderAmount;

                float pd = lastProcessedOrder.Price / match.Price;

                switch (lastProcessedOrder.OrderType)
                {
                    case TradeOrder.Type.Buy:
                        
                        // rapidni pad
                        if (pd > 1f && usdCurrentPriceMaxDiff < 0.0005f)
                        {
                            predictedState.Sell();
                            predictedState.Gain = usdCurrentPriceMaxDiff;
                        }

                        break;

                    case TradeOrder.Type.Sell:

                        if (usdCurrentPriceMaxDiff >= -0.00025f)
                        {
                            amount = pd * phasedStrategy.DefaultBuyOrderAmount;

                            if (pd > 1f)
                            {
                                predictedState.Buy(amount:amount);
                                predictedState.Gain = usdCurrentPriceMaxDiff;
                            }
                        }

                        break;
                }
            }
            */

            if (eurCurrentPriceMaxDiff > 0.0015f)
            {
                TmpUpdateFirstBuyOrder(match);
            }

            return predictedState;
        }

        public TradeData ValidateTradeState(ExchangeMatch match,
                                            TradeData predictedState,
                                            TradeOrder currentOrder,
                                            TradeOrder lastProcessedOrder)
        {/*
            if (match.Price < minPrice)
                minPrice = match.Price;

            if (lastProcessedOrder != null)
            {
                switch (predictedState.State)
                {
                    case TradeState.None:
                        
                        break;

                    case TradeState.Buy:
                        
                        if (lastProcessedOrder.OrderType == TradeOrder.Type.Sell)
                        {
                            var priceDivergence = lastProcessedOrder.Price - match.Price;
                            var mpd = minPrice - match.Price;

                            if (priceDivergence < 5f)
                            {
                                predictedState.IgnoreState();
                            }
                        }

                        break;

                    case TradeState.Sell:

                        break;
                }
            }

            if (currentOrder != null && predictedState.State != TradeState.None)
            {
                var pd = Math.Abs(match.Price - currentOrder.Price);

                if (pd <= 0.02)
                {
                    predictedState.IgnoreState();
                    Console.WriteLine($"Ignoring {predictedState.State} signal, because price difference ({pd}) is too small");
                }
            }*/

            return predictedState;
        }

        #region

        public Task OnCurrencyBought(TradeOrder order, DateTime timestamp, TradeOrder lastProcessedOrder)
        {
            if (lastProcessedOrder != null && lastProcessedOrder.Id >= 0)
            {
                var localProfit = (float)Math.Round(order.Amount - lastProcessedOrder.Amount, 5);

                phasedStrategy.CoinProfit += localProfit;

                Console.WriteLine($"Bought {order.Amount}, BTC profit {localProfit.ToString("F6")} - (buy: {order.Price} / {order.Amount} - sell: {lastProcessedOrder.Price} / {lastProcessedOrder.Amount})");
            }
            else
            {
                Console.WriteLine("Bought first order " + order.Price);
            }

            minPrice = float.MaxValue;

            return Task.FromResult(false);
        }

        public Task OnCurrencySold(TradeOrder order, DateTime timestamp, TradeOrder lastProcessedOrder)
        {
            Console.WriteLine($"Sold {order.Amount} - (sell: {order.Price} / {order.Amount} - buy: {lastProcessedOrder.Price} / {lastProcessedOrder.Amount})");

            return Task.FromResult(false);
        }

        #endregion

        //
        private bool hasBTCfunds = false;

        private void TmpUpdateFirstBuyOrder(ExchangeMatch trade)
        {
            if (hasBTCfunds)
                return;

            phasedStrategy.LastProcessedOrder = new TradeOrder()
            {
                Id = -1,
                OrderId = Guid.NewGuid(),
                OrderType = TradeOrder.Type.Buy,
                OrderState = TradeOrder.State.Filled,
                CurrencyPair = "BTC-EUR",
                Price = trade.Price,
                Amount = phasedStrategy.DefaultBuyOrderAmount,
                Timestamp = trade.Time.ToDateTime()
            };

            hasBTCfunds = true;
        }
    }
}
