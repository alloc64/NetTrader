using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Gateways;
using NetTrader.Core.OrderProcessing;
using NetTrader.Core.TradingStrategies;
using NetTrader.Core.TradingStrategies.Impl.Tiered.Tiers;
using NetTrader.Core.TradingStrategies.Impl.Tiers;
using NetTrader.Core.Utils;

namespace NetTrader.Strategies.Tiered
{
    public class TieredStrategyTest : TradingStrategyBase
    {
        private class BoughtAsset
        {
            public float Price { get; set; }
        }

        private string currencyPair = "BTC-EUR";

        private float TargetBasePrice = 7500f;

        private Tier[] tiers;

        private TieredStrategyBacktestProcessor processor;

        private Dictionary<int, BoughtAsset> boughtAssets = new Dictionary<int, BoughtAsset>();

        //

        public TieredStrategyTest(TieredStrategyBacktestProcessor processor)
        {
            this.processor = processor;

            this.tiers = new TierGenerator().GenerateTiers(TargetBasePrice, processor.FiatFunds);
        }

        #region 

        public override void RegisterGateways(Exchange exchangeType)
        {
        }

        public override Task<bool> RestartTrading()
        {
            processor.CancelAllOrders();

            foreach (var tier in tiers)
            {
                var order = processor.SubmitOrder(currencyPair, tier.GetBuyPrice(TargetBasePrice), tier.CurrencyAmount, TradeOrder.Type.Buy, DateTimeUtils.Timestamp);
                order.TierId = tier.Id;
            }

            return Task.FromResult(true);
        }

        #endregion

        public override Task<bool> OnMatchOccured(ExchangeMatch match)
        {
            return Task.FromResult(true);
        }

        public override async Task<bool> OnCurrencyBought(DateTime timestamp, TradeOrder order)
        {
            await base.OnCurrencyBought(timestamp, order);

            var tier = order.GetTier(tiers);

            var asset = new BoughtAsset()
            {
                Price = order.Price,
            };

            boughtAssets[order.TierId] = asset;

            var sellOrder = processor.SubmitOrder(currencyPair, tier.GetSellPrice(order.Price), tier.CurrencyAmount, TradeOrder.Type.Sell, order.Timestamp.ToUnixTimestamp());

            if (sellOrder != null)
            {
                sellOrder.TierId = order.TierId;
            }
            else
            {
                Console.WriteLine($"Failed to set sell order...");
            }

            return true;
        }

        public override async Task<bool> OnCurrencySold(DateTime timestamp, TradeOrder order)
        {
            await base.OnCurrencySold(timestamp, order);

            BoughtAsset asset = null;

            if (boughtAssets.TryGetValue(order.TierId, out asset))
            {
                var tier = order.GetTier(tiers);

                var buyOrder = processor.SubmitOrder(currencyPair, tier.GetBuyPrice(TargetBasePrice), order.Amount, TradeOrder.Type.Buy, order.Timestamp.ToUnixTimestamp());
                if (buyOrder != null)
                {
                    buyOrder.TierId = order.TierId;
                }
                else
                {
                    Console.WriteLine($"Failed to set buy order...");
                }

                FiatProfit += (order.Price - asset.Price) * order.Amount;

                boughtAssets.Remove(order.TierId);
            }
            else
            {
                
            }

            return true;
        }
    }
}
