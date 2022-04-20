// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// Represents a split transaction.
/// </summary>
/// <param name="Category">The category of the split.</param>
/// <param name="Memo">The memo of the split.</param>
public record BankSplit(string Category, string? Memo)
{
    /// <summary>
    /// Gets the amount of the split.
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    /// Gets the percentage of the split.
    /// </summary>
    public decimal? Percentage { get; init; }
}
