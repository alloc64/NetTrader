/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using NetTrader.Core.Database.Table;

namespace NetTrader.Core.TradingStrategies.Impl.Tiered.Tiers
{
    public static class TierExtension
    {
        public static Tier GetTier(this TradeOrder order, Tier[] tiers)
        {
            if (tiers == null)
                return null;

            if (tiers != null && order.TierId >= 0 && order.TierId < tiers.Length)
                return tiers[order.TierId];

            return null;
        }
    }
}
