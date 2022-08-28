/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.Pipe.Messages
{
    public class SellCurrencyInTierMessage : SellCurrencyAtPrice
    {
        public Tier Tier { get; private set; }

        public SellCurrencyInTierMessage(Tier tier, string currencyPair, TradeOrder parentBuyOrder) : base(currencyPair, parentBuyOrder)
        {
            this.Tier = tier;
        }

        public override async Task<bool> ProcessAsync(TraderCore core)
        {
            var existingSellOrder = core.Database.GetOpenOrderInTier(TradeOrder.Type.Sell, Tier.Id);

            if (existingSellOrder != null)
            {
                Console.WriteLine("Sell TradeOrder in tier " + Tier.Id + " already exists");
                return true;
            }

            Console.WriteLine("Placing sell order in tier: " + Tier);

            float finalPrice = Tier.GetSellPrice(parentBuyOrder.Price);

            if(core.LastMatch != null)
            {
                var lastMatchPrice = core.LastMatch.Price;

                if (finalPrice <= lastMatchPrice)
                {
                    Console.WriteLine("Sell order price " + finalPrice + " was lower than last match price " + lastMatchPrice + "\nSetting order price to " + (lastMatchPrice + 1.0f));
                    finalPrice = lastMatchPrice + 1f;
                }
            }

            var order = await PlaceSellOrder(core, parentBuyOrder.OrderId, Tier.Id, finalPrice, Tier.CurrencyAmount);

            return order != null;
        }
    }
}
