namespace GDAX.NET
{
	public class RealtimeOpen : RealtimeMessageWithOrderId
    {
        public float remaining_size { get; set; }
    }
}