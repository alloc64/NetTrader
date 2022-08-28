namespace GDAX.NET
{
    public class Candle
    {
        public long time { get; set; }

        public float low { get; set; }
        public float high { get; set; }
        public float open { get; set; }
        public float close { get; set; }
        public float volume { get; set; }
    }
}