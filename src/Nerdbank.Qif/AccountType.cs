// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// The kinds of bank/transaction accounts.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// A typical bank account.
    /// </summary>
    Bank,

    /// <summary>
    /// A cash account.
    /// </summary>
    Cash,

    /// <summary>
    /// A credit card account.
    /// </summary>
    CreditCard,

    /// <summary>
    /// An investment account.
    /// </summary>
    Investment,

    /// <summary>
    /// An asset account, such as a house or other expensive property.
    /// </summary>
    Asset,

    /// <summary>
    /// A liability account, such as a loan.
    /// </summary>
    Liability,

    /// <summary>
    /// A container for memorized transactions.
    /// </summary>
    Memorized,
}
