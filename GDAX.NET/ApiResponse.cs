using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GDAX.NET
{
    public class ApiResponse
    {
        public long Before { get; }

        public long After { get; }

        public Dictionary<string, IEnumerable<string>> Headers { get; }

        public HttpStatusCode StatusCode { get; }

        public string Content { get; }

        public ApiResponse(Dictionary<string, IEnumerable<string>> headers, HttpStatusCode statusCode, string content)
        {
            this.Headers = headers;
            this.StatusCode = statusCode;
            this.Content = content;

            long before = 0, after = 0;

            IEnumerable<string> paginationHeaders = null;
            headers.TryGetValue("cb-before", out paginationHeaders);

            var beforeHeader = paginationHeaders?.First();

            if(beforeHeader != null)
                long.TryParse(beforeHeader, out before);

            headers.TryGetValue("cb-after", out paginationHeaders);

            var afterHeader = paginationHeaders?.First();

            if (afterHeader != null)
                long.TryParse(afterHeader, out after);

            this.Before = before;
            this.After = after;
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Value { get; set; }

        public ApiResponse(Dictionary<string, IEnumerable<string>> headers, HttpStatusCode statusCode, string content) : base(headers, statusCode, content)
        {
        }

        public ApiResponse(Dictionary<string, IEnumerable<string>> headers, HttpStatusCode statusCode, string content, T typedContent) : base(headers, statusCode, content)
        {
            this.Value = typedContent;
        }
    }
}