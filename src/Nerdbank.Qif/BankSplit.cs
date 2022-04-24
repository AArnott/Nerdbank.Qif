// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// Represents a split transaction.
/// </summary>
public abstract record BankSplit
{
    /// <summary>Gets the category of the split.</summary>
    public string? Category { get; init; }

    /// <summary>Gets the memo of the split.</summary>
    public string? Memo { get; init; }

    /// <summary>
    /// Gets the amount of the split.
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    /// Gets the percentage of the split.
    /// </summary>
    public decimal? Percentage { get; init; }
}

/// <summary>
/// A <see cref="BankSplit"/> where the <see cref="BankSplit.Amount"/> property is specified.
/// </summary>
public record BankSplitAmount : BankSplit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BankSplitAmount"/> class.
    /// </summary>
    /// <param name="amount">The amount of the split.</param>
    public BankSplitAmount(decimal amount) => base.Amount = amount;

    /// <inheritdoc cref="BankSplit.Amount"/>
    public new decimal Amount => base.Amount!.Value;
}

/// <summary>
/// A <see cref="BankSplit"/> where the <see cref="BankSplit.Percentage"/> property is specified.
/// </summary>
public record BankSplitPercentage : BankSplit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BankSplitPercentage"/> class.
    /// </summary>
    /// <param name="percentage">The percent of the split.</param>
    public BankSplitPercentage(decimal percentage) => base.Percentage = percentage;

    /// <inheritdoc cref="BankSplit.Percentage"/>
    public new decimal Percentage => base.Percentage!.Value;
}
