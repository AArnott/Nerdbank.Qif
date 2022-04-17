// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    /// <value>Typically one of the values found in the <see cref="Types"/> class.</value>
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

    /// <summary>
    /// Well-known values for the <see cref="Type"/> property.
    /// </summary>
    public static class Types
    {
        /// <summary>
        /// A typical bank account.
        /// </summary>
        public const string Bank = "Bank";

        /// <summary>
        /// An asset account, such as a house or other expensive property.
        /// </summary>
        public const string Asset = "Oth A";

        /// <summary>
        /// A liability account, such as a loan.
        /// </summary>
        public const string Liability = "Oth L";

        /// <summary>
        /// A cash account.
        /// </summary>
        public const string Cash = "Cash";

        /// <summary>
        /// A credit card account.
        /// </summary>
        public const string CreditCard = "CCard";

        /// <summary>
        /// An investment account.
        /// </summary>
        public const string Investment = "Invst";

        /// <summary>
        /// A container for memorized transactions.
        /// </summary>
        public const string Memorized = "Memorized";
    }

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
