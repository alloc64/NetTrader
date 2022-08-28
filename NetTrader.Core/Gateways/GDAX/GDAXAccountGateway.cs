/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GDAX.NET;

namespace NetTrader.Core.Gateways.GDAX
{
    public class GDAXAccountGateway : IAccountGateway
    {
        private AccountClient accountClient = new AccountClient("https://api.gdax.com", Settings.Instance.GDAX.RequestAuthenticator);

        public async Task<List<AccountInfo>> GetAccounts()
        {
            List<AccountInfo> accounts = new List<AccountInfo>();

            var exchangeAccounts = await accountClient.ListAccountsAsync();

            foreach(var exchangeAccount in exchangeAccounts.Value)
            {
                accounts.Add(new GDAXAccountInfo(exchangeAccount));
            }

            return accounts;
        }

        public async Task<List<AccountInfo>> GetAccountsForCurrencyPair(string currencyLeft, string currencyRight)
        {
            List<AccountInfo> accountPair = new List<AccountInfo>();

            var accounts = await GetAccounts();

            if(accounts.Count > 1)
            {
                AccountInfo leftAccount = null, rightAccount = null;

                foreach(var acc in accounts)
                {
                    if (acc.Currency.Equals(currencyLeft))
                    {
                        leftAccount = acc;
                    }

                    if (acc.Currency.Equals(currencyRight))
                    {
                        rightAccount = acc;
                    }

                    if (leftAccount != null && rightAccount != null)
                        break;
                }

                accountPair.Add(leftAccount);
                accountPair.Add(rightAccount);
            }

            return accountPair;
        }
    }
}
