using AtelierResleriana.Unity;
using System.Text;
using Region = AtelierResleriana.Game.Region;

namespace AtelierResleriana.Executables.VideoExtractor
{
    public class VideoClipExtractor
    {
        public void Extract(Region region, string outputDirectoryPath)
        {
            string assetBundleDirectoryPath = Path.Combine(outputDirectoryPath, $"UnityFS/{region}");
            string destinationDirectoryPath = Path.Combine(outputDirectoryPath, $"VideoClip/{region}");

            if (!Directory.Exists(assetBundleDirectoryPath))
            {
                throw new DirectoryNotFoundException($"Asset bundle directory not found: {assetBundleDirectoryPath}");
            }

            Directory.CreateDirectory(destinationDirectoryPath);

            string[] assetBundleFilePaths = Directory.EnumerateFiles(assetBundleDirectoryPath).ToArray();

            Console.WriteLine($"Processing {assetBundleFilePaths.Length} asset bundles...");

            foreach (string assetBundleFilePath in assetBundleFilePaths)
            {
                Console.WriteLine($"Processing bundle: {Path.GetFileName(assetBundleFilePath)}");
                ProcessBundle(assetBundleFilePath, destinationDirectoryPath);
            }
        }

        private void ProcessBundle(string assetBundleFilePath, string destinationDirectoryPath)
        {
            using var fileStream = File.OpenRead(assetBundleFilePath);
            UnityFSFileReader unityFSFileReader = new UnityFSFileReader();
            UnityFSFile unityFSFile = unityFSFileReader.Read(fileStream);

            foreach (var unityFSFileDirectoryInfo in unityFSFile.Metadata.DirectoryInfos)
            {
                string unityFSFileDirectoryPath = unityFSFileDirectoryInfo.Path;

                if (!unityFSFileDirectoryPath.StartsWith("CAB-") || Path.HasExtension(unityFSFileDirectoryPath))
                {
                    continue;
                }

                SerializedFileReader serializedFileReader = new SerializedFileReader();
                using var directoryStream = unityFSFile.GetDirectoryStream(unityFSFileDirectoryInfo);
                SerializedFile serializedFile = serializedFileReader.Read(directoryStream);

                foreach (var @object in serializedFile.Objects)
                {
                    if (@object.ClassId == ClassIds.VideoClip)
                    {
                        SerializedObject serializedObject = serializedFile.GetSerializedObject(@object);
                        ExtractVideoClip(serializedObject, unityFSFile, unityFSFileDirectoryPath, destinationDirectoryPath);
                    }
                }
            }
        }

        private static void ExtractVideoClip(
            SerializedObject serializedObject,
            UnityFSFile unityFSFile,
            string unityFSFileDirectoryPath,
            string destinationDirectoryPath)
        {
            string name = GetString(serializedObject, "m_Name");
            string originalPath = GetString(serializedObject, "m_OriginalPath");

            if (!serializedObject.HasValue("m_ExternalResources") ||
                serializedObject["m_ExternalResources"] is not IDictionary<string, object> resourceData)
            {
                Console.WriteLine($"No external video resource found for: {name}");
                return;
            }

            ulong offset = GetUnsignedInteger(resourceData, "m_Offset");
            ulong size = GetUnsignedInteger(resourceData, "m_Size");
            string source = GetString(resourceData, "m_Source");

            if (size == 0 || !TryGetResourcePath(source, unityFSFileDirectoryPath, out string resourcePath))
            {
                Console.WriteLine($"Could not resolve video data for: {name} (source: {source})");
                return;
            }

            using Stream resourceStream = unityFSFile.GetDirectoryStream(resourcePath);

            if (offset > (ulong)resourceStream.Length || size > (ulong)resourceStream.Length - offset)
            {
                Console.WriteLine($"Video data is outside the resource bounds for: {name}");
                return;
            }

            if (size > int.MaxValue)
            {
                Console.WriteLine($"Video is too large to extract in memory: {name} ({size} bytes)");
                return;
            }

            byte[] videoData = new byte[(int)size];
            resourceStream.Seek((long)offset, SeekOrigin.Begin);
            resourceStream.ReadExactly(videoData);

            string extension = GetVideoExtension(originalPath, videoData);
            string safeName = GetSafeFileName(name);
            string videoPath = Path.Combine(destinationDirectoryPath, safeName + extension);

            File.WriteAllBytes(videoPath, videoData);
            Console.WriteLine($"Extracted video clip: {Path.GetFileName(videoPath)}");
        }

        private static bool TryGetResourcePath(string source, string unityFSFileDirectoryPath, out string resourcePath)
        {
            const string archivePrefix = "archive:/";
            resourcePath = string.Empty;

            if (!source.StartsWith(archivePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string[] segments = source[archivePrefix.Length..]
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length != 2 || !segments[0].Equals(unityFSFileDirectoryPath, StringComparison.Ordinal))
            {
                return false;
            }

            resourcePath = segments[1];
            return true;
        }

        private static string GetVideoExtension(string originalPath, byte[] data)
        {
            string extension = Path.GetExtension(originalPath);
            if (!string.IsNullOrWhiteSpace(extension) && extension.Length <= 10)
            {
                return extension.ToLowerInvariant();
            }

            if (data.Length >= 12 &&
                data[4] == (byte)'f' && data[5] == (byte)'t' && data[6] == (byte)'y' && data[7] == (byte)'p')
            {
                return ".mp4";
            }

            if (data.Length >= 4 && data[0] == 0x1A && data[1] == 0x45 && data[2] == 0xDF && data[3] == 0xA3)
            {
                return ".webm";
            }

            if (data.Length >= 4 && data[0] == (byte)'O' && data[1] == (byte)'g' && data[2] == (byte)'g' && data[3] == (byte)'S')
            {
                return ".ogv";
            }

            return ".video";
        }

        private static string GetSafeFileName(string name)
        {
            string safeName = string.Join('_', name.Split(Path.GetInvalidFileNameChars()));
            return string.IsNullOrWhiteSpace(safeName) ? "unnamed" : safeName;
        }

        private static string GetString(SerializedObject serializedObject, string key)
        {
            return serializedObject.HasValue(key) ? GetString(serializedObject.Values, key) : string.Empty;
        }

        private static string GetString(IDictionary<string, object> values, string key)
        {
            if (!values.TryGetValue(key, out object? value))
            {
                return string.Empty;
            }

            return value switch
            {
                byte[] bytes => Encoding.UTF8.GetString(bytes).TrimEnd('\0'),
                string text => text.TrimEnd('\0'),
                _ => string.Empty
            };
        }

        private static ulong GetUnsignedInteger(IDictionary<string, object> values, string key)
        {
            return values.TryGetValue(key, out object? value) && value is IConvertible
                ? Convert.ToUInt64(value)
                : 0;
        }
    }
}
