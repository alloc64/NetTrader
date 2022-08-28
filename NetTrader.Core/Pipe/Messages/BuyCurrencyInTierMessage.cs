/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Threading.Tasks;
using NetTrader.Core.Database;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.Pipe.Messages
{
    public class BuyCurrencyInTierMessage : BuyCurrencyAtPrice
    {
        public Tier Tier { get; private set; }

        public BuyCurrencyInTierMessage(Tier tier, string currencyPair) : base(currencyPair)
        {
            this.Tier = tier;
        }

        public override async Task<bool> ProcessAsync(TraderCore core)
        {
            var existingBuyOrder = core.Database.GetOpenOrderInTier(TradeOrder.Type.Buy, Tier.Id);

            if(existingBuyOrder != null)
            {
                Console.WriteLine("Buy TradeOrder in tier " + Tier.Id + " already exists");
                return true;
            }

            var existingSellOrder = core.Database.GetOpenOrderInTier(TradeOrder.Type.Sell, Tier.Id);

            if (existingSellOrder != null)
            {
                Console.WriteLine("Sell TradeOrder in tier " + Tier.Id + " already exists");
                return true;
            }

            Console.WriteLine("Placing buy order in tier: " + Tier);

            this.price = Tier.GetBuyPrice(core.BasePrice);

            var order = await PlaceBuyOrder(core, Tier.Id, Tier.CurrencyAmount);

            return order != null;
        }
    }
}
