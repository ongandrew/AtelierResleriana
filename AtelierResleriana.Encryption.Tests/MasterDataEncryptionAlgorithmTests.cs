namespace AtelierResleriana.Encryption
{
    [TestClass]
    [TestCategory(nameof(MasterDataEncryptionAlgorithm))]
    public sealed class MasterDataEncryptionAlgorithmTests
    {
        [TestMethod]
        public void IsSymmetric()
        {    
            // Arrange
            var algorithm = MasterDataEncryptionAlgorithm.FromVersion("1.0.0");
            var originalData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // Act
            var encrypted = algorithm.Encrypt(originalData);
            var decrypted = algorithm.Decrypt(encrypted);

            // Assert
            Assert.IsTrue(originalData.SequenceEqual(decrypted));
        }

        [TestMethod]
        [DataRow("Resources/CachedMasterData.1739164044_6p7CNzPRcccY1DVD.bin")]
        [DataRow("Resources/CachedMasterData.1739423910_cUAPYN4t6CGt_5gI.bin")]
        public void ProducesSameResultAsOriginalImplementation(string filePath)
        {
            string version = Path.GetFileName(filePath).Split(".", StringSplitOptions.RemoveEmptyEntries)[1];
            var algorithm = MasterDataEncryptionAlgorithm.FromVersion(version);

            // Get C# implementation result
            byte[] csharpDecrypted = algorithm.Decrypt(File.ReadAllBytes(filePath));

            // Get Node.js implementation result
            byte[] nodeDecrypted = DecryptUsingNode(filePath, version);

            // Compare results
            Assert.IsTrue(csharpDecrypted.SequenceEqual(nodeDecrypted),
                $"Decryption results differ for file {Path.GetFileName(filePath)}");
        }

        private static byte[] DecryptUsingNode(string inputPath, string version)
        {
            // Create a temporary directory for the node script
            var tempDir = Path.Combine(Path.GetTempPath(), "masterdata_test");
            Directory.CreateDirectory(tempDir);

            // Write the decrypt script
            var scriptPath = Path.Combine(tempDir, "decrypt.js");
            File.WriteAllText(scriptPath, @"
const fs = require('fs');
const crypto = require('crypto');

function decryptMasterData(masterdata, version) {
    const hash = crypto.createHash('sha256')
        .update(`wTmkW6hwnA6HXnItdXjZp/BSOdPuh2L9QzdM3bx1e54=${version}`)
        .digest();
    const key = hash.subarray(0, 16);
    const iv = hash.subarray(16, 32);
    const decipher = crypto.createDecipheriv('aes-128-cbc', key, iv);
    decipher.setAutoPadding(true);
    let dec = decipher.update(masterdata);
    dec = Buffer.concat([dec, decipher.final()]);
    return dec;
}

const inputData = fs.readFileSync(process.argv[2]);
const version = process.argv[3];
const decrypted = decryptMasterData(inputData, version);
fs.writeFileSync(process.argv[4], decrypted);");

            // Setup output path
            var outputPath = Path.Combine(tempDir, "output.bin");

            // Run node script
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "node",
                    Arguments = $"\"{scriptPath}\" \"{inputPath}\" \"{version}\" \"{outputPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Node script failed: {process.StandardError.ReadToEnd()}");
            }

            // Read the result
            var result = File.ReadAllBytes(outputPath);

            // Cleanup
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }

            return result;
        }
    }
}
