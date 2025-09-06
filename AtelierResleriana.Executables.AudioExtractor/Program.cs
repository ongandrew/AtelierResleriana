using AtelierResleriana.Game;

namespace AtelierResleriana.Executables.AudioExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string dataDirectoryPath = "../../../../Data";
            Directory.CreateDirectory(dataDirectoryPath);

            foreach (Region region in new Region[] { Region.Japan })
            {
                Console.WriteLine($"Extracting audio for region: {region}");

                AudioClipExtractor audioExtractor = new AudioClipExtractor();
                audioExtractor.Extract(region, dataDirectoryPath);
            }
        }
    }
}
