/***********************************************************************
 * Copyright (c) 2017 Milan Jaitner                                   *
 * Distributed under the MIT software license, see the accompanying    *
 * file COPYING or https://www.opensource.org/licenses/mit-license.php.*
 ***********************************************************************/

using System;

namespace NetTrader.Core.Utils
{
    public static class DateTimeUtils
    {
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ToDateTime(this long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = UnixEpoch.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        public static long Timestamp
        {
            get
            {
                DateTime localDateTime, univDateTime;
                localDateTime = DateTime.Now;
                univDateTime = localDateTime.ToUniversalTime();
                return (long)(univDateTime - UnixEpoch).TotalMilliseconds / 1000;
            }
        }
    }
}