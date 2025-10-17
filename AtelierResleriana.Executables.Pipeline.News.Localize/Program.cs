using AtelierResleriana.News;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Universal.Anthropic.Client.V1;
using Universal.Common;
using UriBuilder = Universal.Common.UriBuilder;

namespace AtelierResleriana.Executables.Pipeline.News.Localize
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const bool Localize = false;

            Console.OutputEncoding = Encoding.UTF8;

            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            string[] locales =
            [
                "en"
            ];

            string localizationDirectoryPath = "../../../../Localization";
            string localizationNewsDirectoryPath = Path.Combine(localizationDirectoryPath, "News");
            string serverProjectDirectoryPath = "../../../../AtelierResleriana.Server";

            AnthropicClient anthropicClient = new AnthropicClient(configuration["Anthropic:ApiKey"]);
            CloudFrontUrlExtractor cloudFrontUrlExtractor = new CloudFrontUrlExtractor();

            foreach (string locale in locales)
            {
                string localizationNewsLocaleDirectoryPath = Path.Combine(localizationNewsDirectoryPath, locale);
                Directory.CreateDirectory(localizationNewsLocaleDirectoryPath);

                using NewsClient newsClient = new NewsClient();

                NewsLists newsLists = await newsClient.GetNewsListsAsync();

                foreach (NewsListItem newsListItem in newsLists.SelectMany(x => x.Value))
                {
                    int id = newsListItem.Id;

                    string outputHtmlPath = Path.Combine(localizationNewsLocaleDirectoryPath, $"{id}.html");

                    if (File.Exists(outputHtmlPath))
                    {
                        continue;
                    }

                    string html = await newsClient.GetNewsItemAsync(id);

                    IDictionary<string, string> cloudFrontUrls = cloudFrontUrlExtractor.Extract(html);

                    foreach ((string baseUrl, string temporaryUrl) in cloudFrontUrls)
                    {
                        UriBuilder uriBuilder = new UriBuilder(baseUrl);
                        html = html.Replace(temporaryUrl, uriBuilder.Path);
                        string outputAssetPath = Path.Combine(serverProjectDirectoryPath, "wwwroot", uriBuilder.Path.TrimStart('/'));
                        if (File.Exists(outputAssetPath))
                        {
                            continue;
                        }
                        byte[] bytes = await newsClient.GetByteArrayAsync(new Uri(temporaryUrl));
                        string outputAssetDirectoryPath = Path.GetDirectoryName(outputAssetPath);
                        Directory.CreateDirectory(outputAssetDirectoryPath);
                        File.WriteAllBytes(outputAssetPath, bytes);
                    }

                    if (!Localize)
                    {
                        continue;
                    }

                    string currentFragment = "<";

                    while (true)
                    {
                        ICollection<Message> messages = new List<Message>()
                        {
                            new Message(Roles.User, new ContentBlock[]
                            {
                                new TextContentBlock(
                                    """
                                    You are a translator for news articles for the game Atelier Resleriana.
                                    You are to translate HTML documents from Japanese to English whilst keeping the HTML structure intact.
                                    You must also respect the localization decisions that the team has chosen.

                                    CHARACTER NAMES:
                                    - マリー → Marie (Female)
                                    - シア → Shia (Female)
                                    - ミュー → Mu (Female)
                                    - ルーウェン → Ruven (Male)
                                    - エリー → Elie (Female)
                                    - リリー → Lilie (Female)
                                    - ユーディー → Judie (Female)
                                    - ヴィオラート → Viorate (Female)
                                    - ヴェイン → Vayne (Male)
                                    - ロゼ → Raze (Male)
                                    - ロロナ → Rorona (Female)
                                    - クーデリア → Cordelia (Female)
                                    - イクセル → Iksel (Male)
                                    - ステルク → Sterk (Male)
                                    - エスティ → Esty (Female)
                                    - トトリ → Totori (Female)
                                    - ミミ → Mimi (Female)
                                    - メルル → Meruru (Female)
                                    - アーシャ → Ayesha (Female)
                                    - ウィルベル → Wilbell (Female)
                                    - レジナ → Regina (Female)
                                    - リンカ → Linca (Female)
                                    - ニオ → Nio (Female)
                                    - マリオン → Marion (Female)
                                    - オディーリア → Odelia (Female)
                                    - キースグリフ → Keithgriff (Male)
                                    - エスカ → Escha (Female)
                                    - ロジー → Logy (Male)
                                    - シャリステラ → Shallistera (Female)
                                    - シャルロッテ → Shallotte (Female)
                                    - ナディ → Nady (Female)
                                    - ソフィー → Sophie (Female)
                                    - プラフタ → Plachta (Female)
                                    - モニカ → Monika (Female)
                                    - コルネリア → Corneria (Female)
                                    - オスカー → Oskar (Male)
                                    - テス → Tess (Female)
                                    - フィリス → Firis (Female)
                                    - イルメリア → Ilmeria (Female)
                                    - リディー → Lydie (Female)
                                    - スール → Suelle (Female)
                                    - ライザ → Ryza (Female)
                                    - クラウディア → Klaudia (Female)
                                    - リラ → Lila (Female)
                                    - レント → Lent (Male)
                                    - タオ → Tao (Male)
                                    - アンペル → Empel (Male)
                                    - パトリツィア → Patricia (Female)
                                    - レスナ → Resna (Female)
                                    - ヴァレリア → Valeria (Female)
                                    - ロマン → Roman (Male)
                                    - ユナ → Juna (Female)
                                    - ハイディ → Heidi (Female)
                                    - フロッケ → Flocke (Female)
                                    - ランツェ → Lanze (Male)
                                    - イザナ → Izana (Female)
                                    - ザスキア → Saskia (Female)
                                    - ヨハナ → Johanna (Female)
                                    - ジェロン → Geron (Male)
                                    - ララ → Lara (Female)
                                    - エレン → Eren (Female)
                                    - ミーケ → Mieke (Female)
                                    - ディオーナ → Diona (Female)
                                    - マクダ → Magda (Female)
                                    - ブラッド → Brad (Male)
                                    - アンチュ → Antje (Female)
                                    - ワルター → Walther (Male)
                                    - クリセルダ → Criselda (Female)
                                    - アルビーナ → Alvina (Female)
                                    - ベップ → Bepp (Female)
                                    - アウグスト → August (Male)
                                    - シトリン → Citrine (Gender Ambiguous)
                                    - ヤンネ → Janne (Male)
                                    - イェルカ → Jelka (Female)
                                    - ユミア → Yumia (Female)
                                    - ルトガー → Rutger (Male)
                                    - ヴィクトル → Viktor (Male)
                                    - アイラ → Isla (Female)
                                    - レイニャ → Lenja (Female)
                                    - ニーナ → Nina (Female)

                                    TECHNICAL TERMS:
                                    ガチャ - Wish: The term "Wish" is to be used instead of "gacha".
                                    引く - Wish: The verb "wish" is to be used instead of "pull".
                                    星導石 - Lodestar Gem(s): The currency used in-game to make Wishes. There is a paid and free variant depending if real currency was used to acquire it.
                                    火属性 - Fire
                                    氷属性 - Ice  
                                    雷属性 - Bolt
                                    風属性 - Air
                                    斬属性 - Slash
                                    打属性 - Strike
                                    突属性 - Stab
                                    アタッカー - Attacker
                                    ブレイカー - Breaker  
                                    ディフェンダー - Defender
                                    サポーター - Supporter

                                    OUTPUT GUIDELINES:
                                    - You are to output HTML, preserving the structure of the HTML exactly, but with the source content localized appropriately.
                                    - Preserving the structure of the HTML means preserving the HTML tags, attributes, their values etc. Only the textual content (inner text/text content) should be localized.
                                    - You are to only output HTML in your response. Do not include any other text before or after the HTML document.
                                    - You may or may not encounter asset URLs in the HTML. If and only if the URL includes CloudFront query parameters, you must discard the scheme and host parts as well as all query parameters, see the example below:
                                        - Example: if you encounter https://asset.resleriana.jp/info/production/news/1051080/6558b72124c3948805c905ecf6b1cd2e.jpg?Expires=1742093878&Signature=TvNyl8Di6DtONuyfAcG7bP3vFuRJBydjFdJf1eSUFNKgCCHZONS1V-sVl3LtSnJs7Z7epFD~Yu5HNL25Tr7TZYVLztEYTSTv1HJ9UdM51vLouJ6JfAXqHWzl6JLkYKl8PiYjmpLic-SgX9~cWWcbJ4QEbpcCQGojAZrOdKwHm2PAMmV9BfMVbHMhyOlslei1zACjgLkA3oU5n1pLzA~ZopbMNbn6IiF6bWxC4opCqcp9rtkwwkiNwSSvOqKbMwSoRVmXo~Io1yDnU9p9H78WZhF~wf5YXLDw0vv2uHL12o5N35aYLDnvoRdbfXs~WyxP2odae81rwi1GlQ6L8tPYEg__&Key-Pair-Id=K1DHEWRMP0R2U5
                                        - You must rewrite it to simply /info/production/news/1051080/6558b72124c3948805c905ecf6b1cd2e.jpg
                                        - Other URLs such as https://asset.resleriana.jp/static/production/images/ja/back-2dd9227234f6a246b76aa67d32e3d967.png with no CloudFront query parameters must be left intact.
                                    """),
                                new DocumentContentBlock(new Source()
                                {
                                    Type = "text",
                                    MediaType = MediaTypes.Text.Plain,
                                    Data = html
                                })
                            }),
                            new Message(Roles.Assistant, currentFragment)
                        };

                        var messageResponse = await anthropicClient.CreateMessageAsync(new MessageRequest
                        {
                            Model = Models.ClaudeSonnet45,
                            MaxTokens = 8192,
                            Messages = messages
                        });

                        string newFragment = string.Join(string.Empty,
                            messageResponse.Content.OfType<TextContentBlock>().Select(x => x.Text));

                        currentFragment += newFragment;

                        if (messageResponse.StopReason != StopReasons.MaxTokens)
                        {
                            break;
                        }
                    }

                    File.WriteAllText(outputHtmlPath, currentFragment);
                    Console.WriteLine($"Processed {outputHtmlPath}");
                }
            }

            Dictionary<string, Dictionary<int, string>> localeTitleLocalizationData = new Dictionary<string, Dictionary<int, string>>();

            foreach (string locale in locales)
            {
                string localizationNewsLocaleDirectoryPath = Path.Combine(localizationNewsDirectoryPath, locale);
                Dictionary<int, string> titleLocalizationData = new Dictionary<int, string>();

                foreach (string file in Directory.EnumerateFiles(localizationNewsLocaleDirectoryPath))
                {
                    int id = int.Parse(Path.GetFileNameWithoutExtension(file));

                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(File.ReadAllText(file));

                    var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//title");
                    titleLocalizationData.Add(id, titleNode.InnerText.Trim());
                }

                localeTitleLocalizationData.Add(locale, titleLocalizationData);
            }

            string localizationNewsMetadataPath = Path.Combine(localizationDirectoryPath, "NewsLocalizationData.json");

            File.WriteAllText(localizationNewsMetadataPath, JsonSerializer.Serialize(localeTitleLocalizationData, new JsonSerializerOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            }));

            string outputNewsMetadataPath = Path.Combine(serverProjectDirectoryPath, "Resources", "NewsLocalizationData.json");

            File.Copy(localizationNewsMetadataPath, outputNewsMetadataPath, true);

            foreach (string locale in locales)
            {
                string localizationNewsLocaleDirectoryPath = Path.Combine(localizationNewsDirectoryPath, locale);
                string outputDirectoryPath = Path.Combine(serverProjectDirectoryPath, "wwwroot", "news", locale);

                Directory.CreateDirectory(outputDirectoryPath);

                foreach (string dirPath in Directory.GetDirectories(localizationNewsLocaleDirectoryPath, "*", SearchOption.AllDirectories))
                {
                    string destSubDir = dirPath.Replace(localizationNewsLocaleDirectoryPath, outputDirectoryPath);
                    Directory.CreateDirectory(destSubDir);
                }

                foreach (string filePath in Directory.GetFiles(localizationNewsLocaleDirectoryPath, "*.*", SearchOption.AllDirectories))
                {
                    string destFilePath = filePath.Replace(localizationNewsLocaleDirectoryPath, outputDirectoryPath);
                    File.Copy(filePath, destFilePath, true);
                }
            }
        }
    }
}
