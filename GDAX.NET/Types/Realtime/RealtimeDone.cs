namespace GDAX.NET
{
    public class RealtimeDone : RealtimeMessageWithOrderId
    {
        public float remaining_size { get; set; }

        public string reason { get; set; }

		public bool IsCancelled { get { return reason == "canceled"; } } 
    }
}