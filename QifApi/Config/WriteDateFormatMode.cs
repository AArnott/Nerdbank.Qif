using System;

namespace QifApi.Config
{
    /// <summary>
    /// Determines how dates will be written.
    /// </summary>
    public enum WriteDateFormatMode
    {
        /// <summary>
        /// This will use DateTime.ToShortDateString(), which in turn
        /// pulls from the current system settings.
        /// </summary>
        Default,

        /// <summary>
        /// This allows you to define a custom date format string.
        /// </summary>
        Custom,
    }
}
