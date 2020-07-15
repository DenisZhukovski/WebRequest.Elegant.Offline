using System;
using System.Text;
using System.Threading.Tasks;

namespace WebRequest.Elegant.Offline.DataSync
{
    public class DataUploader : IDataUploader
    {
        private readonly IWebRequest _webRequest;
        private readonly IWebCache _webCache;
        private readonly IInternetConnectivity _internetConnectivity;

        public DataUploader(
            IWebRequest webRequest, 
            IWebCache webCache, 
            IInternetConnectivity internetConnectivity)
        {
            _webRequest = webRequest ?? throw new ArgumentNullException(nameof(webRequest));
            _webCache = webCache ?? throw new ArgumentNullException(nameof(webCache));
            _internetConnectivity = internetConnectivity ?? throw new ArgumentNullException(nameof(internetConnectivity));
        }

        public async Task SendLocalChangesAsync()
        {
            if (_internetConnectivity.IsConnected)
            {
                var cachedReqeuestsList = await _webCache.GetPendingRequestsAsync().ConfigureAwait(false);

                foreach (var cachedRequestData in cachedReqeuestsList)
                {
                    await _webRequest
                        .WithPath(cachedRequestData.Url)
                        .WithMethod(cachedRequestData.HttpMethod)
                        .WithBody(new JsonObject(cachedRequestData.Body))
                        .GetResponseAsync().ConfigureAwait(false);

                    await _webCache.RemovePendingRequestAsync(cachedRequestData.RequestId).ConfigureAwait(false);
                }
            }
            else
            {
                throw new InvalidOperationException("The Internet Connection is required to send local changes.");
            }
        }

        public override string ToString()
        {
            var cachedReqeuestsList = _webCache.GetPendingRequestsAsync().Result;
            var ss = new StringBuilder();
            foreach (var cachedRequestData in cachedReqeuestsList)
            {
                ss.AppendLine(_webRequest
                    .WithPath(cachedRequestData.Url)
                    .WithMethod(cachedRequestData.HttpMethod)
                    .WithBody(new JsonObject(cachedRequestData.Body))
                    .ToString());
            }

            return ss.ToString();
        }
    }

    internal class JsonObject : IJsonObject
    {
        private readonly string _jsonString;

        public JsonObject(string jsonString)
        {
            _jsonString = jsonString ?? throw new ArgumentNullException(nameof(jsonString));
        }

        public string ToJson() => _jsonString;
    }
}
