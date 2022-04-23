// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// The kinds of tokens that may appear in a QIF file.
/// </summary>
public enum QifToken
{
    /// <summary>
    /// The beginning of the file, before <see cref="QifParser.Read"/> is called.
    /// </summary>
    BOF,

    /// <summary>
    /// The end of the file, after <see cref="QifParser.Read"/> has returned <see langword="false"/>.
    /// </summary>
    EOF,

    /// <summary>
    /// The reader is positioned at a <c>!</c> header (e.g. <c>!Type:</c>, <c>!Account</c>).
    /// </summary>
    Header,

    /// <summary>
    /// The reader is positioned at a detail line.
    /// </summary>
    Field,

    /// <summary>
    /// The reader is positioned at a comma-delimited value.
    /// </summary>
    CommaDelimitedValue,

    /// <summary>
    /// The reader is positioned at the end of a list of fields for an individual record.
    /// </summary>
    EndOfRecord,
}
