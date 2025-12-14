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
                    Version = 639013504529460317,
                    Uri = new Uri("https://atelierresleriana.blob.core.windows.net/localization/639013504529460317.zip")
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
