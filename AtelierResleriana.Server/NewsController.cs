using AtelierResleriana.News;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UriBuilder = Universal.Common.UriBuilder;

namespace AtelierResleriana.Server
{
    [Controller]
    public class NewsController : Controller
    {
        private static Dictionary<string, Dictionary<int, string>> NewsLocalizationData { get; set; } = new Dictionary<string, Dictionary<int, string>>();

        static NewsController()
        {
            NewsLocalizationData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, string>>>(System.IO.File.ReadAllText("Resources/NewsLocalizationData.json"));
        }

        [FromServices]
        public IWebHostEnvironment WebHostEnvironment { get; set; }

        [HttpGet]
        [Route("/news/{id:int}")]
        public async Task<IActionResult> GetNewsByIdAsync([FromRoute] int id, [FromQuery(Name = "language")] string? locale, CancellationToken cancellationToken)
        {
            locale = locale?.ToLower() ?? "en";
            string filePath = Path.Combine(WebHostEnvironment.WebRootPath, "news", locale, $"{id}.html");

            if (!System.IO.File.Exists(filePath))
            {
                return Redirect($"https://info.resleriana.jp/news/{id}?language={locale}");
            }

            string htmlContent = await System.IO.File.ReadAllTextAsync(filePath, cancellationToken);

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            var dateNode = doc.DocumentNode.SelectSingleNode("//p[@class='notice_contents_header_date']");
            var contentNode = doc.DocumentNode.SelectSingleNode("//div[@class='notice_contents_main_detail']");
            var langNode = doc.DocumentNode.SelectSingleNode("//div[@id='language']");

            if (titleNode == null || dateNode == null || contentNode == null)
            {
                return Redirect($"https://info.resleriana.jp/news/{id}?language={locale}");
            }

            ViewData["Title"] = titleNode.InnerText.Trim();
            ViewData["Date"] = dateNode.InnerText.Trim();
            ViewData["Content"] = contentNode.InnerHtml;
            ViewData["Language"] = langNode?.GetAttributeValue("data-language", "ja") ?? "ja";

            return View("/Views/News.cshtml");
        }

        [HttpGet]
        [Route("api/News")]
        public async Task<IActionResult> GetNewsListsAsync([FromQuery(Name = "language")] string? locale, CancellationToken cancellationToken)
        {
            locale ??= "en";
            using NewsClient newsClient = new NewsClient();

            NewsLists newsLists = await newsClient.GetNewsListsAsync(cancellationToken).ConfigureAwait(false);

            foreach (var newsList in newsLists)
            {
                foreach (var newsListItem in newsList.Value)
                {
                    UriBuilder uriBuilder = new UriBuilder(newsListItem.Uri);
                    newsListItem.Uri = new Uri(string.Join("", uriBuilder.Segments), UriKind.Relative);
                    if (NewsLocalizationData.ContainsKey(locale))
                    {
                        if (NewsLocalizationData[locale].ContainsKey(newsListItem.Id))
                        {
                            newsListItem.Title = NewsLocalizationData[locale][newsListItem.Id];
                        }
                    }
                }
            }

            return Ok(newsLists);
        }
    }
}
