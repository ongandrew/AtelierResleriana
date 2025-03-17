using System;
using System.Collections.Generic;

namespace AtelierResleriana.Text
{
    /// <summary>
    /// Validates and compares format parameters in strings used for string.Format operations.
    /// </summary>
    public class StringFormatParameterMatcher
    {
        /// <summary>
        /// Determines whether two strings contain the same set of format parameters.
        /// </summary>
        /// <param name="first">The first string to compare.</param>
        /// <param name="second">The second string to compare.</param>
        /// <returns>
        /// true if both strings contain exactly the same set of format parameters;
        /// false if the parameter sets differ or if either string is null.
        /// </returns>
        /// <example>
        /// <code>
        /// var validator = new FormatParameterValidator();
        /// bool match = validator.IsMatch("Hello {0}", "こんにちは {0}"); // Returns true
        /// bool noMatch = validator.IsMatch("Value {0}", "値 {1}"); // Returns false
        /// </code>
        /// </example>
        public bool IsMatch(string first, string second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            ISet<int> firstFormatParameters = GetFormatParameters(first);
            ISet<int> secondFormatParameters = GetFormatParameters(second);
            return firstFormatParameters.SetEquals(secondFormatParameters);
        }

        /// <summary>
        /// Extracts all unique format parameters from a string.
        /// </summary>
        /// <param name="text">The string to analyze for format parameters.</param>
        /// <returns>
        /// A set of integers representing the unique format parameters found in the string.
        /// Returns an empty set if the text is null or contains no valid format parameters.
        /// </returns>
        /// <remarks>
        /// Format parameters must be in the form {n} where n is an integer.
        /// Nested braces or invalid format specifiers are ignored.
        /// </remarks>
        private static ISet<int> GetFormatParameters(string text)
        {
            var parameters = new HashSet<int>();

            if (string.IsNullOrEmpty(text))
            {
                return parameters;
            }

            ReadOnlySpan<char> span = text.AsSpan();
            int currentIndex = 0;

            while (currentIndex < span.Length)
            {
                // Find opening brace
                int openBrace = span.Slice(currentIndex).IndexOf('{');
                if (openBrace == -1) break;

                currentIndex += openBrace;

                // Check if we have room for at least "{0}"
                if (currentIndex + 2 >= span.Length) break;

                // Find closing brace
                int searchStart = currentIndex + 1;
                int closeBrace = span.Slice(searchStart).IndexOf('}');
                if (closeBrace == -1) break;

                closeBrace += searchStart;

                // Extract and parse the number
                ReadOnlySpan<char> numberSpan = span.Slice(currentIndex + 1, closeBrace - currentIndex - 1);
                if (int.TryParse(numberSpan, out int paramIndex))
                {
                    parameters.Add(paramIndex);
                }

                currentIndex = closeBrace + 1;
            }

            return parameters;
        }
    }
}