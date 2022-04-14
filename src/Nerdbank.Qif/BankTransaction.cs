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
    /// Deserializes a <see cref="BankTransaction"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    /// <devremarks>
    /// Keep this in close sync with <see cref="MemorizedTransaction.Load(QifReader)"/>.
    /// </devremarks>
    public static BankTransaction Load(QifReader reader)
    {
        DateTime? date = null;
        decimal? amount = null;
        ClearedState clearedStatus = ClearedState.None;
        string? number = null;
        string? payee = null;
        string? memo = null;
        string? category = null;
        ImmutableList<string> address = ImmutableList<string>.Empty;
        ImmutableList<string> splitCategories = ImmutableList<string>.Empty;
        ImmutableList<string> splitMemos = ImmutableList<string>.Empty;
        ImmutableList<decimal> splitAmounts = ImmutableList<decimal>.Empty;
        ImmutableList<decimal> splitPercentage = ImmutableList<decimal>.Empty;
        while (reader.TryReadField(out ReadOnlyMemory<char> fieldName, out _))
        {
            if (QifUtilities.Equals(FieldNames.Date, fieldName))
            {
                date = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(FieldNames.Amount, fieldName))
            {
                amount = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(FieldNames.ClearedStatus, fieldName))
            {
                clearedStatus = reader.ReadFieldAsClearedState();
            }
            else if (QifUtilities.Equals(FieldNames.Number, fieldName))
            {
                number = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.Payee, fieldName))
            {
                payee = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.Memo, fieldName))
            {
                memo = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.Category, fieldName))
            {
                category = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.Address, fieldName))
            {
                address = address.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(FieldNames.SplitCategory, fieldName))
            {
                splitCategories = splitCategories.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(FieldNames.SplitMemo, fieldName))
            {
                splitMemos = splitMemos.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(FieldNames.SplitAmount, fieldName))
            {
                splitAmounts = splitAmounts.Add(reader.ReadFieldAsDecimal());
            }
            else if (QifUtilities.Equals(FieldNames.SplitPercent, fieldName))
            {
                splitPercentage = splitPercentage.Add(reader.ReadFieldAsDecimal());
            }
        }

        if (splitCategories.Count != splitMemos.Count ||
            splitCategories.Count != Math.Max(splitAmounts.Count, splitPercentage.Count))
        {
            throw new InvalidTransactionException("Inconsistent number of fields for splits.");
        }

        ImmutableList<BankSplit> splits = ImmutableList<BankSplit>.Empty;
        if (splitCategories.Count > 0)
        {
            var splitsBuilder = splits.ToBuilder();
            for (int i = 0; i < splitCategories.Count; i++)
            {
                BankSplit split = new(splitCategories[i], splitMemos[i])
                {
                    Amount = splitAmounts.Count > i ? splitAmounts[i] : null,
                    Percentage = splitPercentage.Count > i ? splitPercentage[i] : null,
                };
                splitsBuilder.Add(split);
            }

            splits = splitsBuilder.ToImmutable();
        }

        reader.ReadEndOfRecord();

        return new(
            ValueOrThrow(date, FieldNames.Date),
            ValueOrThrow(amount, FieldNames.Amount))
        {
            ClearedStatus = clearedStatus,
            Number = number,
            Payee = payee,
            Memo = memo,
            Category = category,
            Address = address,
            Splits = splits,
        };
    }

    /// <summary>
    /// The names of each field that may appear in this record.
    /// </summary>
    private protected class FieldNames
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
