using System.Text.RegularExpressions;

namespace AtelierResleriana.News
{
    public class CloudFrontUrlExtractor
    {
        public IDictionary<string, string> Extract(string html)
        {
            var result = new Dictionary<string, string>();

            var regex = new Regex(@"(https:\/\/[^?""]+)([^""]+Expires=\d+[^""]+)");
            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                var baseUrl = match.Groups[1].Value;
                var fullUrl = baseUrl + match.Groups[2].Value;

                if (!result.ContainsKey(baseUrl))
                {
                    result[baseUrl] = fullUrl;
                }
            }

            return result;
        }
    }
}