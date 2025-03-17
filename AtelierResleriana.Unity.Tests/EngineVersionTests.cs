using System;

namespace AtelierResleriana.Unity
{
    [TestClass]
    [TestCategory(nameof(EngineVersion))]
    public sealed class EngineVersionTests
    {
        [TestMethod]
        public void Constructor_WithValidInput_CreatesInstance()
        {
            var version = new EngineVersion(1, 2, 3);
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.IsNull(version.Suffix);
        }

        [TestMethod]
        public void Constructor_WithSuffix_CreatesInstance()
        {
            var version = new EngineVersion(1, 2, 3, "f1");
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.AreEqual("f1", version.Suffix);
        }

        [TestMethod]
        public void Parse_ValidVersion_ReturnsInstance()
        {
            var version = EngineVersion.Parse("1.2.3");
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.IsNull(version.Suffix);
        }

        [TestMethod]
        public void Parse_VersionWithSuffix_ReturnsInstance()
        {
            var version = EngineVersion.Parse("1.2.3f1");
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.AreEqual("f1", version.Suffix);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Parse_InvalidFormat_ThrowsFormatException()
        {
            EngineVersion.Parse("1.2");
        }

        [TestMethod]
        public void TryParse_ValidVersion_ReturnsTrue()
        {
            bool success = EngineVersion.TryParse("1.2.3", out var version);
            Assert.IsTrue(success);
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.IsNull(version.Suffix);
        }

        [TestMethod]
        public void TryParse_ValidVersionWithSuffix_ReturnsTrue()
        {
            bool success = EngineVersion.TryParse("1.2.3f1", out var version);
            Assert.IsTrue(success);
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(2, version.Minor);
            Assert.AreEqual(3, version.Patch);
            Assert.AreEqual("f1", version.Suffix);
        }

        [TestMethod]
        public void TryParse_InvalidFormat_ReturnsFalse()
        {
            bool success = EngineVersion.TryParse("1.2", out var version);
            Assert.IsFalse(success);
            Assert.AreEqual(0, version.Major);
            Assert.AreEqual(0, version.Minor);
            Assert.AreEqual(0, version.Patch);
            Assert.IsNull(version.Suffix);
        }

        [TestMethod]
        public void TryParse_NonNumericComponents_ReturnsFalse()
        {
            bool success = EngineVersion.TryParse("1.a.3", out var version);
            Assert.IsFalse(success);
            Assert.AreEqual(0, version.Major);
            Assert.AreEqual(0, version.Minor);
            Assert.AreEqual(0, version.Patch);
            Assert.IsNull(version.Suffix);
        }

        [TestMethod]
        public void TryParse_Null_ReturnsFalse()
        {
            bool success = EngineVersion.TryParse(null, out var version);
            Assert.IsFalse(success);
            Assert.AreEqual(0, version.Major);
            Assert.AreEqual(0, version.Minor);
            Assert.AreEqual(0, version.Patch);
            Assert.IsNull(version.Suffix);
        }

        [TestMethod]
        public void TryParse_Empty_ReturnsFalse()
        {
            bool success = EngineVersion.TryParse("", out var version);
            Assert.IsFalse(success);
            Assert.AreEqual(0, version.Major);
            Assert.AreEqual(0, version.Minor);
            Assert.AreEqual(0, version.Patch);
            Assert.IsNull(version.Suffix);
        }

        [TestMethod]
        public void CompareTo_DifferentMajor_ReturnsExpectedOrder()
        {
            var v1 = new EngineVersion(1, 0, 0);
            var v2 = new EngineVersion(2, 0, 0);
            Assert.IsTrue(v1.CompareTo(v2) < 0);
            Assert.IsTrue(v2.CompareTo(v1) > 0);
        }

        [TestMethod]
        public void CompareTo_DifferentMinor_ReturnsExpectedOrder()
        {
            var v1 = new EngineVersion(1, 0, 0);
            var v2 = new EngineVersion(1, 1, 0);
            Assert.IsTrue(v1.CompareTo(v2) < 0);
            Assert.IsTrue(v2.CompareTo(v1) > 0);
        }

        [TestMethod]
        public void CompareTo_DifferentPatch_ReturnsExpectedOrder()
        {
            var v1 = new EngineVersion(1, 0, 0);
            var v2 = new EngineVersion(1, 0, 1);
            Assert.IsTrue(v1.CompareTo(v2) < 0);
            Assert.IsTrue(v2.CompareTo(v1) > 0);
        }

        [TestMethod]
        public void CompareTo_WithAndWithoutSuffix_ReturnsExpectedOrder()
        {
            var v1 = new EngineVersion(1, 0, 0);
            var v2 = new EngineVersion(1, 0, 0, "f1");
            Assert.IsTrue(v1.CompareTo(v2) < 0);
            Assert.IsTrue(v2.CompareTo(v1) > 0);
        }

        [TestMethod]
        public void CompareTo_DifferentSuffixes_ReturnsExpectedOrder()
        {
            var v1 = new EngineVersion(1, 0, 0, "f1");
            var v2 = new EngineVersion(1, 0, 0, "f2");
            Assert.IsTrue(v1.CompareTo(v2) < 0);
            Assert.IsTrue(v2.CompareTo(v1) > 0);
        }

        [TestMethod]
        public void Equals_SameVersion_ReturnsTrue()
        {
            var v1 = new EngineVersion(1, 2, 3, "f1");
            var v2 = new EngineVersion(1, 2, 3, "f1");
            Assert.IsTrue(v1.Equals(v2));
            Assert.IsTrue(v2.Equals(v1));
            Assert.IsTrue(v1 == v2);
            Assert.IsFalse(v1 != v2);
        }

        [TestMethod]
        public void Equals_DifferentVersions_ReturnsFalse()
        {
            var v1 = new EngineVersion(1, 2, 3, "f1");
            var v2 = new EngineVersion(1, 2, 3, "f2");
            Assert.IsFalse(v1.Equals(v2));
            Assert.IsFalse(v2.Equals(v1));
            Assert.IsFalse(v1 == v2);
            Assert.IsTrue(v1 != v2);
        }

        [TestMethod]
        public void ToString_WithoutSuffix_ReturnsExpectedFormat()
        {
            var version = new EngineVersion(1, 2, 3);
            Assert.AreEqual("1.2.3", version.ToString());
        }

        [TestMethod]
        public void ToString_WithSuffix_ReturnsExpectedFormat()
        {
            var version = new EngineVersion(1, 2, 3, "f1");
            Assert.AreEqual("1.2.3f1", version.ToString());
        }

        [TestMethod]
        public void Operators_LessThanGreaterThan_WorkAsExpected()
        {
            var v1 = new EngineVersion(1, 0, 0);
            var v2 = new EngineVersion(2, 0, 0);

            Assert.IsTrue(v1 < v2);
            Assert.IsTrue(v1 <= v2);
            Assert.IsTrue(v2 > v1);
            Assert.IsTrue(v2 >= v1);
        }

        [TestMethod]
        public void GetHashCode_SameVersion_ReturnsSameHash()
        {
            var v1 = new EngineVersion(1, 2, 3);
            var v2 = new EngineVersion(1, 2, 3);
            Assert.AreEqual(v1.GetHashCode(), v2.GetHashCode());
        }
    }
}