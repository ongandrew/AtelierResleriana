using Universal.Common.Net.Http;

namespace AtelierResleriana.Game
{
    public class MasterDataClient : HttpServiceClient
    {
        public async Task<byte[]> GetMasterDataAsync(Region region, string masterDataVersion, CancellationToken cancellationToken = default)
        {
            if (region == Region.Global)
            {
                throw new InvalidOperationException("Global master data requires a local specifier.");
            }

            if (region != Region.Japan)
            {
                throw new NotSupportedException();
            }

            return await GetByteArrayAsync(new Uri($"https://cdn.resleriana.jp/master_data/{masterDataVersion}"), cancellationToken).ConfigureAwait(false);
        }

        public async Task<byte[]> GetMasterDataAsync(Region region, string locale, string masterDataVersion, CancellationToken cancellationToken = default)
        {
            if (region != Region.Global)
            {
                throw new NotSupportedException();
            }

            return await GetByteArrayAsync(new Uri($"https://asset.resleriana.com/master_data/{locale}/{masterDataVersion}"), cancellationToken).ConfigureAwait(false);
        }

        protected override HttpClient CreateHttpClient()
        {
            return new HttpClient()
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }
    }
}
