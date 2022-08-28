using System;
namespace BinanceExchange.API.Utility
{
    public static class LongUtils
    {
        public static Guid ToGuid(this long value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static long ToLong(this Guid value)
        {
            byte[] b = value.ToByteArray();
            long bint = BitConverter.ToInt64(b, 0);
            return bint;
        }
    }
}
