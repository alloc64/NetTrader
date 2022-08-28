/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetTrader.Core.Gateways.Binance
{
    public class BinanceAccountGateway : BinanceGateway, IAccountGateway
    {
        public BinanceAccountGateway(string currencyPair) : base(currencyPair)
        {
        }

        public async Task<List<AccountInfo>> GetAccountsForCurrencyPair(string currencyLeft, string currencyRight)
        {
            List<AccountInfo> accountInfo = new List<AccountInfo>();

            try
            {
                var split = currencyPair.Split("-"[0]);

                var accountInformation = await client.GetAccountInformation();

                foreach(var b in accountInformation.Balances)
                {
                    bool insert = false;
                    foreach(var c in split)
                    {
                        if(c == b.Asset)
                        {
                            insert = true;
                            break;
                        }
                    }

                    if(insert)
                        accountInfo.Add(new BinanceAccountInfo(b));
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            return accountInfo;
        }
    }
}
