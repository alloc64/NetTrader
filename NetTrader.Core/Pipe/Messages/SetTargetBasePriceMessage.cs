/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;
using NetTrader.Core.Pipe.Interfaces;
using NetTrader.Core.TradingStrategies.Impl.Tiered.Tiers;
using NetTrader.Core.TradingStrategies.Impl.Tiers;

namespace NetTrader.Core.Pipe.Messages
{
    public class SetTargetBasePriceMessage : CancelOpenedOrdersMessage
    {
        private float price;

        private static TierGenerator tierGenerator = new TierGenerator();

        public SetTargetBasePriceMessage(float price) : base()
        {
            this.price = price;
        }

        public override async Task<bool> ProcessAsync(TraderCore core)
        {
            await base.ProcessAsync(core);

            core.BasePrice = price;

            core.Tiers = tierGenerator.GenerateTiers(price, Settings.Instance.Generic.Budget);

            await PlaceBuyOrders(core);

            await PlaceSellOrders(core);

            return true;
        }

        private async Task PlaceBuyOrders(TraderCore core)
        {
            float fiatFunds = Settings.Instance.Generic.Budget;

            int i = 0;
            foreach (var tier in core.Tiers)
            {
                if (i > 0 && i % 5 == 0)
                    await Task.Delay(1500);
                
                float buyPrice = tier.GetBuyPrice(core.BasePrice) * tier.CurrencyAmount;

                if (fiatFunds - buyPrice >= 0)
                {
                    fiatFunds -= buyPrice;
                    core.ExchangePipe.SendMessage(new BuyCurrencyInTierMessage(tier, core.CurrencyPair));
                }

                i++;
            }
        }

        private async Task PlaceSellOrders(TraderCore core)
        {
            var database = core.Database;

            var filledBuyOrders = database.GetFilledOrders(TradeOrder.Type.Buy);

            int i = 0;
            if (filledBuyOrders != null)
            {
                foreach (var parentBuyOrder in filledBuyOrders)
                {
                    if (i > 0 && i % 5 == 0)
                        await Task.Delay(1500);
                    
                    var tier = parentBuyOrder.GetTier(core.Tiers);

                    if(tier != null)
                    {
                        core.ExchangePipe.SendMessage(new SellCurrencyInTierMessage(tier, core.CurrencyPair, parentBuyOrder));
                    }
                    else
                    {
                        Console.WriteLine("Tier of parentBuyOrder already doesn't exist. Cancelling parent buy order with id: " + parentBuyOrder.Id);
                        parentBuyOrder.OrderState = TradeOrder.State.Cancelled;
                        database.UpdateOrder(parentBuyOrder);
                    }

                    i++;
                }
            }
        }
    }
}
