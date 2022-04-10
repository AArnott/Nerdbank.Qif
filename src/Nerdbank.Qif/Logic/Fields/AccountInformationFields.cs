// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif.Logic;

/// <summary>
/// The account information fields used in transactions.
/// </summary>
internal static class AccountInformationFields
{
    /// <summary>
    /// Name.
    /// </summary>
    internal const string Name = "N";

    /// <summary>
    /// Type of account.
    /// </summary>
    internal const string AccountType = "T";

    /// <summary>
    /// Description.
    /// </summary>
    internal const string Description = "D";

    /// <summary>
    /// Only for credit card account.
    /// </summary>
    internal const string CreditLimit = "L";

    /// <summary>
    /// Statement balance date.
    /// </summary>
    internal const string StatementBalanceDate = "/";

    /// <summary>
    /// Statement balance.
    /// </summary>
    internal const string StatementBalance = "$";

    /// <summary>
    /// End of entry.
    /// </summary>
    internal const string EndOfEntry = "^";
}
