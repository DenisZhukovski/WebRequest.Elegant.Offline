using System.Net.Http;

namespace WebRequest.Elegant.Offline.Extensions
{
    public static class WebRequestExtensions
    {
        public static IWebRequest WithOffline(
            this IWebRequest webRequest,
            IInternetConnectivity internetConnectivity,
            string offlineStorageDirectory)
        {
            return webRequest.WithOffline(
                new WebCache(offlineStorageDirectory), 
                internetConnectivity
            );
        }

        public static IWebRequest WithOffline(
            this IWebRequest webRequest,
            IWebCache webCache,
            IInternetConnectivity internetConnectivity)
        {
            return new OfflineUnsafeWebRequest(
                new OfflineSafeWebRequest(
                    webRequest,
                    webCache,
                    internetConnectivity),
                webCache,
                internetConnectivity);
        }

        /// <summary>
        /// <para>The convention has been established that the GET and HEAD methods SHOULD NOT have the significance
        /// of taking an action other than retrieval. These methods ought to be considered "safe".</para>
        /// <para>Methods can also have the property of "idempotence" in that (aside from error or expiration issues)
        /// the side-effects of N > 0 identical requests is the same as for a single request. The methods GET,
        /// HEAD, PUT and DELETE share this property. Also, the methods OPTIONS and TRACE SHOULD NOT have side
        /// effects, and so are inherently idempotent.</para>
        /// <see cref="https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html"/>.
        /// </summary>
        public static bool SafeHttpMethod(this IWebRequest webRequest)
        {
            return webRequest.HttpMethod == HttpMethod.Get
                || webRequest.HttpMethod == HttpMethod.Options
                || webRequest.HttpMethod == HttpMethod.Head;
        }
    }
}
