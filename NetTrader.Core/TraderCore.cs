/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading;
using NetTrader.Core.Database;
using NetTrader.Core.Pipe;
using NetTrader.Core.Server;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.Pipe.Messages;
using NetTrader.Core.Candles;
using NetTrader.Core.StopLoss;
using NetTrader.Core.WatchDog.Impl;
using NetTrader.Core.TargetBasePrice;
using NetTrader.Core.TradingStrategies;
using NetTrader.Core.TradingStrategies.Impl;
using System.Collections.Generic;
using NetTrader.Core.OrderProcessing;

namespace NetTrader.Core
{
    public class TraderCore
    {
        public string CurrencyPair { get { return Settings.Instance.Generic.CurrencyPair; } }

        public float BasePrice { get; set; }

        public float TBPRecalculationOffset { get; set; } = Settings.Instance.Generic.TBPRecalculationOffset;

        public float TBPRecalculationPriceOffset { get; set; } = Settings.Instance.Generic.TBPRecalculationPriceOffset;

        public float StopLossThreshold { get; set; } = Settings.Instance.Generic.StopLossThreshold;

        public float StopLossBasePriceOffset { get; set; } = Settings.Instance.Generic.StopLossBasePriceOffset;

        //

        public ExchangeMatch LastMatch { get; set; }

        //TODO: toto bude treba kompletne zabstraktnt
        public Tier[] Tiers { get; set; }

        //

        public readonly ITradingStrategy Strategy;

        public List<IRealtimeGateway> RealtimeGateways { get; set; } = new List<IRealtimeGateway>();

        public IAccountGateway AccountGateway { get; set; }

        public ITradeGateway TradeGateway { get; set; }

        public readonly TradeDatabase Database = new TradeDatabase();

        //

        public readonly StopLossProcessor StopLossProcessor;

        public readonly TargetBasePriceCalculator TargetBasePriceCalculator;

        public readonly CandlesProcessor CandlesProcessor = new CandlesProcessor();

        public readonly TraderServer Server;

        public readonly MessagePipe ExchangePipe;

        public readonly MessagePipe TradingPipe;

        public readonly LeakedOrdersWatchDog OrdersWatchDog;

        public readonly IOrderProcessor OrderProcessor;

        private bool running = false;

        //

        static TraderCore()
        {
            Settings.Load(@"../../etc/settings.json");
    }

        public TraderCore()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            OrderProcessor = new ProductionOrderProcessor(this);

            //TODO: pridat rounding do GDAX libky - 2 setiny na buy 6 setin na sell
            //TODO: add GUI element to switch strategies in realtime
            //Strategy = new TradingStrategies.Impl.Phased.PhasedStrategy(this,
            //                                                            buyOrderAmount: 0.1f,
            //                                                            sellOrderAmount: 0.1f);
            Strategy = new TradingStrategies.Impl.TieredTradingStrategy(this);

            StopLossProcessor = new StopLossProcessor(this);

            TargetBasePriceCalculator = new TargetBasePriceCalculator(this);

            Strategy.RegisterGateways(Settings.Instance.Generic.Exchange);

            ValidateGateways();

            Server = new TraderServer(this);

            ExchangePipe = new MessagePipe(this);

            TradingPipe = new MessagePipe(this);

            OrdersWatchDog = new LeakedOrdersWatchDog(this);
        }

        private void ValidateGateways()
        {
            if (RealtimeGateways.Count < 0)
                throw new InvalidOperationException("No realtime gateway specified");
            
            if (AccountGateway == null)
                throw new InvalidOperationException("No account gateway specified");
            
            if (TradeGateway == null)
                throw new InvalidOperationException("No trade gateway specified");
        }

        //

        public void Start()
        {
            Console.WriteLine("NetTrader.Core started...");

            running = true;

            ExchangePipe.Start();

            TradingPipe.Start();

            //

            foreach(var realtimeGateway in RealtimeGateways)
                realtimeGateway?.Start();

            Server.Start();

            OrdersWatchDog.Start();

            RestartTrading();

            while(running)
            {
                Thread.Sleep(50);
            }
        }

        public void Stop()
        {
            OrdersWatchDog.Stop();

            running = false;
        }

        public void RestartTrading()
        {
            //TODO: zadavani buy/sell orders musi byt asynchronni na 100%

            //TODO: pokud mam buy order, vypsat za kolik se proda, pokud se nakoupi. U sell orders je to jedno

            //TradingPipe.SendMessage(new GetProfitsByDay());

            Strategy.RestartTrading();  
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            TradingPipe.SendMessage(new CancelOpenedOrdersMessage());

            Thread.Sleep(5000);

            Stop();
        }
    }
}
