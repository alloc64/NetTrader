/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.Pipe.Messages;
using NetTrader.Core.TradingStrategies.Impl.Tiered.Tiers;

namespace NetTrader.Core.TradingStrategies.Impl
{
    public class TieredTradingStrategy : TradingStrategyBase
    {
        public TieredTradingStrategy(TraderCore core) : base(core)
        {
        }

        public override void RegisterGateways(Exchange exchangeType)
        {
            switch (exchangeType)
            {
                case Exchange.Binance:
                    core.RealtimeGateways.Add(new Gateways.Binance.BinanceRealtimeGateway(core, core.CurrencyPair));
                    core.AccountGateway = new Gateways.Binance.BinanceAccountGateway(core.CurrencyPair);
                    core.TradeGateway = new Gateways.Binance.BinanceTradeGateway(core.CurrencyPair);
                    break;

                case Exchange.GDAX:
                    core.RealtimeGateways.Add(new Gateways.GDAX.GDAXRealtimeGateway(core,core. CurrencyPair));
                    core.AccountGateway = new Gateways.GDAX.GDAXAccountGateway();
                    core.TradeGateway = new Gateways.GDAX.GDAXTradeGateway();
                    break;

                default:
                    throw new InvalidOperationException("Exchange value not implemented. Bot cannot continue.");
            }
        }

        public override Task<bool> RestartTrading()
        {
            var basePrice = core.BasePrice;
            core.TradingPipe.SendMessage(new SetTargetBasePriceMessage(basePrice <= 0f ? Settings.Instance.Generic.DefaultTargetBasePrice : basePrice));

            return Task.FromResult(true);
        }

        public override Task<bool> OnMatchOccured(ExchangeMatch match)
        {
            return Task.FromResult(true);
        }

        public override async Task<bool> OnCurrencyBought(DateTime timestamp, TradeOrder order)
        {
            await base.OnCurrencyBought(timestamp, order);

            await Task.Delay(500);

            core.TradingPipe.SendMessage(new SellCurrencyInTierMessage(order.GetTier(core.Tiers), core.CurrencyPair, order));

            return true;
        }

        public override async Task<bool> OnCurrencySold(DateTime timestamp, TradeOrder order)
        {
            await base.OnCurrencySold(timestamp, order);

            var parentBuyOrder = core.Database.GetOrder(order.ParentOrderId);

            if (parentBuyOrder == null)
            {
                Console.WriteLine("parentBuyOrder not found for " + order.ParentOrderId);
                return true;
            }

            order.OrderState = parentBuyOrder.OrderState = TradeOrder.State.Traded;
            core.Database.UpdateOrder(parentBuyOrder);
            core.Database.UpdateOrder(order);

            float priceDiff = order.Price - parentBuyOrder.Price;

            // podarilo se nakoupit a prodat s marginem
            Console.WriteLine("Made profit with: " + priceDiff + ", profit: " + priceDiff * order.Amount);

            await Task.Delay(2000);

            core.TradingPipe.SendMessage(new BuyCurrencyInTierMessage(order.GetTier(core.Tiers), order.CurrencyPair));

            return true;
        }
    }
}
