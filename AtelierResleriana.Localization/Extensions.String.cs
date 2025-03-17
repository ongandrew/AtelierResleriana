namespace AtelierResleriana.Localization
{
    public static partial class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static bool IsValidLocalization(this string? text, string language)
        {
            if (language == "en")
            {
                return !ShouldLocalize(text);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines if a string should be localized by checking for Japanese characters.
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <returns>True if the text contains Japanese characters, false otherwise</returns>
        public static bool ShouldLocalize(this string? text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return text.Any(c =>
                (c >= 0x3040 && c <= 0x309F) ||    // Hiragana
                (c >= 0x30A0 && c <= 0x30FF) ||    // Katakana
                (c >= 0x4E00 && c <= 0x9FFF) ||    // CJK Unified Ideographs (Kanji)
                (c >= 0xFF65 && c <= 0xFF9F));     // Halfwidth Katakana
        }
    }
}
