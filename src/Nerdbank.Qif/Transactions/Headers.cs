// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// The transaction type headers.
/// </summary>
internal static class Headers
{
    /// <summary>
    /// Bank account transactions.
    /// </summary>
    internal const string Bank = "!Type:Bank";

    /// <summary>
    /// Cash account transactions.
    /// </summary>
    internal const string Cash = "!Type:Cash";

    /// <summary>
    /// Credit card account transactions.
    /// </summary>
    internal const string CreditCard = "!Type:CCard";

    /// <summary>
    /// Investment account transactions.
    /// </summary>
    internal const string Investment = "!Type:Invst";

    /// <summary>
    /// Asset account transactions.
    /// </summary>
    internal const string Asset = "!Type:Oth A";

    /// <summary>
    /// Liability account transactions.
    /// </summary>
    internal const string Liability = "!Type:Oth L";

    /// <summary>
    /// Account list or which account follows.
    /// </summary>
    internal const string AccountList = "!Account";

    /// <summary>
    /// Category list.
    /// </summary>
    internal const string CategoryList = "!Type:Cat";

    /// <summary>
    /// Class list.
    /// </summary>
    internal const string ClassList = "!Type:Class";

    /// <summary>
    /// Memorized transaction list.
    /// </summary>
    internal const string MemorizedTransactionList = "!Type:Memorized";
}
