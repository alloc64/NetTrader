/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using NetTrader.Core.Database.Table;
using SQLite.Net;

namespace NetTrader.Core.Database
{
    public partial class TradeDatabase
    {
        private SQLiteConnection connection;

        public TradeDatabase()
        {
            connection = new SQLiteConnection(new SQLite.Net.Platform.Generic.SQLitePlatformGeneric(), Settings.Instance.Generic.DatabasePath);
            connection.CreateTable<TradeOrder>();
        }

    }
}
