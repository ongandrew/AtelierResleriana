using Universal.Common.Net.Http;

namespace AtelierResleriana.Game
{
    public class AssetClient : HttpServiceClient
    {
        public Uri BaseUri { get; init; }

        public AssetClient(Uri baseUri)
        {
            BaseUri = baseUri;
        }

        protected override HttpClient CreateHttpClient()
        {
            return new HttpClient()
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public async Task<byte[]> GetAssetAsync(string fileAssetsVersion, string platform, string relativePath, CancellationToken cancellationToken = default)
        {
            Uri uri = new Uri(BaseUri, $"asset/{fileAssetsVersion}/{platform}/{relativePath}");
            return await GetByteArrayAsync(
                uri,
                cancellationToken
            ).ConfigureAwait(false);
        }

        public static AssetClient ForTopLevelDomain(string topLevelDomain)
        {
            return new AssetClient(new Uri($"https://asset.resleriana.{topLevelDomain}"));
        }

        public static AssetClient ForGlobal() => ForTopLevelDomain("com");
        public static AssetClient ForJapan() => ForTopLevelDomain("jp");
        public static AssetClient ForRegion(Region region)
        {
            if (region == Region.Japan)
            {
                return ForJapan();
            }
            else if (region == Region.Global)
            {
                return ForGlobal();
            }

            throw new NotSupportedException();
        }

        protected override Task HandleNonSuccessStatusCodeAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            throw new HttpException(httpResponseMessage);
        }
    }
}
