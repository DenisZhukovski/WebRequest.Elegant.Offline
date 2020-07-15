using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebRequest.Elegant.Offline.Extensions;

namespace WebRequest.Elegant.Offline
{
    public class OfflineSafeWebRequest : IWebRequest
    {
        private readonly IWebRequest _webRequest;
        private readonly IWebCache _webCache;
        private readonly IInternetConnectivity _internetConnectivity;

        public OfflineSafeWebRequest(
            IWebRequest webRequest,
            IWebCache webCache,
            IInternetConnectivity internetConnectivity)
        {
            _webRequest = webRequest;
            _webCache = webCache;
            _internetConnectivity = internetConnectivity;
        }

        public IToken Token => _webRequest.Token;

        public Uri Uri => _webRequest.Uri;

        public HttpMethod HttpMethod => _webRequest.HttpMethod;

        public IJsonObject Body => _webRequest.Body;

        public IWebRequest WithBody(IJsonObject postBody)
        {
            return new OfflineSafeWebRequest(
                _webRequest.WithBody(postBody),
                _webCache,
                _internetConnectivity);
        }

        public async Task<HttpResponseMessage> GetResponseAsync()
        {
            if (_webRequest.SafeHttpMethod())
            {
                if (_internetConnectivity.IsConnected)
                {
                    var response = await _webRequest.GetResponseAsync().ConfigureAwait(false);
                    await _webCache.PutResponseAsync(_webRequest, response).ConfigureAwait(false);
                    return response;
                }

                return await _webCache.GetResponseAsync(_webRequest).ConfigureAwait(false);
            }
            else
            {
                var response = await _webRequest.GetResponseAsync().ConfigureAwait(false);
                return response;
            }
        }

        public IWebRequest WithMethod(HttpMethod method)
        {
            return new OfflineSafeWebRequest(
                _webRequest.WithMethod(method),
                _webCache,
                _internetConnectivity);
        }

        public IWebRequest WithPath(Uri uri)
        {
            return new OfflineSafeWebRequest(
                _webRequest.WithPath(uri),
                _webCache,
                _internetConnectivity);
        }

        public IWebRequest WithQueryParams(Dictionary<string, string> parameters)
        {
            return new OfflineSafeWebRequest(
                _webRequest.WithQueryParams(parameters),
                _webCache,
                _internetConnectivity);
        }

        public override string ToString()
        {
            return _webRequest.ToString();
        }
    }
}
