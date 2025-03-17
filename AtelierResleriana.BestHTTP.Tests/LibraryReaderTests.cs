using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AtelierResleriana.BestHTTP
{
    [TestClass]
    [TestCategory(nameof(LibraryReader))]
    public sealed class LibraryReaderTests
    {
        [TestMethod]
        [DataRow("Resources/Library1", 1)]
        [DataRow("Resources/Library2", 6)]
        [DataRow("Resources/Library3", 21)]
        public void CanReadLibrary(string filePath, int entryCount)
        {
            LibraryReader libraryReader = new LibraryReader();
            IEnumerable<LibraryEntry> libraryEntries = libraryReader.Read(File.OpenRead(filePath)).ToArray();

            Assert.IsNotNull(libraryEntries);
            Assert.AreEqual(entryCount, libraryEntries.Count());

            LibraryEntry libraryEntry = libraryEntries.First();

            Console.WriteLine(JsonSerializer.Serialize(libraryEntry));
        }
    }
}
