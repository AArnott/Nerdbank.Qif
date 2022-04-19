// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// The base type for an account.
/// </summary>
/// <param name="Type">The type of the account.</param>
/// <param name="Name">The name of the account.</param>
/// <remarks>
/// Supported derived types are <see cref="BankAccount"/> and <see cref="InvestmentAccount"/>.
/// </remarks>
public abstract record Account(string Type, string Name)
{
    /// <summary>
    /// Gets the description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the credit limit.
    /// </summary>
    public decimal? CreditLimit { get; init; }

    /// <summary>
    /// Gets the statement balance date.
    /// </summary>
    public DateTime? StatementBalanceDate { get; init; }

    /// <summary>
    /// Gets the statement balance.
    /// </summary>
    public decimal? StatementBalance { get; init; }

    /// <summary>
    /// Gets the account type.
    /// </summary>
    public abstract AccountType? AccountType { get; }

    /// <summary>
    /// Gets the list of transactions in this account.
    /// </summary>
    public abstract IReadOnlyList<Transaction> Transactions { get; }

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
        /// An investment account.
        /// </summary>
        public const string Investment2 = "Port";

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
