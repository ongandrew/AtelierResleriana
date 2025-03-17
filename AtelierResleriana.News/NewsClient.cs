using HtmlAgilityPack;
using Universal.Common.Net.Http;

namespace AtelierResleriana.News
{
    public class NewsClient : HttpServiceClient
    {
        public async Task<NewsLists> GetNewsListsAsync(CancellationToken cancellationToken = default)
        {
            string htmlDocumentString = await GetStringAsync(new Uri("https://info.resleriana.jp/news/"), cancellationToken).ConfigureAwait(false);
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlDocumentString);
            var newsLists = new NewsLists();

            // Find all tab content divs
            var tabContents = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'js-tab-content')]");
            foreach (var tabContent in tabContents)
            {
                // Get the category (tab) name from the data-tab attribute
                string category = tabContent.GetAttributeValue("data-tab", "unknown");
                var newsItems = new List<NewsListItem>();

                // Find all news articles within this tab
                var articles = tabContent.SelectNodes(".//a[contains(@class, 'notice_list_block')]");
                if (articles != null)
                {
                    foreach (var article in articles)
                    {
                        // Extract article ID
                        string id = article.GetAttributeValue("data-content-id", string.Empty);

                        // Get the relative URL
                        string relativeUrl = article.GetAttributeValue("href", string.Empty);

                        // Create absolute URL
                        Uri uri = new Uri($"https://info.resleriana.jp/news/{relativeUrl}");

                        // Find the icon image
                        var iconImgNode = article.SelectSingleNode(".//p[@class='notice_list_category']/img");
                        string iconUrl = iconImgNode?.GetAttributeValue("src", string.Empty) ?? string.Empty;
                        Uri iconUri = new Uri(iconUrl);

                        // Get the title from the content div
                        var titleNode = article.SelectSingleNode(".//div[@class='notice_list_content']");
                        string title = titleNode?.InnerText.Trim() ?? string.Empty;

                        // Create NewsItem
                        var newsItem = new NewsListItem
                        {
                            Id = int.Parse(id),
                            Title = title,
                            IconUri = iconUri,
                            Uri = uri
                        };

                        newsItems.Add(newsItem);
                    }
                }

                newsLists[category] = newsItems;
            }

            return newsLists;
        }

        public async Task<string> GetNewsItemAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetStringAsync(new Uri($"https://info.resleriana.jp/news/{id}?language=ja"), cancellationToken).ConfigureAwait(false);
        }
    }
}