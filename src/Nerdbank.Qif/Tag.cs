// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// A tag.
/// </summary>
/// <param name="Name">The tag name.</param>
public record Tag(string Name)
{
    /// <summary>
    /// Gets the tag description.
    /// </summary>
    public string? Description { get; init; }

    internal static class FieldNames
    {
        internal const string Name = "N";
        internal const string Description = "D";
    }
}
