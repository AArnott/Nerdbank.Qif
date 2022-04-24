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

    /// <summary>
    /// Gets the set of tags applied to this split item.
    /// </summary>
    public ImmutableSortedSet<string> Tags { get; init; } = ImmutableSortedSet<string>.Empty;

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

    /// <inheritdoc/>
    public virtual bool Equals(BankSplit? other)
    {
        return (object)this == other ||
            ((object?)other != null
            && this.EqualityContract == other!.EqualityContract
            && EqualityComparer<decimal?>.Default.Equals(this.Amount, other!.Amount)
            && EqualityComparer<decimal?>.Default.Equals(this.Percentage, other!.Percentage)
            && EqualityComparer<string?>.Default.Equals(this.Memo, other!.Memo)
            && EqualityComparer<string?>.Default.Equals(this.Category, other!.Category)
            && ByValueCollectionComparer<string>.Default.Equals(this.Tags, other!.Tags));
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = default;
        hash.Add(this.EqualityContract);
        hash.Add(this.Amount);
        hash.Add(this.Percentage);
        hash.Add(this.Memo);
        hash.Add(this.Category);
        hash.Add(this.Tags, ByValueCollectionComparer<string>.Default);
        return hash.ToHashCode();
    }
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
    public BankSplitAmount(decimal amount) => this.Amount = amount;
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
    public BankSplitPercentage(decimal percentage) => this.Percentage = percentage;
}
