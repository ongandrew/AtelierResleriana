using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtelierResleriana.Game
{
    public static class Versions
    {
        public static string? MasterData(Region region)
        {
            throw new NotImplementedException();
        }

        public static string FileAssets(Region region)
        {
            string? contentCatalogFilePath = Paths.ContentCatalogFilePath(region);

            if (contentCatalogFilePath == null)
            {
                throw new NotSupportedException();
            }

            return Path.GetFileName(contentCatalogFilePath)[0..^Paths.ContentCatalogPostfix.Length];
        }
    }
}
