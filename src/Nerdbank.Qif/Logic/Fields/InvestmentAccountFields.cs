// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif.Logic;

/// <summary>
/// The investment account fields used in transactions.
/// </summary>
internal static class InvestmentAccountFields
{
    /// <summary>
    /// Date.
    /// </summary>
    internal const string Date = "D";

    /// <summary>
    /// Action.
    /// </summary>
    internal const string Action = "N";

    /// <summary>
    /// Security.
    /// </summary>
    internal const string Security = "Y";

    /// <summary>
    /// Price.
    /// </summary>
    internal const string Price = "I";

    /// <summary>
    /// Number of shares or split ratio.
    /// </summary>
    internal const string Quantity = "Q";

    /// <summary>
    /// Transaction amount.
    /// </summary>
    internal const string TransactionAmount = "T";

    /// <summary>
    /// Cleared status.
    /// </summary>
    internal const string ClearedStatus = "C";

    /// <summary>
    /// Text in the first line for transfers and reminders.
    /// </summary>
    internal const string TextFirstLine = "P";

    /// <summary>
    /// Memo.
    /// </summary>
    internal const string Memo = "M";

    /// <summary>
    /// Commission.
    /// </summary>
    internal const string Commission = "O";

    /// <summary>
    /// Account for the transfer.
    /// </summary>
    internal const string AccountForTransfer = "L";

    /// <summary>
    /// Amount transferred.
    /// </summary>
    internal const string AmountTransferred = "$";

    /// <summary>
    /// End of entry.
    /// </summary>
    internal const string EndOfEntry = "^";
}
