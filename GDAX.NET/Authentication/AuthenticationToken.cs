namespace GDAX.NET
{
    public class AuthenticationToken
    {
        public string Key { get; set; }
        public string Signature { get; set; }
        public long Timestamp { get; set; }
        public string Passphrase { get; set; }

        public AuthenticationToken(string key, string passphrase, string signature, long timestamp)
        {
            Key = key;
            Passphrase = passphrase;
            Signature = signature;
            Timestamp = timestamp;
        }
    }
}