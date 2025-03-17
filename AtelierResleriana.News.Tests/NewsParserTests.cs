namespace AtelierResleriana.News
{
    [TestClass]
    [TestCategory(nameof(NewsParser))]
    public sealed class NewsParserTests
    {
        private readonly NewsParser _parser = new();

        [TestMethod]
        public void Parse_ParsesAllCategories()
        {
            // Arrange
            var html = @"
                <div class='js-tab-content' data-tab='all'>
                    <a class='notice_list_block' data-content-id='1057430' href='1057430?language=ja'>
                        <div class='notice_list_block_bg'>
                            <p class='notice_list_category'>
                                <img src='https://asset.resleriana.jp/static/production/images/news/category/info/icon.png'>
                            </p>
                            <div class='notice_list_content'>Test Article 1</div>
                        </div>
                    </a>
                </div>
                <div class='js-tab-content' data-tab='event'>
                    <a class='notice_list_block' data-content-id='1057420' href='1057420?language=ja'>
                        <div class='notice_list_block_bg'>
                            <p class='notice_list_category'>
                                <img src='https://asset.resleriana.jp/static/production/images/news/category/event/icon.png'>
                            </p>
                            <div class='notice_list_content'>Test Article 2</div>
                        </div>
                    </a>
                </div>";

            // Act
            var result = _parser.ParseNewsLists(html);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey("all"));
            Assert.IsTrue(result.ContainsKey("event"));

            // Check "all" category
            var allNews = result["all"];
            Assert.AreEqual(1, allNews.Count);
            var article1 = allNews[0];
            Assert.AreEqual(1057430, article1.Id);
            Assert.AreEqual("Test Article 1", article1.Title);
            Assert.AreEqual("https://asset.resleriana.jp/static/production/images/news/category/info/icon.png", article1.IconUri.ToString());
            Assert.AreEqual("https://info.resleriana.jp/news/1057430?language=ja", article1.Uri.ToString());

            // Check "event" category
            var eventNews = result["event"];
            Assert.AreEqual(1, eventNews.Count);
            var article2 = eventNews[0];
            Assert.AreEqual(1057420, article2.Id);
            Assert.AreEqual("Test Article 2", article2.Title);
            Assert.AreEqual("https://asset.resleriana.jp/static/production/images/news/category/event/icon.png", article2.IconUri.ToString());
            Assert.AreEqual("https://info.resleriana.jp/news/1057420?language=ja", article2.Uri.ToString());
        }

        [TestMethod]
        public void Parse_HandlesEmptyHtml()
        {
            // Arrange
            var html = "<html></html>";

            // Act
            var result = _parser.ParseNewsLists(html);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Parse_HandlesEmptyCategory()
        {
            // Arrange
            var html = @"
                <div class='js-tab-content' data-tab='all'>
                </div>";

            // Act
            var result = _parser.ParseNewsLists(html);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey("all"));
            Assert.AreEqual(0, result["all"].Count);
        }
    }
}