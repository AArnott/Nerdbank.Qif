// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif.Logic;

/// <summary>
/// The memorized transaction list fields used in transactions.
/// </summary>
internal static class MemorizedTransactionListFields
{
    /// <summary>
    /// Check transaction.
    /// </summary>
    internal const string CheckTransaction = "KC";

    /// <summary>
    /// Deposit transaction.
    /// </summary>
    internal const string DepositTransaction = "KD";

    /// <summary>
    /// Payment transaction.
    /// </summary>
    internal const string PaymentTransaction = "KP";

    /// <summary>
    /// Investment transaction.
    /// </summary>
    internal const string InvestmentTransaction = "KI";

    /// <summary>
    /// Electronic payee transaction.
    /// </summary>
    internal const string ElectronicPayeeTransaction = "KE";

    /// <summary>
    /// Amount.
    /// </summary>
    internal const string Amount = "T";

    /// <summary>
    /// Cleared status.
    /// </summary>
    internal const string ClearedStatus = "C";

    /// <summary>
    /// Payee.
    /// </summary>
    internal const string Payee = "P";

    /// <summary>
    /// Memo.
    /// </summary>
    internal const string Memo = "M";

    /// <summary>
    /// Up to five lines; the sixth line is an optional message.
    /// </summary>
    internal const string Address = "A";

    /// <summary>
    /// Category or Transfer/Class.
    /// </summary>
    internal const string Category = "L";

    /// <summary>
    /// Category/class in split.
    /// </summary>
    internal const string SplitCategory = "S";

    /// <summary>
    /// Memo in split.
    /// </summary>
    internal const string SplitMemo = "E";

    /// <summary>
    /// Dollar amount of split.
    /// </summary>
    internal const string SplitAmount = "$";

    /// <summary>
    /// Amortization: First payment date.
    /// </summary>
    internal const string AmortizationFirstPaymentDate = "1";

    /// <summary>
    /// Amortization: Total years for loan.
    /// </summary>
    internal const string AmortizationTotalYearsForLoan = "2";

    /// <summary>
    /// Amortization: Number of payments already made.
    /// </summary>
    internal const string AmortizationNumberOfPaymentsAlreadyMade = "3";

    /// <summary>
    /// Amortization: Number of periods per year.
    /// </summary>
    internal const string AmortizationNumberOfPeriodsPerYear = "4";

    /// <summary>
    /// Amortization: Interest rate.
    /// </summary>
    internal const string AmortizationInterestRate = "5";

    /// <summary>
    /// Amortization: Current loan balance.
    /// </summary>
    internal const string AmortizationCurrentLoanBalance = "6";

    /// <summary>
    /// Amortization: Original loan amount.
    /// </summary>
    internal const string AmortizationOriginalLoanAmount = "7";

    /// <summary>
    /// End of entry.
    /// </summary>
    internal const string EndOfEntry = "^";
}
