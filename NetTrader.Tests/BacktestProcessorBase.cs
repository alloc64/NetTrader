/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.TradingStrategies;
using NetTrader.Core.TradingStrategies.Impl.Phased;
using NetTrader.Core.Utils;

namespace NetTrader.Tests
{
    public abstract class BacktestProcessorBase
    {
        public float FiatFunds { get; private set; } = 0f;

        public float CoinFunds { get; private set; } = 0f;

        public float Fee { get; private set; } = 0f;

        public int BuyTrades { get; set; }

        public int SellTrades { get; set; }

        public string CurrencyPair { get; private set; }

        private int tradeIdx = 0;

        //

        protected ITradingStrategy strategy;

        protected List<TradeOrder> currentOrders = new List<TradeOrder>();

        private Mutex orderLock = new Mutex();

        //

        public BacktestProcessorBase(string currencyPair, float fiatFunds, float coinFunds, float fee)
        {
            this.CurrencyPair = currencyPair;
            this.FiatFunds = fiatFunds;
            this.CoinFunds = coinFunds;
            this.Fee = fee;
        }

        protected void SetStrategy(ITradingStrategy strategy)
        {
            this.strategy = strategy;
        }

        #region Orders

        public TradeOrder SubmitOrder(string currencyPair,
                                      float price,
                                      float amount,
                                      TradeOrder.Type type,
                                      long timestamp)
        {
            switch (type)
            {
                case TradeOrder.Type.Buy:
                    if (price * amount > FiatFunds)
                        return null;
                    break;

                case TradeOrder.Type.Sell:
                    if (amount > CoinFunds)
                        return null;
                    break;
            }

            var order = new TradeOrder()
            {
                Id = tradeIdx++,
                OrderId = Guid.NewGuid(),
                OrderType = type,
                CurrencyPair = currencyPair,
                Price = price,
                Amount = amount,
                Timestamp = timestamp.ToDateTime()
            };

            orderLock.WaitOne();
            currentOrders.Add(order);
            orderLock.ReleaseMutex();

            return order;
        }

        public List<TradeOrder> GetActiveOrders(TradeOrder queryOrder = null)
        {
            if (queryOrder != null)
                return currentOrders.Where(o => o.OrderId == queryOrder.OrderId).ToList();

            return currentOrders;
        }

        public void CancelAllOrders()
        {
            currentOrders.RemoveRange(0, currentOrders.Count);
        }

        public void CancelOrder(Guid orderId)
        {
            orderLock.WaitOne();
            currentOrders.RemoveAll(o => o.OrderId == orderId);
            orderLock.ReleaseMutex();
        }

        #endregion

        public virtual Task Start()
        {
            strategy.RestartTrading();

            return Task.FromResult(true);
        }

        public async Task<List<TradeState>> ProcessTick(ExchangeMatch match)
        {
            if (strategy == null)
                throw new NullReferenceException("strategy must be set by SetStrategy");

            List<TradeState> tradeStates = new List<TradeState>();

            await strategy.OnMatchOccured(match);

            orderLock.WaitOne();

            for (int orderIdx = 0; orderIdx < currentOrders.Count; orderIdx++)
            {
                var order = currentOrders[orderIdx];

                if (order.CurrencyPair != match.CurrencyPair)
                    continue;

                TradeState tradeState = TradeState.None;

                switch (order.OrderType)
                {
                    case TradeOrder.Type.Buy:

                        if (order.Price >= match.Price)
                            tradeState = await ExecuteOrder(match, orderIdx, order);

                        break;

                    case TradeOrder.Type.Sell:

                        if (order.Price <= match.Price)
                            tradeState = await ExecuteOrder(match, orderIdx, order);

                        break;
                }

                if (tradeState != TradeState.None)
                    tradeStates.Add(tradeState);
            }

            orderLock.ReleaseMutex();

            return tradeStates;
        }

        private async Task<TradeState> ExecuteOrder(ExchangeMatch match, int orderIdx, TradeOrder order)
        {
            switch (order.OrderType)
            {
                case TradeOrder.Type.Buy:

                    FiatFunds -= match.Price * order.Amount;
                    CoinFunds += order.Amount * (1f - Fee);

                    await strategy.OnCurrencyBought(match.Time.ToDateTime(), order);

                    currentOrders.RemoveAt(orderIdx);

                    BuyTrades++;

                    return TradeState.Buy;

                case TradeOrder.Type.Sell:

                    FiatFunds += match.Price * order.Amount * (1f - Fee);
                    CoinFunds -= order.Amount;

                    await strategy.OnCurrencySold(match.Time.ToDateTime(), order);

                    currentOrders.RemoveAt(orderIdx);

                    SellTrades++;

                    return TradeState.Sell;
            }

            return TradeState.None;
        }

        public void DumpProfits()
        {
            Console.WriteLine("\n------------------------------------------------\n");
            Console.WriteLine(strategy.DumpProfits());
            Console.WriteLine();
            Console.WriteLine($"Overall BUY trades:   {BuyTrades}");
            Console.WriteLine($"Overall SELL trades:  {SellTrades}");
            Console.WriteLine("\n------------------------------------------------\n");
            Console.WriteLine($"Overall funds {FiatFunds.ToString("F10")} EUR");
            Console.WriteLine($"Overall funds {CoinFunds.ToString("F10")} BTC");
            Console.WriteLine("\n------------------------------------------------\n");
        }
    }
}
