using AtelierResleriana.Game;

namespace AtelierResleriana.Executables.ImageExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string dataDirectoryPath = "../../../../Data";
            Directory.CreateDirectory(dataDirectoryPath);

            foreach (Region region in new Region[] { Region.Japan })
            {
                Texture2DExtractor texture2DExtractor = new Texture2DExtractor();
                texture2DExtractor.Extract(region, dataDirectoryPath);
            }
        }
    }
}
