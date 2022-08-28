using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace GDAX.NET
{
    public class RequestAuthenticator : IRequestAuthenticator
    {
        private readonly string _apiKey;
        private readonly string _passphrase;
        private readonly string _secret;

        public RequestAuthenticator(string apiKey, string passphrase, string secret)
        {
            if(string.IsNullOrEmpty(apiKey)) throw new ArgumentNullException(nameof(apiKey));
            if(string.IsNullOrEmpty(passphrase)) throw new ArgumentNullException(nameof(passphrase));
            if(string.IsNullOrEmpty(secret)) throw new ArgumentNullException(nameof(secret));

            _apiKey = apiKey;
            _passphrase = passphrase;
            _secret = secret;
        }

		#region Auth token

		public AuthenticationToken GetAuthenticationToken(ApiRequest request)
		{
			return GetAuthenticationToken((long)request.Timestamp, request.HttpMethod.ToString(), request.RequestUrl, request.RequestBody); 
		}

		public AuthenticationToken GetAuthenticationToken(string method, string requestUrl, string requestBody)
		{
			return GetAuthenticationToken(DateTimeUtilities.Timestamp, method, requestUrl, requestBody);
		}

		public AuthenticationToken GetAuthenticationToken(long timestamp, string method, string requestUrl, string requestBody)
		{
			var signature = ComputeSignature(timestamp, method, requestUrl, requestBody);

			return new AuthenticationToken(_apiKey, _passphrase, signature, timestamp);
		}

		#endregion

		#region Signatures

		//

		private string ComputeSignature(double timestamp, string method, string requestUrl, string requestBody)
		{
			byte[] data = Convert.FromBase64String(_secret);
			var prehash = timestamp + method.ToUpper() + requestUrl + requestBody;
			return HashString(prehash, data);
		}

		private string ComputeSignature(ApiRequest request)
		{
			return ComputeSignature(request.Timestamp, request.HttpMethod.ToString(), request.RequestUrl, request.RequestBody);
		}

		#endregion

		private string HashString(string str, byte[] secret)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            using (var hmac = new HMACSHA256(secret))
            {
                byte[] hash = hmac.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
