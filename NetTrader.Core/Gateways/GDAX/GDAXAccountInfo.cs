/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using GDAX.NET;

namespace NetTrader.Core.Gateways.GDAX
{
    public class GDAXAccountInfo : AccountInfo
    {
        public GDAXAccountInfo(Account account)
        {
            Id = account.id;
            Currency = account.currency;
            Available = account.available;
            Balance = account.balance;
        }
    }
}
