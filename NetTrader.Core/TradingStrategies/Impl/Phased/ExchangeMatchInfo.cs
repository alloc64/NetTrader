/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using NetTrader.Core.Gateways;

namespace NetTrader.Core.TradingStrategies.Impl.Phased
{
    public class ExchangeMatchInfo
    {
        public long TimeIndex { get; set; }

        public long Time
        {
            get
            {
                if (USDMatch != null)
                    return USDMatch.Time;

                if (EURMatch != null)
                    return EURMatch.Time;

                return 0;
            }
        }

        public ExchangeMatch USDMatch { get; set; }
        public ExchangeMatch EURMatch { get; set; }

        public ExchangeMatchInfo(long time)
        {
            this.TimeIndex = time;
        }

        public bool IsValid { get { return USDMatch != null && EURMatch != null; } }

        public void ProcessMatch(ExchangeMatch match)
        {
            switch (match.CurrencyPair)
            {
                case "BTC-EUR":
                    EURMatch = match;
                    break;

                case "BTC-USD":
                    USDMatch = match;
                    break;
            }
        }
    }
}
