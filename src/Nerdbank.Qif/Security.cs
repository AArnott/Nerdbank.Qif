// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// A security.
/// </summary>
/// <param name="Name">The name of the security.</param>
public record Security(string Name)
{
    /// <summary>
    /// Gets the ticker symbol for this security.
    /// </summary>
    public string? Symbol { get; init; }

    /// <summary>
    /// Gets the type of security.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// The names of each field that may appear in this record.
    /// </summary>
    internal static class FieldNames
    {
        internal const string Name = "N";
        internal const string Symbol = "S";
        internal const string Type = "T";
    }
}
