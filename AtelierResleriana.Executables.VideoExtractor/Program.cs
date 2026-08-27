using AtelierResleriana.Game;

namespace AtelierResleriana.Executables.VideoExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string dataDirectoryPath = "../../../../Data";
            Directory.CreateDirectory(dataDirectoryPath);

            foreach (Region region in new Region[] { Region.Japan })
            {
                Console.WriteLine($"Extracting video for region: {region}");

                VideoClipExtractor videoClipExtractor = new VideoClipExtractor();
                videoClipExtractor.Extract(region, dataDirectoryPath);
            }
        }
    }
}
