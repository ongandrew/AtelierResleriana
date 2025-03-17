using System.Globalization;

namespace AtelierResleriana.Unity
{
    /// <summary>
    /// Represents a version number with a suffix, consisting of major, minor, and patch components.
    /// Unity-specific implementation.
    /// </summary>
    /// <remarks>
    /// Version numbers are compared first by their numeric components, then by their suffixes.
    /// For example:
    /// - "2.5.0" comes before "2.5.1"
    /// - "2.5.0" comes before "2.5.0f1"
    /// - "2.5.0f1" comes before "2.5.0f2"
    /// </remarks>
    public readonly struct EngineVersion : IEquatable<EngineVersion>, IComparable<EngineVersion>
    {
        /// <summary>
        /// Gets the major version component.
        /// </summary>
        public int Major { get; }
        /// <summary>
        /// Gets the minor version component.
        /// </summary>
        public int Minor { get; }
        /// <summary>
        /// Gets the patch version component.
        /// </summary>
        public int Patch { get; }
        /// <summary>
        /// Gets the version suffix. For example, in "2.5.0f5" the suffix would be "f5".
        /// </summary>
        public string Suffix { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineVersion"/> struct.
        /// </summary>
        /// <param name="major">The major version component.</param>
        /// <param name="minor">The minor version component.</param>
        /// <param name="patch">The patch version component.</param>
        public EngineVersion(int major, int minor, int patch) : this(major, minor, patch, null)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineVersion"/> struct.
        /// </summary>
        /// <param name="major">The major version component.</param>
        /// <param name="minor">The minor version component.</param>
        /// <param name="patch">The patch version component.</param>
        /// <param name="suffix">A version suffix.</param>
        public EngineVersion(int major, int minor, int patch, string suffix)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Suffix = suffix;
        }

        /// <summary>
        /// Parses a version string into a <see cref="EngineVersion"/>.
        /// </summary>
        /// <param name="version">The version string to parse. Must have three numeric components separated by periods, with an optional suffix.</param>
        /// <returns>A new <see cref="EngineVersion"/> instance.</returns>
        /// <exception cref="FormatException">Thrown when the version string does not have exactly three numeric components.</exception>
        /// <remarks>
        /// The version string must be in the format "X.Y.Z" or "X.Y.ZsuffixString" where X, Y, and Z are integers.
        /// </remarks>
        public static EngineVersion Parse(string version)
        {
            // Find where numbers end
            int suffixStart = 0;
            while (suffixStart < version.Length &&
                   (char.IsDigit(version[suffixStart]) || version[suffixStart] == '.'))
            {
                suffixStart++;
            }

            // Split numeric part
            string[] parts = version.Substring(0, suffixStart).Split('.');
            if (parts.Length != 3)
            {
                throw new FormatException($"Version string must have 3 numeric components: {version}");
            }

            return new EngineVersion(
                int.Parse(parts[0], CultureInfo.InvariantCulture),
                int.Parse(parts[1], CultureInfo.InvariantCulture),
                int.Parse(parts[2], CultureInfo.InvariantCulture),
                suffixStart < version.Length ? version.Substring(suffixStart) : null
            );
        }

        /// <summary>
        /// Attempts to parse a version string into a <see cref="EngineVersion"/>.
        /// </summary>
        /// <param name="version">The version string to parse. Must have three numeric components separated by periods, with an optional suffix.</param>
        /// <param name="semanticVersion">When this method returns, contains the semantic version equivalent 
        /// of the version string, if the conversion succeeded, or a default SemanticVersion with all components 
        /// set to 0 if the conversion failed.</param>
        /// <returns>true if the version was successfully parsed; otherwise, false.</returns>
        /// <remarks>
        /// The version string must be in the format "X.Y.Z" or "X.Y.ZsuffixString" where X, Y, and Z are integers.
        /// </remarks>
        public static bool TryParse(string version, out EngineVersion semanticVersion)
        {
            semanticVersion = default;

            if (string.IsNullOrEmpty(version))
                return false;

            try
            {
                // Find where numbers end
                int suffixStart = 0;
                while (suffixStart < version.Length &&
                       (char.IsDigit(version[suffixStart]) || version[suffixStart] == '.'))
                {
                    suffixStart++;
                }

                // Split numeric part
                string[] parts = version.Substring(0, suffixStart).Split('.');
                if (parts.Length != 3)
                    return false;

                if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int major) ||
                    !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int minor) ||
                    !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out int patch))
                    return false;

                semanticVersion = new EngineVersion(
                    major,
                    minor,
                    patch,
                    suffixStart < version.Length ? version.Substring(suffixStart) : null
                );

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Compares this instance with another <see cref="EngineVersion"/> and returns an integer that indicates their relative order.
        /// </summary>
        /// <param name="other">The version to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the versions being compared.
        /// Less than zero indicates this instance precedes the other version.
        /// Zero indicates this instance is equal to the other version.
        /// Greater than zero indicates this instance follows the other version.
        /// </returns>
        /// <remarks>
        /// Versions are compared first by their numeric components (major, minor, patch), then by their suffixes.
        /// A version with no suffix comes before a version with a suffix.
        /// When both versions have suffixes, they are compared ordinally.
        /// </remarks>
        public int CompareTo(EngineVersion other)
        {
            int majorComparison = Major.CompareTo(other.Major);
            if (majorComparison != 0) return majorComparison;

            int minorComparison = Minor.CompareTo(other.Minor);
            if (minorComparison != 0) return minorComparison;

            int patchComparison = Patch.CompareTo(other.Patch);
            if (patchComparison != 0) return patchComparison;

            // If one has no suffix and the other does, no suffix comes first
            if (string.IsNullOrEmpty(Suffix) && !string.IsNullOrEmpty(other.Suffix)) return -1;
            if (!string.IsNullOrEmpty(Suffix) && string.IsNullOrEmpty(other.Suffix)) return 1;

            // Both have suffixes or both don't, compare them
            return string.Compare(Suffix ?? "", other.Suffix ?? "", StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether this instance and another specified <see cref="EngineVersion"/> have the same value.
        /// </summary>
        /// <param name="other">The version to compare with this instance.</param>
        /// <returns>true if all components including the suffix are equal; otherwise, false.</returns>
        public bool Equals(EngineVersion other) =>
            Major == other.Major &&
            Minor == other.Minor &&
            Patch == other.Patch &&
            string.Equals(Suffix ?? "", other.Suffix ?? "", StringComparison.Ordinal);

        /// <summary>
        /// Determines whether this instance and a specified object have the same value.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>true if the object is a <see cref="EngineVersion"/> and all its components including the suffix are equal to this instance; otherwise, false.</returns>
        public override bool Equals(object obj) =>
            obj is EngineVersion version && Equals(version);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Major;
                hash = hash * 31 + Minor;
                hash = hash * 31 + Patch;
                return hash;
            }
        }

        /// <summary>
        /// Returns a string representation of this version.
        /// </summary>
        /// <returns>A string in the format "major.minor.patch" or "major.minor.patchsuffix" if a suffix exists.</returns>
        public override string ToString() =>
            Suffix == null ? $"{Major}.{Minor}.{Patch}" : $"{Major}.{Minor}.{Patch}{Suffix}";

        /// <summary>
        /// Returns a value indicating whether two specified versions are equal.
        /// </summary>
        /// <param name="left">The first version to compare.</param>
        /// <param name="right">The second version to compare.</param>
        /// <returns>true if the versions are equal; otherwise, false.</returns>
        public static bool operator ==(EngineVersion left, EngineVersion right) =>
            left.Equals(right);

        /// <summary>
        /// Returns a value indicating whether two specified versions are not equal.
        /// </summary>
        /// <param name="left">The first version to compare.</param>
        /// <param name="right">The second version to compare.</param>
        /// <returns>true if the versions are not equal; otherwise, false.</returns>
        public static bool operator !=(EngineVersion left, EngineVersion right) =>
            !(left == right);

        /// <summary>
        /// Returns a value indicating whether a specified version is less than another specified version.
        /// </summary>
        /// <param name="left">The first version to compare.</param>
        /// <param name="right">The second version to compare.</param>
        /// <returns>true if left is less than right; otherwise, false.</returns>
        public static bool operator <(EngineVersion left, EngineVersion right) =>
            left.CompareTo(right) < 0;

        /// <summary>
        /// Returns a value indicating whether a specified version is greater than another specified version.
        /// </summary>
        /// <param name="left">The first version to compare.</param>
        /// <param name="right">The second version to compare.</param>
        /// <returns>true if left is greater than right; otherwise, false.</returns>
        public static bool operator >(EngineVersion left, EngineVersion right) =>
            left.CompareTo(right) > 0;

        /// <summary>
        /// Returns a value indicating whether a specified version is less than or equal to another specified version.
        /// </summary>
        /// <param name="left">The first version to compare.</param>
        /// <param name="right">The second version to compare.</param>
        /// <returns>true if left is less than or equal to right; otherwise, false.</returns>
        public static bool operator <=(EngineVersion left, EngineVersion right) =>
            left.CompareTo(right) <= 0;

        /// <summary>
        /// Returns a value indicating whether a specified version is greater than or equal to another specified version.
        /// </summary>
        /// <param name="left">The first version to compare.</param>
        /// <param name="right">The second version to compare.</param>
        /// <returns>true if left is greater than or equal to right; otherwise, false.</returns>
        public static bool operator >=(EngineVersion left, EngineVersion right) =>
            left.CompareTo(right) >= 0;

        public static implicit operator string(EngineVersion unityVersion)
        {
            return unityVersion.ToString();
        }

        public static implicit operator EngineVersion(string version)
        {
            return Parse(version);
        }
    }
}
