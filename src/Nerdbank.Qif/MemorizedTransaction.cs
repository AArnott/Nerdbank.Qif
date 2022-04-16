// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    public int? AmortizationNumberOfPaymentsAlreadyMade { get; set; }

    /// <summary>
    /// Gets or sets the amortization number of periods per year.
    /// </summary>
    public int? AmortizationNumberOfPeriodsPerYear { get; set; }

    /// <summary>
    /// Gets or sets the amortization original loan amount.
    /// </summary>
    public decimal? AmortizationOriginalLoanAmount { get; set; }

    /// <summary>
    /// Gets or sets the amortization total years for loan.
    /// </summary>
    public int? AmortizationTotalYearsForLoan { get; set; }

    /// <summary>
    /// The names of each field that may appear in this record.
    /// </summary>
    internal new class FieldNames : BankTransaction.FieldNames
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
