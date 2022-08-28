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
    public class Candle
    {
        public Candle(long time, float low, float high, float open, float close, float volume)
        {
            this.Time = time;
            this.Low = low;
            this.High = high;
            this.Open = open;
            this.Close = close;
            this.Volume = volume;    
        }

        public Candle(List<ExchangeMatch> matches, ExchangeMatch match)
        {
            Time = match.Time;
            Low = matches.Min(stick => stick.Price);
            High = matches.Max(stick => stick.Price);
            Open = matches.First().Price;
            Close = matches.Last().Price;
            Volume = matches.Sum(stick => stick.Amount);
        }

        public long Time { get; set; }

        public float Low { get; set; }

        public float High { get; set; }

        public float Open { get; set; }

        public float Close { get; set; }

        public float Volume { get; set; }

        public bool Clear { get; set; }

        //

        public static Candle ReadFromCSVLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return null;

            var arr = line.Split(","[0]);

            if (arr.Length != 6)
                return null;
        
            for (int i = 0; i < arr.Length; i++)
            {
                var val = arr[i];
                arr[i] = val?.Trim();
            }

            long time = 0;
            if (!long.TryParse(arr[0], out time))
                return null;

            float low = 0f;
            if (!float.TryParse(arr[1], out low))
                return null;

            float high = 0f;
            if (!float.TryParse(arr[2], out high))
                return null;

            float open = 0f;
            if (!float.TryParse(arr[3], out open))
                return null;

            float close = 0f;
            if (!float.TryParse(arr[4], out close))
                return null;

            float volume = 0f;
            if (!float.TryParse(arr[5], out volume))
                return null;

            return new Candle(time, low, high, open, close, volume);
        }

        public string ToCSV()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}\n", Time, Low, High, Open, Close, Volume);
        }

        public override string ToString()
        {
            return string.Format("[Candle: Time={0}, Low={1}, High={2}, Open={3}, Close={4}, Volume={5}]", Time, Low, High, Open, Close, Volume);
        }
    }

}
