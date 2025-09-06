using AtelierResleriana.Unity;
using System.Text;
using Fmod5Sharp.FmodTypes;
using Region = AtelierResleriana.Game.Region;
using Fmod5Sharp;
using Fmod5Sharp.Util;

namespace AtelierResleriana.Executables.AudioExtractor
{
    public class AudioClipExtractor
    {
        public void Extract(Region region, string outputDirectoryPath)
        {
            string assetBundleDirectoryPath = Path.Combine(outputDirectoryPath, $"UnityFS/{region}");
            string destinationDirectoryPath = Path.Combine(outputDirectoryPath, $"AudioClip/{region}");

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

                if (unityFSFileDirectoryPath.StartsWith("CAB-") && !Path.HasExtension(unityFSFileDirectoryPath))
                {
                    SerializedFileReader serializedFileReader = new SerializedFileReader();
                    using var directoryStream = unityFSFile.GetDirectoryStream(unityFSFileDirectoryInfo);
                    SerializedFile serializedFile = serializedFileReader.Read(directoryStream);

                    foreach (var @object in serializedFile.Objects)
                    {
                        if (@object.ClassId == ClassIds.AudioClip)
                        {
                            SerializedObject serializedObject = serializedFile.GetSerializedObject(@object);
                            ExtractAudioClip(serializedObject, unityFSFile, unityFSFileDirectoryPath, destinationDirectoryPath);
                        }
                    }
                }
            }
        }

        private void ExtractAudioClip(SerializedObject serializedObject, UnityFSFile unityFSFile, string unityFSFileDirectoryPath, string destinationDirectoryPath)
        {
            byte[] nameBytes = (byte[])serializedObject["m_Name"];
            string name = Encoding.UTF8.GetString(nameBytes).TrimEnd('\0');

            AudioCompressionFormat compressionFormat = (AudioCompressionFormat)(int)serializedObject["m_CompressionFormat"];

            byte[] audioData = null;

            // Get audio data (embedded or streamed)
            if (serializedObject.HasValue("m_AudioData") && serializedObject["m_AudioData"] is byte[] embeddedAudioData && embeddedAudioData.Length > 0)
            {
                audioData = embeddedAudioData;
            }
            else if (serializedObject.HasValue("m_Resource") && serializedObject["m_Resource"] is IDictionary<string, object> resourceData)
            {
                ulong offset = (ulong)resourceData["m_Offset"];
                ulong size = (ulong)resourceData["m_Size"];
                byte[] sourceBytes = (byte[])resourceData["m_Source"];
                string source = Encoding.UTF8.GetString(sourceBytes);

                if (source.StartsWith("archive:/"))
                {
                    source = source.Substring(9);
                    string[] segments = source.Split("/");

                    if (segments.Length == 2 && segments[0] == unityFSFileDirectoryPath)
                    {
                        string directoryPath = segments[1];
                        Stream stream = unityFSFile.GetDirectoryStream(directoryPath);

                        audioData = new byte[size];
                        stream.Seek((long)offset, SeekOrigin.Begin);
                        stream.ReadExactly(audioData);
                    }
                }
            }

            if (audioData == null)
            {
                Console.WriteLine($"No audio data found for: {name}");
                return;
            }

            Console.WriteLine($"Found audio clip: {name} (Format: {compressionFormat})");

            // Always save the raw .fsb file first (like you do .data for textures)
            string fsbPath = Path.Combine(destinationDirectoryPath, name + ".fsb");
            File.WriteAllBytes(fsbPath, audioData);

            // Check if it's already a standard format and save directly
            if (IsStandardAudioFormat(audioData, out string standardExtension))
            {
                string standardPath = Path.Combine(destinationDirectoryPath, name + standardExtension);
                File.WriteAllBytes(standardPath, audioData);
                Console.WriteLine($"Extracted standard format audio: {name}{standardExtension}");
                return;
            }

            // Try to parse as FSB and convert supported formats
            if (TryExtractFsbSamples(audioData, name, destinationDirectoryPath))
            {
                Console.WriteLine($"Successfully extracted FSB samples for: {name}");
            }
            else
            {
                Console.WriteLine($"Could not extract samples from FSB for: {name} (format may not be supported)");
            }
        }

        private bool IsStandardAudioFormat(byte[] data, out string extension)
        {
            if (data.Length < 8)
            {
                extension = "";
                return false;
            }

            // Check for OGG
            if (data[0] == 0x4F && data[1] == 0x67 && data[2] == 0x67 && data[3] == 0x53)
            {
                extension = ".ogg";
                return true;
            }

            // Check for RIFF (WAV)
            if (data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46)
            {
                extension = ".wav";
                return true;
            }

            // Check for M4A (ftyp)
            if (data[4] == 0x66 && data[5] == 0x74 && data[6] == 0x79 && data[7] == 0x70)
            {
                extension = ".m4a";
                return true;
            }

            extension = "";
            return false;
        }

        private bool TryExtractFsbSamples(byte[] audioData, string baseName, string destinationDirectoryPath)
        {
            try
            {
                // Try to load the FSB using Fmod5Sharp
                if (!FsbLoader.TryLoadFsbFromByteArray(audioData, out FmodSoundBank bank))
                {
                    return false;
                }

                var samples = bank.Samples;
                if (samples == null || samples.Count == 0)
                {
                    return false;
                }

                // Check if the audio type is supported
                if (!bank.Header.AudioType.IsSupported())
                {
                    Console.WriteLine($"  FSB audio type {bank.Header.AudioType} not supported by Fmod5Sharp");
                    return false;
                }

                string? fileExtension = bank.Header.AudioType.FileExtension();
                if (fileExtension == null)
                {
                    return false;
                }

                // Extract each sample
                for (int i = 0; i < samples.Count; i++)
                {
                    var sample = samples[i];

                    // Try to rebuild as standard file format
                    if (sample.RebuildAsStandardFileFormat(out byte[] convertedData, out string actualExtension))
                    {
                        string fileName = samples.Count > 1 ? $"{baseName}-{i}.{actualExtension}" : $"{baseName}.{actualExtension}";
                        string filePath = Path.Combine(destinationDirectoryPath, fileName);
                        File.WriteAllBytes(filePath, convertedData);
                        Console.WriteLine($"  Extracted sample: {fileName}");
                    }
                    else
                    {
                        Console.WriteLine($"  Failed to convert sample {i} for {baseName}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error processing FSB for {baseName}: {ex.Message}");
                return false;
            }
        }
    }
}