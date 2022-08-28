/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using BinanceExchange.API.Client;

namespace NetTrader.Core.Gateways.Binance
{
    public class BinanceGateway
    {
        protected BinanceClient client;

        protected string currencyPair;

        public BinanceGateway(string currencyPair)
        {
            this.client = new BinanceClient(Settings.Instance.Binance.RequestAuthenticator);
            this.currencyPair = currencyPair;
        }

        protected string FixCurrencyPair(string currencyPair)
        {
            return currencyPair?.Replace("-", "");
        }
    }
}
