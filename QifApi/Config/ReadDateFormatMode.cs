using System;

namespace QifApi
{
    /// <summary>
    /// Determines how dates will be parsed.
    /// </summary>
    public enum ReadDateFormatMode
    {
        /// <summary>
        /// This will use DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out result).
        /// </summary>
        Default,

        /// <summary>
        /// This allows you to define a custom date format string.
        /// </summary>
        Custom,
    }
}
