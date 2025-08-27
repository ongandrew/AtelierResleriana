using AtelierResleriana.Unity;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using CommunityToolkit.HighPerformance;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using Region = AtelierResleriana.Game.Region;

namespace AtelierResleriana.Executables.ImageExtractor
{
    public class Texture2DExtractor
    {
        public void Extract(Region region, string outputDirectoryPath)
        {
            string assetBundleDirectoryPath = Path.Combine(outputDirectoryPath, $"UnityFS/{region}");
            string destinationDirectoryPath = Path.Combine(outputDirectoryPath, $"Texture2D/{region}");

            if (!Directory.Exists(assetBundleDirectoryPath))
            {
                throw new DirectoryNotFoundException($"Asset bundle directory not found: {assetBundleDirectoryPath}");
            }

            Directory.CreateDirectory(destinationDirectoryPath);

            string[] assetBundleFilePaths = Directory.EnumerateFiles(assetBundleDirectoryPath).ToArray();

            foreach (string assetBundleFilePath in assetBundleFilePaths)
            {
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
                        if (@object.ClassId == ClassIds.Texture2D)
                        {
                            SerializedObject serializedObject = serializedFile.GetSerializedObject(@object);

                            byte[] nameBytes = (byte[])serializedObject["m_Name"];
                            string name = Encoding.UTF8.GetString(nameBytes).TrimEnd('\0');

                            int width = (int)serializedObject["m_Width"];
                            int height = (int)serializedObject["m_Height"];
                            TextureFormat textureFormat = (TextureFormat)(int)serializedObject["m_TextureFormat"];

                            IDictionary<string, object> streamData = (IDictionary<string, object>)serializedObject["m_StreamData"];
                            ulong offset = (ulong)streamData["offset"];
                            uint size = (uint)streamData["size"];
                            byte[] pathBytes = (byte[])streamData["path"];
                            string path = Encoding.UTF8.GetString(pathBytes);

                            string filePath = Path.Combine(destinationDirectoryPath, name);
                            File.WriteAllBytes(filePath, @object.Data);

                            if (path.StartsWith("archive:/"))
                            {
                                path = path.Substring(9);
                                string[] segments = path.Split("/");

                                if (segments.Length == 2)
                                {
                                    string baseUnityFSFileDirectoryPath = segments[0];

                                    if (baseUnityFSFileDirectoryPath == unityFSFileDirectoryPath)
                                    {
                                        string directoryPath = segments[1];
                                        Stream stream = unityFSFile.GetDirectoryStream(directoryPath);
                                        byte[] imageData = new byte[size];
                                        stream.Seek((long)offset, SeekOrigin.Begin);
                                        stream.ReadExactly(imageData);
                                        string imageDataPath = Path.ChangeExtension(Path.Combine(destinationDirectoryPath, name), ".data");
                                        File.WriteAllBytes(imageDataPath, imageData);

                                        if (textureFormat == TextureFormat.Bc7)
                                        {
                                            static byte[] ConvertToByteArray(Memory2D<ColorRgba32> pixels, int width, int height)
                                            {
                                                byte[] result = new byte[width * height * 4]; // RGBA = 4 bytes per pixel
                                                var span = pixels.Span;

                                                for (int y = 0; y < height; y++)
                                                {
                                                    for (int x = 0; x < width; x++)
                                                    {
                                                        var pixel = span[y, x];
                                                        int index = (y * width + x) * 4;

                                                        result[index] = pixel.r;     // Red
                                                        result[index + 1] = pixel.g; // Green  
                                                        result[index + 2] = pixel.b; // Blue
                                                        result[index + 3] = pixel.a; // Alpha
                                                    }
                                                }

                                                return result;
                                            }

                                            unsafe Bitmap ConvertToBitmap(Memory2D<ColorRgba32> pixels, int width, int height)
                                            {
                                                var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                                                try
                                                {
                                                    var span = pixels.Span;
                                                    byte* ptr = (byte*)bitmapData.Scan0;
                                                    int stride = bitmapData.Stride;

                                                    for (int y = 0; y < height; y++)
                                                    {
                                                        for (int x = 0; x < width; x++)
                                                        {
                                                            var pixel = span[y, x];
                                                            // Unity uses 0, 0 for the bottom left by convention.
                                                            int flippedY = height - 1 - y;
                                                            int offset = flippedY * stride + x * 4;

                                                            // System.Drawing uses BGRA format
                                                            ptr[offset] = pixel.b;     // Blue
                                                            ptr[offset + 1] = pixel.g; // Green
                                                            ptr[offset + 2] = pixel.r; // Red
                                                            ptr[offset + 3] = pixel.a; // Alpha
                                                        }
                                                    }
                                                }
                                                finally
                                                {
                                                    bitmap.UnlockBits(bitmapData);
                                                }

                                                return bitmap;
                                            }

                                            BcDecoder decoder = new BcDecoder();

                                            var pixels = decoder.DecodeRaw2D(imageData, width, height, CompressionFormat.Bc7);

                                            using (var bitmap = ConvertToBitmap(pixels, width, height))
                                            {
                                                string pngPath = Path.ChangeExtension(Path.Combine(destinationDirectoryPath, name), ".png");
                                                bitmap.Save(pngPath, ImageFormat.Png);
                                            }
                                        }
                                        else if (textureFormat == TextureFormat.Rgb24)
                                        {
                                            unsafe Bitmap ConvertRGB24ToBitmap(byte[] rgb24Data, int width, int height)
                                            {
                                                var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                                                    ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                                                try
                                                {
                                                    byte* ptr = (byte*)bitmapData.Scan0;
                                                    int stride = bitmapData.Stride;

                                                    for (int y = 0; y < height; y++)
                                                    {
                                                        for (int x = 0; x < width; x++)
                                                        {
                                                            // Read RGB24 data (3 bytes per pixel)
                                                            int srcIndex = (y * width + x) * 3;
                                                            byte r = rgb24Data[srcIndex];
                                                            byte g = rgb24Data[srcIndex + 1];
                                                            byte b = rgb24Data[srcIndex + 2];

                                                            // Write to bitmap (flip Y coordinate like before)
                                                            int flippedY = height - 1 - y;
                                                            int dstOffset = flippedY * stride + x * 4;

                                                            ptr[dstOffset] = b;     // Blue
                                                            ptr[dstOffset + 1] = g; // Green
                                                            ptr[dstOffset + 2] = r; // Red
                                                            ptr[dstOffset + 3] = 255; // Alpha (fully opaque)
                                                        }
                                                    }
                                                }
                                                finally
                                                {
                                                    bitmap.UnlockBits(bitmapData);
                                                }

                                                return bitmap;
                                            }

                                            using (var bitmap = ConvertRGB24ToBitmap(imageData, width, height))
                                            {
                                                string pngPath = Path.ChangeExtension(Path.Combine(destinationDirectoryPath, name), ".png");
                                                bitmap.Save(pngPath, ImageFormat.Png);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}