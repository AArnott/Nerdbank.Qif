// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// A memorized transaction.
/// </summary>
/// <param name="Type">The type of memorized transaction.</param>
public record MemorizedTransaction(MemorizedTransactionType Type)
{
    /// <summary>
    /// Gets the cleared or reconciled state of the transaction.
    /// </summary>
    public ClearedState ClearedStatus { get; init; }

    /// <summary>
    /// Gets the payee, or a description for deposits, transfers, etc.
    /// </summary>
    public string? Payee { get; init; }

    /// <summary>
    /// Gets the memo.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    /// Gets the date of the transaction.
    /// </summary>
    public DateTime? Date { get; init; }

    /// <summary>
    /// Gets the check number. Can also be "Deposit", "Transfer", "Print", "ATM", "EFT".
    /// </summary>
    public string? Number { get; init; }

    /// <summary>
    /// Gets the amount of the transaction.
    /// </summary>
    public decimal? Amount { get; init; }

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
    /// Gets the amortization current loan balance.
    /// </summary>
    public decimal? AmortizationCurrentLoanBalance { get; init; }

    /// <summary>
    /// Gets the amortization first payment date.
    /// </summary>
    public DateTime? AmortizationFirstPaymentDate { get; init; }

    /// <summary>
    /// Gets the amortization interest rate.
    /// </summary>
    public decimal? AmortizationInterestRate { get; init; }

    /// <summary>
    /// Gets the amortization number of payments already made.
    /// </summary>
    public int? AmortizationNumberOfPaymentsAlreadyMade { get; init; }

    /// <summary>
    /// Gets the amortization number of periods per year.
    /// </summary>
    public int? AmortizationNumberOfPeriodsPerYear { get; init; }

    /// <summary>
    /// Gets the amortization original loan amount.
    /// </summary>
    public decimal? AmortizationOriginalLoanAmount { get; init; }

    /// <summary>
    /// Gets the amortization total years for loan.
    /// </summary>
    public int? AmortizationTotalYearsForLoan { get; init; }

    /// <summary>
    /// The names of each field that may appear in this record.
    /// </summary>
    internal class FieldNames : BankTransaction.FieldNames
    {
        internal const string Type = "K";
        internal const string AmortizationFirstPaymentDate = "1";
        internal const string AmortizationTotalYearsForLoan = "2";
        internal const string AmortizationNumberOfPaymentsAlreadyMade = "3";
        internal const string AmortizationNumberOfPeriodsPerYear = "4";
        internal const string AmortizationInterestRate = "5";
        internal const string AmortizationCurrentLoanBalance = "6";
        internal const string AmortizationOriginalLoanAmount = "7";
    }

    internal static class TransactionTypeCodes
    {
        internal const char Check = 'C';
        internal const char Deposit = 'D';
        internal const char Payment = 'P';
        internal const char Investment = 'I';
        internal const char ElectronicPayee = 'E';
    }
}
