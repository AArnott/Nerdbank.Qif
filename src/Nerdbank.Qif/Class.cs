// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// A class, that may be used to help describe a transaction.
/// </summary>
/// <param name="Name">The name of the class.</param>
public record Class(string Name)
{
    /// <summary>
    /// Gets the description of this class.
    /// </summary>
    public string? Description { get; init; }

    internal static class FieldNames
    {
        internal const string Name = "N";
        internal const string Description = "D";
    }
}
