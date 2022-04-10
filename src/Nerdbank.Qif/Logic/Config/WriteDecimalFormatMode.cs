// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// Determines how decimals will be written.
/// </summary>
public enum WriteDecimalFormatMode
{
    /// <summary>
    /// This will use decimal.ToString(CultureInfo.CurrentCulture)
    /// to write decimal values.
    /// </summary>
    Default,

    /// <summary>
    /// This allows you to define a custom decimal format string.
    /// </summary>
    Custom,
}
