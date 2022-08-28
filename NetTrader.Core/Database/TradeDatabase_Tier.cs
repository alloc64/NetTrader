/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using NetTrader.Core.Database.Table;

namespace NetTrader.Core.Database
{
    public partial class TradeDatabase
    {
        public void DeleteAllTiers()
        {
            connection.DeleteAll<Tier>();
        }
    }
}
