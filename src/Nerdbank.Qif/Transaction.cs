// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// A base type for a transaction.
/// </summary>
/// <param name="AccountType">The type of account this transaction is found within, or the type of this transaction.</param>
/// <param name="Date">The date of the transaction.</param>
/// <remarks>
/// Supported derived types are <see cref="BankTransaction"/> and <see cref="InvestmentTransaction"/>.
/// </remarks>
public abstract record Transaction(AccountType AccountType, DateTime Date)
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
}
