// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static Nerdbank.Qif.QifUtilities;

namespace Nerdbank.Qif;

/// <summary>
/// A bank transaction.
/// </summary>
/// <param name="Date">The date of the transaction.</param>
/// <param name="Amount">The amount of the transaction.</param>
public partial record BankTransaction(DateTime Date, decimal Amount)
{
    /// <summary>
    /// The QIF header that introduces <see cref="BankTransaction"/> records.
    /// </summary>
    public static readonly (string Name, string Value) Header = ("Type", "Bank");

    /// <summary>
    /// Gets the cleared or reconciled state of the transaction.
    /// </summary>
    public ClearedState ClearedStatus { get; init; }

    /// <summary>
    /// Gets the check number. Can also be "Deposit", "Transfer", "Print", "ATM", "EFT".
    /// </summary>
    public string? Number { get; init; }

    /// <summary>
    /// Gets the transaction payee.
    /// </summary>
    public string? Payee { get; init; }

    /// <summary>
    /// Gets the transaction memo.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    /// Gets the category assigned to the transaction.
    /// </summary>
    public string? Category { get; init; }

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

    /// <summary>
    /// Gets the name of the account these transactions belong to, if known.
    /// </summary>
    public string? AccountName { get; init; }

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
