// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// Determines how dates will be parsed.
/// </summary>
public enum ReadDateFormatMode
{
    /// <summary>
    /// This will use <c>DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out result)</c>.
    /// </summary>
    Default,

    /// <summary>
    /// This allows you to define a custom date format string.
    /// </summary>
    Custom,
}
