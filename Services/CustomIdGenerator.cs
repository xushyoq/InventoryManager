using System.Text;
using System.Text.RegularExpressions;

namespace InventoryManager.Services
{
    /// <summary>
    /// Generates human-readable custom IDs from a template.
    ///
    /// Supported placeholders:
    ///   {YEAR}      — current 4-digit year
    ///   {MONTH}     — current 2-digit month (zero-padded)
    ///   {DAY}       — current 2-digit day (zero-padded)
    ///   {SEQ}       — sequence number (no padding)
    ///   {SEQ:N}     — sequence number zero-padded to N digits
    ///
    /// Example template: "COMP-{YEAR}-{SEQ:4}"
    ///   with counter = 5  →  "COMP-2026-0005"
    /// </summary>
    public static class CustomIdGenerator
    {
        private static readonly Regex SeqPattern =
            new(@"\{SEQ(?::(\d+))?\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string Generate(string template, int counter)
        {
            var now = DateTime.UtcNow;

            var result = template
                .Replace("{YEAR}", now.Year.ToString("D4"), StringComparison.OrdinalIgnoreCase)
                .Replace("{MONTH}", now.Month.ToString("D2"), StringComparison.OrdinalIgnoreCase)
                .Replace("{DAY}", now.Day.ToString("D2"), StringComparison.OrdinalIgnoreCase);

            result = SeqPattern.Replace(result, match =>
            {
                var paddingGroup = match.Groups[1];
                if (paddingGroup.Success && int.TryParse(paddingGroup.Value, out var width))
                    return counter.ToString($"D{width}");
                return counter.ToString();
            });

            return result;
        }

        /// <summary>
        /// Returns true if the template contains at least one known placeholder.
        /// Used to validate user input before saving.
        /// </summary>
        public static bool IsValidTemplate(string? template)
        {
            if (string.IsNullOrWhiteSpace(template)) return false;
            return template.Contains("{YEAR}", StringComparison.OrdinalIgnoreCase)
                || template.Contains("{MONTH}", StringComparison.OrdinalIgnoreCase)
                || template.Contains("{DAY}", StringComparison.OrdinalIgnoreCase)
                || SeqPattern.IsMatch(template);
        }
    }
}
