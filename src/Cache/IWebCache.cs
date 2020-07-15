using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebRequest.Elegant.Offline
{
    public interface IWebCache
    {
        Task PutRequestAsync(RequestData requestData);

        Task<List<RequestData>> GetPendingRequestsAsync();

        Task RemovePendingRequestAsync(long requestId);

        Task<HttpResponseMessage> GetResponseAsync(IWebRequest webRequest);

        Task PutResponseAsync(IWebRequest webRequest, HttpResponseMessage response);

        Task ClearAsync();

        void Drop();
    }

    public class RequestData
    {
        public RequestData()
        {
        }

        public RequestData(IWebRequest webRequest)
        {
            RequestId = new WebRequestUniqueId(webRequest).ToNumber();
            Url = webRequest.Uri.ToString();
            Body = webRequest.Body.ToJson();
            HttpMethod = webRequest.HttpMethod;
            TimeInUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public long RequestId { get; set; }

        public string Url { get; set; }

        public string Body { get; set; }

        public HttpMethod HttpMethod { get; set; }

        public long TimeInUnixMilliseconds { get; set; }
    }
}
