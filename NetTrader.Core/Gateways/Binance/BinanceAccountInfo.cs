/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using BinanceExchange.API.Models.Response;

namespace NetTrader.Core.Gateways.Binance
{
    public class BinanceAccountInfo : AccountInfo
    {
        public BinanceAccountInfo(BalanceResponse balance)
        {
            Currency = balance.Asset;
            Available = (float)balance.Free;
            Balance = (float)(balance.Free + balance.Locked); 
        }
    }
}
