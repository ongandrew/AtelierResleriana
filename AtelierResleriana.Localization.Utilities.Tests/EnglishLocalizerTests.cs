using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Universal.Anthropic.Client.V1;
using Universal.GenerativeAI.Anthropic;

namespace AtelierResleriana.Localization.Utilities
{
    [TestClass]
    [TestCategory(nameof(EnglishLocalizer))]
    public sealed class EnglishLocalizerTests
    {
        private static string ApiKey { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("AppSettings.json", true)
                .AddUserSecrets<EnglishLocalizerTests>()
                .Build();

            ApiKey = configuration["Anthropic:ApiKey"];
        }

        [TestMethod]
        public async Task CanLocalizeString()
        {
            EnglishLocalizer englishLocalizer = new EnglishLocalizer(new AnthropicTextGenerator(new AnthropicClient(ApiKey)), new AnthropicTextTransformer(new AnthropicClient(ApiKey)));

            string text = await englishLocalizer.LocalizeAsync("レスナ");
            Assert.AreEqual("Resna", text);
        }
    }
}
