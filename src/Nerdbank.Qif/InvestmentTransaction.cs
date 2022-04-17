// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// An investment transaction.
/// </summary>
/// <param name="Date">The date of the transaction.</param>
public record InvestmentTransaction(DateTime Date) : Transaction(Date)
{
    /// <summary>
    /// Gets the action.
    /// </summary>
    public string? Action { get; init; }

    /// <summary>
    /// Gets the security.
    /// </summary>
    public string? Security { get; init; }

    /// <summary>
    /// Gets the price.
    /// </summary>
    public decimal? Price { get; init; }

    /// <summary>
    /// Gets the quantity.
    /// </summary>
    public decimal? Quantity { get; init; }

    /// <summary>
    /// Gets the transaction amount.
    /// </summary>
    public decimal? TransactionAmount { get; init; }

    /// <summary>
    /// Gets the commission.
    /// </summary>
    public decimal? Commission { get; init; }

    /// <summary>
    /// Gets the account for transfer.
    /// </summary>
    public string? AccountForTransfer { get; init; }

    /// <summary>
    /// Gets the amount transferred, if cash is moved between accounts.
    /// </summary>
    public decimal? AmountTransferred { get; init; }

    /// <summary>
    /// The names of each field that may appear in this record.
    /// </summary>
    internal static class FieldNames
    {
        internal const string Date = "D";
        internal const string Action = "N";
        internal const string Security = "Y";
        internal const string Price = "I";
        internal const string Quantity = "Q";
        internal const string TransactionAmount = "T";
        internal const string TransactionAmount2 = "U";
        internal const string ClearedStatus = "C";
        internal const string Payee = "P";
        internal const string Memo = "M";
        internal const string Commission = "O";
        internal const string AccountForTransfer = "L";
        internal const string AmountTransferred = "$";
        internal const string EndOfEntry = "^";
    }
}
