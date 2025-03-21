using Microsoft.AspNetCore.Mvc;

namespace AtelierResleriana.Server
{
    [ApiController]
    public class LocalizationController : ControllerBase
    {
        [HttpGet("api/Localization/Data/Version")]
        public async Task<IActionResult> GetLocalizationDataVersionsAsync()
        {
            return Ok(new LocalizationDataVersion[]
            {
                new LocalizationDataVersion()
                {
                    Version = 638781525946561190,
                    Uri = new Uri("https://atelierresleriana.blob.core.windows.net/localization/638781525946561190.zip")
                },
                new LocalizationDataVersion()
                {
                    Version = 638781475466569613,
                    Uri = new Uri("https://atelierresleriana.blob.core.windows.net/localization/638781475466569613.zip")
                },
                new LocalizationDataVersion()
                {
                    Version = 638778174424104802,
                    Uri = new Uri("https://atelierresleriana.blob.core.windows.net/localization/638778174424104802.zip")
                },
                new LocalizationDataVersion()
                {
                    Version = 638777986101694833,
                    Uri = new Uri("https://atelierresleriana.blob.core.windows.net/localization/638777986101694833.zip")
                },
                new LocalizationDataVersion()
                {
                    Version = 638777800093074041,
                    Uri = new Uri("https://atelierresleriana.blob.core.windows.net/localization/638777800093074041.zip")
                }
            });
        }

        private record class LocalizationDataVersion
        {
            public required long Version { get; set; }
            public required Uri Uri { get; set; }
        }
    }
}
