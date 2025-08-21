namespace RESTfulAPI.Domain.Utilities
{
    public static class NameCasing
    {
        /// First char upper, rest lower.
        public static string? FirstUpperRestLower(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            var s = value.Trim();
            if (s.Length == 1) return s.ToUpperInvariant();

            return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
        }
    }
}