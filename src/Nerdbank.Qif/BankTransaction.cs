// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// A bank transaction.
/// </summary>
/// <param name="AccountType">The type of account this transaction is found within, or the type of this transaction.</param>
/// <param name="Date">The date of the transaction.</param>
/// <param name="Amount">The amount of the transaction.</param>
public partial record BankTransaction(AccountType AccountType, DateTime Date, decimal Amount) : Transaction(AccountType, Date)
{
    /// <summary>
    /// Gets the check number. Can also be "Deposit", "Transfer", "Print", "ATM", "EFT".
    /// </summary>
    public string? Number { get; init; }

    /// <summary>
    /// Gets the category assigned to the transaction.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets the set of tags applied to this transaction.
    /// </summary>
    public ImmutableSortedSet<string> Tags { get; init; } = ImmutableSortedSet<string>.Empty;

    /// <summary>
    /// Gets the list of address lines of the payee of this transaction.
    /// </summary>
    /// <remarks>
    /// Up to 5 address lines are allowed. A 6th address line is a message that prints on the check. 1st line is normally the same as the Payee line—the name of the Payee.
    /// </remarks>
    public ImmutableList<string> Address { get; init; } = ImmutableList<string>.Empty;

    /// <summary>
    /// Gets the split lines in the transaction.
    /// </summary>
    public ImmutableList<BankSplit> Splits { get; init; } = ImmutableList<BankSplit>.Empty;

    /// <inheritdoc/>
    public virtual bool Equals(BankTransaction? other)
    {
        return (object)this == other ||
            ((object?)other != null
            && this.EqualityContract == other!.EqualityContract
            && EqualityComparer<DateTime>.Default.Equals(this.Date, other!.Date)
            && EqualityComparer<decimal>.Default.Equals(this.Amount, other!.Amount)
            && EqualityComparer<ClearedState>.Default.Equals(this.ClearedStatus, other!.ClearedStatus)
            && EqualityComparer<string?>.Default.Equals(this.Number, other!.Number)
            && EqualityComparer<string?>.Default.Equals(this.Payee, other!.Payee)
            && EqualityComparer<string?>.Default.Equals(this.Memo, other!.Memo)
            && EqualityComparer<string?>.Default.Equals(this.Category, other!.Category)
            && ByValueCollectionComparer<string>.Default.Equals(this.Tags, other!.Tags)
            && ByValueCollectionComparer<string>.Default.Equals(this.Address, other!.Address)
            && ByValueCollectionComparer<BankSplit>.Default.Equals(this.Splits, other!.Splits));
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        HashCode hash = default;
        hash.Add(this.EqualityContract);
        hash.Add(this.Date);
        hash.Add(this.Amount);
        hash.Add(this.ClearedStatus);
        hash.Add(this.Number);
        hash.Add(this.Payee);
        hash.Add(this.Memo);
        hash.Add(this.Category);
        hash.Add(this.Tags, ByValueCollectionComparer<string>.Default);
        hash.Add(this.Address, ByValueCollectionComparer<string>.Default);
        hash.Add(this.Splits, ByValueCollectionComparer<BankSplit>.Default);
        return hash.ToHashCode();
    }

    /// <summary>
    /// The names of each field that may appear in this record.
    /// </summary>
    internal class FieldNames
    {
        internal const string Date = "D";
        internal const string Amount = "T";
        internal const string ClearedStatus = "C";
        internal const string Number = "N";
        internal const string Payee = "P";
        internal const string Memo = "M";
        internal const string Category = "L";
        internal const string Address = "A";
        internal const string SplitCategory = "S";
        internal const string SplitMemo = "E";
        internal const string SplitAmount = "$";
        internal const string SplitPercent = "%";
    }
}
