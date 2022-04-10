// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

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
