using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace AtelierResleriana.Unity
{
    [TestClass]
    [TestCategory(nameof(UnityFSFileWriter))]
    public class UnityFSFileWriterTests
    {
        private readonly Stopwatch Stopwatch = new Stopwatch();

        private void LogTiming(string operation, long elapsedMilliseconds, long? streamSize = null)
        {
            string sizeInfo = streamSize.HasValue ? $", Size: {streamSize:N0} bytes" : "";
            Console.WriteLine($"{operation}: {elapsedMilliseconds}ms{sizeInfo}");
        }

        [TestMethod]
        [DataRow("Resources/UnityFS1", UnityFSFileCompression.None)]
        [DataRow("Resources/UnityFS1", UnityFSFileCompression.Lz4)]
        [DataRow("Resources/UnityFS1", UnityFSFileCompression.Lz4hc)]
        [DataRow("Resources/UnityFS2", UnityFSFileCompression.None)]
        [DataRow("Resources/UnityFS2", UnityFSFileCompression.Lz4)]
        [DataRow("Resources/UnityFS2", UnityFSFileCompression.Lz4hc)]
        [DataRow("Resources/UnityFS3", UnityFSFileCompression.None)]
        [DataRow("Resources/UnityFS3", UnityFSFileCompression.Lz4)]
        [DataRow("Resources/UnityFS3", UnityFSFileCompression.Lz4hc)]
        public void DataSurvivesRoundTrips(string filePath, UnityFSFileCompression compression)
        {
            // Read original file
            Stopwatch.Restart();
            using Stream stream = File.OpenRead(filePath);
            LogTiming("File Open", Stopwatch.ElapsedMilliseconds, stream.Length);

            Stopwatch.Restart();
            UnityFSFileReader unityFSFileReader = new UnityFSFileReader();
            UnityFSFile unityFSFile = unityFSFileReader.Read(stream);
            LogTiming("Initial Read", Stopwatch.ElapsedMilliseconds);

            // Process directories
            Stopwatch.Restart();
            IList<UnityFSFileDirectory> directories = new List<UnityFSFileDirectory>();
            foreach (var directoryInfo in unityFSFile.Metadata.DirectoryInfos)
            {
                byte[] bytes = unityFSFile.GetDirectoryBytes(directoryInfo);
                directories.Add(new UnityFSFileDirectory()
                {
                    Path = directoryInfo.Path,
                    Bytes = unityFSFile.GetDirectoryBytes(directoryInfo),
                    Flags = directoryInfo.Flags
                });
            }
            LogTiming("Directory Processing", Stopwatch.ElapsedMilliseconds);

            // Write with compression
            Stopwatch.Restart();
            UnityFSFileWriter unityFSFileWriter = new UnityFSFileWriter(new UnityFSFileWriter.Options()
            {
                Compression = compression
            });
            Stream destinationStream = unityFSFileWriter.Write(directories);
            LogTiming($"Write Operation ({compression})", Stopwatch.ElapsedMilliseconds, destinationStream.Length);

            // Verify data
            Stopwatch.Restart();
            UnityFSFile rereadUnityFSFile = unityFSFileReader.Read(destinationStream);
            LogTiming("Verification Read", Stopwatch.ElapsedMilliseconds);

            Stopwatch.Restart();
            foreach (var directoryInfo in unityFSFile.Metadata.DirectoryInfos)
            {
                byte[] bytes = rereadUnityFSFile.GetDirectoryBytes(directoryInfo);
                byte[] referenceBytes = unityFSFile.GetDirectoryBytes(directoryInfo.Path);
                bool areEqual = bytes.AsSpan().SequenceEqual(referenceBytes);
                Assert.IsTrue(areEqual, $"Content mismatch for directory: {directoryInfo.Path}");
            }
            LogTiming("Comparison", Stopwatch.ElapsedMilliseconds);

            // Log metadata for debugging
            Console.WriteLine("\nOriginal vs Rewritten Metadata:");
            Console.WriteLine(JsonSerializer.Serialize(unityFSFile.Header));
            Console.WriteLine(JsonSerializer.Serialize(unityFSFile.Metadata));
            Console.WriteLine(JsonSerializer.Serialize(rereadUnityFSFile.Header));
            Console.WriteLine(JsonSerializer.Serialize(rereadUnityFSFile.Metadata));
        }
    }
}