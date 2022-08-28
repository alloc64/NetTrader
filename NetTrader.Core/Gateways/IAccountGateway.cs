/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetTrader.Core.Gateways
{
    public interface IAccountGateway
    {
        Task<List<AccountInfo>> GetAccountsForCurrencyPair(string currencyLeft, string currencyRight);
    }
}
