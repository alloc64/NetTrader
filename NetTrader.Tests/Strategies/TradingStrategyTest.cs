/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.TradingStrategies;
using NetTrader.Core.Utils;

namespace NetTrader.Strategies
{
    public abstract class TradingStrategyTest
    {
        public float FiatFunds { get; protected set; }

        public float CoinFunds { get; protected set; }

        protected int BuyTrades { get; private set; }

        protected int SellTrades { get; private set; }

        //

        public abstract Task Process();

        protected void DumpProfits()
        {
            Console.WriteLine("BuyTrades:" + BuyTrades);
            Console.WriteLine("SoldTrades:" + SellTrades);

            Console.WriteLine("EUR:" + Math.Round(FiatFunds, 5));
            Console.WriteLine("COIN:" + Math.Round(CoinFunds, 5));
        }

        private int tradeProbability = 100;

        private static int tradeIdx = 0;
        protected TradeOrder SellCurrency(string currencyPair, float finalSellPrice, float currencyAmount, long tradeTimestamp)
        {
            TradeOrder order = null;

            if (CoinFunds - currencyAmount >= 0 && GetRandInt(0, 100) <= tradeProbability)
            {
                order = new TradeOrder()
                {
                    Id = tradeIdx++,
                    OrderId = Guid.NewGuid(),
                    OrderType = TradeOrder.Type.Sell,
                    OrderState = TradeOrder.State.Filled,
                    CurrencyPair = currencyPair,
                    Price = finalSellPrice,
                    Amount = currencyAmount,
                    Timestamp = tradeTimestamp.ToDateTime()
                };

                FiatFunds += order.Price * order.Amount;
                CoinFunds -= order.Amount;

                OnCurrencySold(order);
            }

            return order;
        }

        protected TradeOrder BuyCurrency(string currencyPair, float finalBuyPrice, float currencyAmount, long tradeTimestamp)
        {
            TradeOrder order = null;

            if (FiatFunds - (finalBuyPrice * currencyAmount) >= 0 && GetRandInt(0, 100) <= tradeProbability)
            {
                order = new TradeOrder()
                {
                    Id = tradeIdx++,
                    OrderId = Guid.NewGuid(),
                    OrderType = TradeOrder.Type.Buy,
                    OrderState = TradeOrder.State.Filled,
                    CurrencyPair = currencyPair,
                    Price = finalBuyPrice,
                    Amount = currencyAmount,
                    Timestamp = tradeTimestamp.ToDateTime()
                };

                FiatFunds -= order.Price * order.Amount;
                CoinFunds += order.Amount;

                OnCurrencyBought(order);
            }

            return order;
        }

        #region Trading Strategy Events

        public virtual void OnCurrencyBought(TradeOrder order)
        {
            BuyTrades++;
        }

        public virtual void OnCurrencySold(TradeOrder order)
        {
            SellTrades++;
        }

        #endregion

        //

        private static Random random = new Random();
        private int GetRandInt(int min, int max)
        {
            int ret = random.Next(min, max);

            return ret;
        }
    }
}
