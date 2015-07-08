using System;

namespace QifApi
{
    /// <summary>
    /// Determines how decimals will be parsed.
    /// </summary>
    public enum ReadDecimalFormatMode
    {
        /// <summary>
        /// This will use decimal.TryParse(@this, out result)
        /// to parse decimal values.
        /// </summary>
        Default,

        /// <summary>
        /// This will use decimal.TryParse(@this, config.CustomReadDecimalFormat, CultureInfo.CurrentCulture, out result)
        /// to parse decimal values.
        /// </summary>
        Custom,
    }
}
