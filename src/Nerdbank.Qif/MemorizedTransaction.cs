// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static Nerdbank.Qif.QifUtilities;

namespace Nerdbank.Qif;

/// <summary>
/// A memorized transaction.
/// </summary>
/// <param name="Type">The type of memorized transaction.</param>
/// <param name="Date">The date of the transaction.</param>
/// <param name="Amount">The amount of the transaction.</param>
public record MemorizedTransaction(MemorizedTransactionType Type, DateTime Date, decimal Amount) : BankTransaction(Date, Amount)
{
    /// <summary>
    /// Gets or sets the amortization current loan balance.
    /// </summary>
    public decimal? AmortizationCurrentLoanBalance { get; set; }

    /// <summary>
    /// Gets or sets the amortization first payment date.
    /// </summary>
    public DateTime? AmortizationFirstPaymentDate { get; set; }

    /// <summary>
    /// Gets or sets the amortization interest rate.
    /// </summary>
    public decimal? AmortizationInterestRate { get; set; }

    /// <summary>
    /// Gets or sets the amortization number of payments already made.
    /// </summary>
    public decimal? AmortizationNumberOfPaymentsAlreadyMade { get; set; }

    /// <summary>
    /// Gets or sets the amortization number of periods per year.
    /// </summary>
    public decimal? AmortizationNumberOfPeriodsPerYear { get; set; }

    /// <summary>
    /// Gets or sets the amortization original loan amount.
    /// </summary>
    public decimal? AmortizationOriginalLoanAmount { get; set; }

    /// <summary>
    /// Gets or sets the amortization total years for loan.
    /// </summary>
    public decimal? AmortizationTotalYearsForLoan { get; set; }

    /// <summary>
    /// Deserializes a <see cref="MemorizedTransaction"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    /// <devremarks>
    /// Keep this in close sync with <see cref="BankTransaction.Load(QifReader)"/>.
    /// </devremarks>
    public static new MemorizedTransaction Load(QifReader reader)
    {
        MemorizedTransactionType? type = null;
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
            else if (QifUtilities.Equals(FieldNames.Type, fieldName))
            {
                if (reader.Field.Span.Length != 1)
                {
                    throw new InvalidTransactionException("Unexpected length in exception type.");
                }

                type = reader.Field.Span[0] switch
                {
                    TransactionTypeCodes.Check => MemorizedTransactionType.Check,
                    TransactionTypeCodes.Deposit => MemorizedTransactionType.Deposit,
                    TransactionTypeCodes.Payment => MemorizedTransactionType.Payment,
                    TransactionTypeCodes.Investment => MemorizedTransactionType.Investment,
                    TransactionTypeCodes.ElectronicPayee => MemorizedTransactionType.ElectronicPayee,
                    _ => throw new InvalidTransactionException("Unsupported memorized transaction type."),
                };
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
            ValueOrThrow(type, FieldNames.Type),
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
    private new class FieldNames : BankTransaction.FieldNames
    {
        internal const string Type = "K";
    }

    private static class TransactionTypeCodes
    {
        internal const char Check = 'C';
        internal const char Deposit = 'D';
        internal const char Payment = 'P';
        internal const char Investment = 'I';
        internal const char ElectronicPayee = 'E';
    }
}
