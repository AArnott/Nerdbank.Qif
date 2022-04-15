// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static Nerdbank.Qif.QifUtilities;

namespace Nerdbank.Qif;

/// <summary>
/// An account.
/// </summary>
/// <param name="Name">The name of the account.</param>
public record Account(string Name)
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the credit limit.
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Gets or sets the statement balance date.
    /// </summary>
    public DateTime? StatementBalanceDate { get; set; }

    /// <summary>
    /// Gets or sets the statement balance.
    /// </summary>
    public decimal? StatementBalance { get; set; }

    internal static class FieldNames
    {
        internal const string Name = "N";
        internal const string Type = "T";
        internal const string Description = "D";
        internal const string CreditLimit = "L";
        internal const string StatementBalanceDate = "/";
        internal const string StatementBalance = "$";
    }
}
