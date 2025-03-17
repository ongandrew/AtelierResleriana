using AtelierResleriana.BestHTTP;
using AtelierResleriana.Game;
using AtelierResleriana.MasterData;
using System.Text.Json;

namespace AtelierResleriana.Executables.Pipeline.MasterData.Extract
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string dataDirectoryPath = "../../../../Data";
            string localizationDirectoryPath = "../../../../Localization";
            string localizationMasterDataDirectoryPath = Path.Combine(localizationDirectoryPath, "MasterData");

            foreach ((Region region, IEnumerable<string> locales) in new Dictionary<Region, IEnumerable<string>>() 
                { 
                    [Region.Global] = new string[] { "en", "zh-CN", "zh-TW" },
                    [Region.Japan] = new string[] { "jp" } 
                })
            {
                foreach (string locale in locales)
                {
                    string persistentDataDirectory = Paths.PersistentDataDirectory(region);
                    string libraryFilePath = Path.Combine(persistentDataDirectory, "Library");

                    bool IsMasterDataLibraryEntry(LibraryEntry x)
                    {
                        var uriBuilder = new Universal.Common.UriBuilder(x.Uri);

                        if (uriBuilder.Segments.Length < 3)
                        {
                            return false;
                        }

                        return uriBuilder.Segments[1] == "master_data/";
                    }

                    string GetMasterDataVersion(LibraryEntry libraryEntry)
                    {
                        var uriBuilder = new Universal.Common.UriBuilder(libraryEntry.Uri);

                        if (region == Region.Global)
                        {
                            return uriBuilder.Segments[3];
                        }

                        return uriBuilder.Segments[2];
                    }

                    LibraryReader libraryReader = new LibraryReader();

                    using Stream libraryStream = File.OpenRead(libraryFilePath);

                    LibraryEntry latestGlobalMasterDataLibraryEntry = libraryReader.Read(libraryStream).ToArray()
                        .Where(IsMasterDataLibraryEntry)
                        .OrderBy(x => x.Received)
                        .Last();

                    string masterDataVersion = GetMasterDataVersion(latestGlobalMasterDataLibraryEntry);

                    string masterDataFilesDirectoryPath = Path.Combine(dataDirectoryPath, "MasterData", region.ToString(), masterDataVersion);
                    if (region == Region.Global)
                    {
                        masterDataFilesDirectoryPath = Path.Combine(masterDataFilesDirectoryPath, locale);
                    }

                    if (Directory.Exists(masterDataFilesDirectoryPath))
                    {
                        Directory.Delete(masterDataFilesDirectoryPath, true);
                    }
                    Directory.CreateDirectory(masterDataFilesDirectoryPath);

                    string localizationJsonFileDirectory = Path.Combine(localizationMasterDataDirectoryPath, region.ToString());
                    if (region == Region.Global)
                    {
                        localizationJsonFileDirectory = Path.Combine(localizationJsonFileDirectory, locale);
                    }
                    Directory.CreateDirectory(localizationJsonFileDirectory);

                    MasterDataReader masterDataReader = new MasterDataReader();

                    using MasterDataClient masterDataClient = new MasterDataClient();

                    byte[]? masterDataFileBytes = null;

                    if (region == Region.Japan)
                    {
                        masterDataFileBytes = await masterDataClient.GetMasterDataAsync(region, masterDataVersion).ConfigureAwait(false);
                    }
                    else if (region == Region.Global)
                    {
                        masterDataFileBytes = await masterDataClient.GetMasterDataAsync(region, locale.Replace("-", "_").ToLower(), masterDataVersion).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }

                    using Stream masterDataStream = new MemoryStream(masterDataFileBytes);

                    IEnumerable<MasterDataFile> masterDataFiles = await masterDataReader.ReadEncryptedAsync(masterDataStream, masterDataVersion).ConfigureAwait(false);

                    foreach (MasterDataFile masterDataFile in masterDataFiles)
                    {
                        string binaryFilePath = Path.Combine(masterDataFilesDirectoryPath, masterDataFile.Name);
                        File.WriteAllBytes(binaryFilePath, masterDataFile.Bytes);

                        MasterDataSerializer masterDataSerializer = new MasterDataSerializer();
                        string jsonFilePath = Path.Combine(localizationJsonFileDirectory, $"{masterDataFile.Name}.json");
                        File.WriteAllText(jsonFilePath, JsonSerializer.Serialize(masterDataSerializer.Deserialize(masterDataFile.Bytes),
                            new JsonSerializerOptions()
                            {
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                WriteIndented = true,
                            }));
                    }
                }
            }
        }
    }
}
