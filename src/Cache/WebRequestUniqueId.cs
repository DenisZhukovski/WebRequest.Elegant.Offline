using System.Text;

namespace WebRequest.Elegant.Offline
{
    public class WebRequestUniqueId
    {
        private readonly IWebRequest _webRequest;

        public WebRequestUniqueId(IWebRequest webRequest)
        {
            _webRequest = webRequest;
        }

        public long ToNumber()
        {
            return StringHash.ELFHash(WebRequestAsStringWithoutToken());
        }

        private string WebRequestAsStringWithoutToken()
        {
            var webRequestAsString = _webRequest.ToString();
            string[] parsedWebRequestString = webRequestAsString.Split("\n");
            var stringBuilder = new StringBuilder();
            foreach (var webRequestStringPart in parsedWebRequestString)
            {
                if (!webRequestStringPart.StartsWith("Token:"))
                {
                    stringBuilder.Append(webRequestStringPart);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
