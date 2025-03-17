using HtmlAgilityPack;

namespace AtelierResleriana.News
{
    internal class NewsParser
    {
        public NewsLists ParseNewsLists(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            var newsLists = new NewsLists();

            // Find all tab content divs
            var tabContents = document.DocumentNode.SelectNodes("//div[contains(@class, 'js-tab-content')]");
            if (tabContents == null)
            {
                return newsLists;
            }

            foreach (var tabContent in tabContents)
            {
                string category = tabContent.GetAttributeValue("data-tab", "unknown");
                var newsItems = new List<NewsListItem>();

                var articles = tabContent.SelectNodes(".//a[contains(@class, 'notice_list_block')]");
                if (articles != null)
                {
                    foreach (var article in articles)
                    {
                        newsItems.Add(ParseNewsListItem(article));
                    }
                }

                newsLists[category] = newsItems;
            }

            return newsLists;
        }

        private NewsListItem ParseNewsListItem(HtmlNode article)
        {
            string id = article.GetAttributeValue("data-content-id", string.Empty);
            string relativeUrl = article.GetAttributeValue("href", string.Empty);
            Uri uri = new Uri($"https://info.resleriana.jp/news/{relativeUrl}");

            var iconImgNode = article.SelectSingleNode(".//p[@class='notice_list_category']/img");
            string iconUrl = iconImgNode?.GetAttributeValue("src", string.Empty) ?? string.Empty;
            Uri iconUri = new Uri(iconUrl);

            var titleNode = article.SelectSingleNode(".//div[@class='notice_list_content']");
            string title = titleNode?.InnerText.Trim() ?? string.Empty;

            return new NewsListItem
            {
                Id = int.Parse(id),
                Title = title,
                IconUri = iconUri,
                Uri = uri
            };
        }
    }
}