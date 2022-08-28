/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using NetTrader.Core.Gateways;

namespace NetTrader.Core.Candles
{
    public class CandlesProcessor
    {
        public readonly List<Candle> MinuteCandles = new List<Candle>();

        private List<ExchangeMatch> matches = new List<ExchangeMatch>();

        private long lastMatchTimestamp = -1;

        public Candle CreateCandle(TraderCore core, ExchangeMatch match)
        {
            if (lastMatchTimestamp == -1)
                lastMatchTimestamp = match.Time;

            float d = match.Time - lastMatchTimestamp;

            bool clear = false;

            if(d > 60f)
            {
                matches.Clear();
                lastMatchTimestamp = match.Time;
                clear = true;
            }

            matches.Add(match);

            var candle = new Candle(matches, match)
            {
                Clear = clear
            };

            if(clear)
                MinuteCandles.Add(candle);

            return candle;
        }
    }
}
