using System;

namespace GDAX.NET
{
    public static class DateTimeUtilities
    {
        public static double ToUnixTimestamp(this DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
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