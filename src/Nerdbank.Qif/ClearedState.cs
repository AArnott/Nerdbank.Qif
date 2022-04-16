// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// Enumerates the reconciliation states of a <see cref="BankTransaction"/>.
/// </summary>
public enum ClearedState
{
    /// <summary>
    /// The transaction has not been matched with the financial institution's records.
    /// </summary>
    None,

    /// <summary>
    /// The transaction has been matched with the financial institution's records.
    /// </summary>
    Cleared,

    /// <summary>
    /// The transaction has been matched with the financial institution's records
    /// and reconciled as a group with other transactions.
    /// </summary>
    Reconciled,
}
