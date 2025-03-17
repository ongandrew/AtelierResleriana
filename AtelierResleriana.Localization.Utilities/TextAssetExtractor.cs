using AtelierResleriana.Game;
using AtelierResleriana.Text;
using AtelierResleriana.Unity;
using System.Text;

namespace AtelierResleriana.Localization.Utilities
{
    public class TextAssetExtractor
    {
        public bool WriteJson { get; set; }

        public TextAssetExtractor() : this(new Options()) { }
        public TextAssetExtractor(Options options)
        {
            WriteJson = options.WriteJson;
        }

        public void Extract(Region region, string outputDirectoryPath)
        {
            string assetBundleDirectoryPath = Path.Combine(outputDirectoryPath, $"UnityFS/{region}");
            string destinationDirectoryPath = Path.Combine(outputDirectoryPath, $"TextAsset/{region}");
            Directory.CreateDirectory(destinationDirectoryPath);

            string[] assetBundleFilePaths = Directory.EnumerateFiles(assetBundleDirectoryPath).ToArray();
            Parallel.ForEach(assetBundleFilePaths, assetBundleFilePath =>
            {
                UnityFSFileReader unityFSFileReader = new UnityFSFileReader();
                UnityFSFile unityFSFile = unityFSFileReader.Read(File.OpenRead(assetBundleFilePath));

                foreach (var unityFSFileDirectoryInfo in unityFSFile.Metadata.DirectoryInfos)
                {
                    string unityFSFileDirectoryPath = unityFSFileDirectoryInfo.Path;
                    if (unityFSFileDirectoryPath.StartsWith("CAB-") && !Path.HasExtension(unityFSFileDirectoryPath))
                    {
                        SerializedFileReader serializedFileReader = new SerializedFileReader();
                        SerializedFile serializedFile = serializedFileReader.Read(unityFSFile.GetDirectoryStream(unityFSFileDirectoryInfo));

                        foreach (var @object in serializedFile.Objects)
                        {
                            if (@object.ClassId == ClassIds.TextAsset)
                            {
                                SerializedObject serializedObject = serializedFile.GetSerializedObject(@object);
                                PackedTextReader packedTextReader = new PackedTextReader();
                                byte[] bytes = (byte[])serializedObject["m_Script"];
                                if (packedTextReader.IsValid(bytes))
                                {
                                    string name = Encoding.ASCII.GetString((byte[])serializedObject["m_Name"]);
                                    string filePath = Path.Combine(destinationDirectoryPath, name);
                                    File.WriteAllBytes(filePath, @object.Data);
                                    if (WriteJson)
                                    {
                                        PackedText packedText = packedTextReader.Read(bytes);
                                        string jsonFilePath = $"{filePath}.json";
                                        File.WriteAllText(jsonFilePath, packedText.ToJson());
                                    }
                                    Console.WriteLine($"Wrote text asset: {name}");
                                }
                            }
                        }
                    }
                }
            });
        }

        public class Options
        {
            public bool WriteJson { get; set; } = false;
        }
    }
}
