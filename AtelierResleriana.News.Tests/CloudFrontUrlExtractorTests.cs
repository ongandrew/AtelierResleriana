namespace AtelierResleriana.News
{
    [TestClass]
    [TestCategory(nameof(News.CloudFrontUrlExtractor))]
    public sealed class CloudFrontUrlExtractorTests
    {
        private CloudFrontUrlExtractor CloudFrontUrlExtractor;

        [TestInitialize]
        public void Setup()
        {
            CloudFrontUrlExtractor = new CloudFrontUrlExtractor();
        }

        [TestMethod]
        public void Extract_EmptyHtml_ReturnsEmptyDictionary()
        {
            var result = CloudFrontUrlExtractor.Extract("");
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Extract_HtmlWithNoUrls_ReturnsEmptyDictionary()
        {
            var html = "<div>No CloudFront URLs here</div>";
            var result = CloudFrontUrlExtractor.Extract(html);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Extract_SingleCloudFrontUrl_ReturnsSingleEntry()
        {
            var baseUrl = "https://asset.resleriana.jp/info/production/news/12345/image.jpg";
            var queryParams = "?Expires=1741938110&Signature=abc123&Key-Pair-Id=ABCDEF";
            var html = $@"<img src=""{baseUrl}{queryParams}"" />";

            var result = CloudFrontUrlExtractor.Extract(html);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey(baseUrl));
            Assert.AreEqual(baseUrl + queryParams, result[baseUrl]);
        }

        [TestMethod]
        public void Extract_MultipleCloudFrontUrls_ReturnsAllUnique()
        {
            var html = @"
                <img src=""https://asset.resleriana.jp/img1.jpg?Expires=1741938110&Signature=abc&Key-Pair-Id=KEY1"" />
                <img src=""https://asset.resleriana.jp/img2.jpg?Expires=1741938110&Signature=def&Key-Pair-Id=KEY1"" />
                <img src=""https://asset.resleriana.jp/img3.jpg?Expires=1741938110&Signature=ghi&Key-Pair-Id=KEY1"" />";

            var result = CloudFrontUrlExtractor.Extract(html);

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Keys.All(k => k.StartsWith("https://asset.resleriana.jp/")));
            Assert.IsTrue(result.Values.All(v => v.Contains("Expires=") && v.Contains("Signature=") && v.Contains("Key-Pair-Id=")));
        }

        [TestMethod]
        public void Extract_DuplicateBaseUrls_KeepsFirstOccurrence()
        {
            var baseUrl = "https://asset.resleriana.jp/img1.jpg";
            var firstQuery = "?Expires=1741938110&Signature=abc&Key-Pair-Id=KEY1";
            var secondQuery = "?Expires=1741938110&Signature=def&Key-Pair-Id=KEY1";

            var html = $@"
                <img src=""{baseUrl}{firstQuery}"" />
                <img src=""{baseUrl}{secondQuery}"" />";

            var result = CloudFrontUrlExtractor.Extract(html);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(baseUrl + firstQuery, result[baseUrl]);
        }

        [TestMethod]
        public void Extract_NonCloudFrontUrls_IgnoresNonMatchingUrls()
        {
            var html = @"
                <img src=""https://example.com/image.jpg"" />
                <img src=""https://asset.resleriana.jp/img1.jpg?Expires=1741938110&Signature=abc&Key-Pair-Id=KEY1"" />
                <a href=""https://example.com/link"">Link</a>";

            var result = CloudFrontUrlExtractor.Extract(html);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Keys.Single().Contains("asset.resleriana.jp"));
        }

        [TestMethod]
        public void Extract_UrlsInVariousAttributes_FindsAllValidUrls()
        {
            var html = @"
                <img src=""https://asset.resleriana.jp/img1.jpg?Expires=1741938110&Signature=abc&Key-Pair-Id=KEY1"" />
                <div style=""background-image: url('https://asset.resleriana.jp/img2.jpg?Expires=1741938110&Signature=def&Key-Pair-Id=KEY1')""></div>
                <link rel=""preload"" href=""https://asset.resleriana.jp/img3.jpg?Expires=1741938110&Signature=ghi&Key-Pair-Id=KEY1"">";

            var result = CloudFrontUrlExtractor.Extract(html);

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void Extract_MalformedUrls_HandlesGracefully()
        {
            var html = @"
                <img src=""https://asset.resleriana.jp/img1.jpg?Expires="" />
                <img src=""https://asset.resleriana.jp/img2.jpg?Signature=abc"" />
                <img src=""https://asset.resleriana.jp/img3.jpg?Expires=1741938110&Signature=def&Key-Pair-Id=KEY1"" />";

            var result = CloudFrontUrlExtractor.Extract(html);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Values.Single().Contains("Expires=") && result.Values.Single().Contains("Signature=") && result.Values.Single().Contains("Key-Pair-Id="));
        }

        [TestMethod]
        public async Task Extract_RealNewsItem_Works()
        {
            using (NewsClient newsClient = new NewsClient())
            {
                string html = await newsClient.GetNewsItemAsync(1057360);

                var result = CloudFrontUrlExtractor.Extract(html);

                Assert.AreNotEqual(0, result.Count);
            }
        }
    }
}