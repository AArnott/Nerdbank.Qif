// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// Memorized transaction types.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// A check transaction type.
    /// </summary>
    Check,

    /// <summary>
    /// A deposit transaction type.
    /// </summary>
    Deposit,

    /// <summary>
    /// A payment transaction type.
    /// </summary>
    Payment,

    /// <summary>
    /// An investment transaction type.
    /// </summary>
    Investment,

    /// <summary>
    /// An electronic payee transaction type.
    /// </summary>
    ElectronicPayee,
}
