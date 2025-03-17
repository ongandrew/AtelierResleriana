using Newtonsoft.Json;

namespace AtelierResleriana.News
{
    [TestClass]
    [TestCategory(nameof(NewsClient))]
    public sealed class NewsClientTests
    {
        [TestMethod]
        public async Task CanGetNewsLists()
        {
            using (NewsClient newsClient = new NewsClient())
            {
                NewsLists newsLists = await newsClient.GetNewsListsAsync();
                Assert.IsNotNull(newsLists);
                Assert.AreEqual(4, newsLists.Count);
                foreach (var kvp in newsLists)
                {
                    Assert.IsNotNull(kvp.Value);
                    Assert.AreNotEqual(0, kvp.Value.Count());
                }

                Console.WriteLine(JsonConvert.SerializeObject(newsLists));
            }
        }

        [TestMethod]
        public async Task CanGetNewsItem()
        {
            using (NewsClient newsClient = new NewsClient())
            {
                string html = await newsClient.GetNewsItemAsync(1057360);
                Assert.IsNotNull(html);
            }
        }
    }
}
