// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif.Logic;

/// <summary>
/// The non-investment account fields used in transactions.
/// </summary>
internal static class NonInvestmentAccountFields
{
    /// <summary>
    /// Date.
    /// </summary>
    internal const string Date = "D";

    /// <summary>
    /// Amount.
    /// </summary>
    internal const string Amount = "T";

    /// <summary>
    /// Cleared status.
    /// </summary>
    internal const string ClearedStatus = "C";

    /// <summary>
    /// Check, reference number, or transaction type.
    /// </summary>
    internal const string Number = "N";

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
    /// Category/Subcategory/Transfer/Class.
    /// </summary>
    internal const string Category = "L";

    /// <summary>
    /// Category/Transfer/Class.
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
    /// End of entry.
    /// </summary>
    internal const string EndOfEntry = "^";
}
