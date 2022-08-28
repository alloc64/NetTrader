/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Text;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.OrderProcessing;
using NetTrader.Core.TradingStrategies.Impl.Phased;

namespace NetTrader.Core.TradingStrategies
{
    public abstract class TradingStrategyBase : ITradingStrategy
    {
        protected TraderCore core;

        protected IOrderProcessor orderProcessor;

        //

        public float FiatProfit { get; set; }

        public float CoinProfit { get; set; }

        protected float BuyTrades { get; set; }

        protected float SellTrades { get; set; }

        //

        public TradingStrategyBase()
        {
            
        }

        public TradingStrategyBase(IOrderProcessor orderProcessor)
        {
            this.orderProcessor = orderProcessor;
        }

        public TradingStrategyBase(TraderCore core) : this(core.OrderProcessor)
        {
            this.core = core;
        }

        public abstract void RegisterGateways(Exchange exchangeType);

        public abstract Task<bool> RestartTrading();

        public abstract Task<bool> OnMatchOccured(ExchangeMatch match);

        public virtual Task<bool> OnCurrencyBought(DateTime timestamp, TradeOrder order)
        {
            BuyTrades++;
            return Task.FromResult(false);
        }

        public virtual Task<bool> OnCurrencySold(DateTime timestamp, TradeOrder order)
        {
            SellTrades++;
            return Task.FromResult(false);
        }

        public string DumpProfits()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"EUR Profit:   {FiatProfit.ToString("F10")}");
            sb.AppendLine($"BTC Profit:   {CoinProfit.ToString("F10")}");
            sb.AppendLine($"Buy trades:   {BuyTrades}");
            sb.AppendLine($"Sell trades:  {SellTrades}");

            return sb.ToString();
        }
    }
}
