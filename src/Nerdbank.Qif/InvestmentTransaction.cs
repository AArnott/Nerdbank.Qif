// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// An investment transaction.
/// </summary>
/// <param name="Date">The date of the transaction.</param>
public record InvestmentTransaction(DateTime Date) : Transaction(AccountType.Investment, Date)
{
    /// <summary>
    /// Gets the action.
    /// </summary>
    /// <value>Typically set to one of the constants declared on the <see cref="Actions"/> class.</value>
    public string? Action { get; init; }

    /// <summary>
    /// Gets the security.
    /// </summary>
    public string? Security { get; init; }

    /// <summary>
    /// Gets the price.
    /// </summary>
    public decimal? Price { get; init; }

    /// <summary>
    /// Gets the quantity.
    /// </summary>
    public decimal? Quantity { get; init; }

    /// <summary>
    /// Gets the transaction amount.
    /// </summary>
    public decimal? TransactionAmount { get; init; }

    /// <summary>
    /// Gets the commission.
    /// </summary>
    public decimal? Commission { get; init; }

    /// <summary>
    /// Gets the account for transfer.
    /// </summary>
    public string? AccountForTransfer { get; init; }

    /// <summary>
    /// Gets the amount transferred, if cash is moved between accounts.
    /// </summary>
    public decimal? AmountTransferred { get; init; }

    /// <summary>
    /// Contains the values typically found on the <see cref="Action"/> property.
    /// </summary>
    public static class Actions
    {
        /// <summary>Buy a security with cash in the account.</summary>
        public const string Buy = "Buy";

        /// <summary>Buy a security with cash transferred from another account.</summary>
        public const string BuyX = "BuyX";

        /// <summary>Sell a security with proceeds received in the account.</summary>
        public const string Sell = "Sell";

        /// <summary>Sell a security and transfer the proceeds to another account.</summary>
        public const string SellX = "SellX";

        /// <summary>Long-term capital gains distribution received in the account.</summary>
        public const string CGLong = "CGLong";

        /// <summary>Long-term capital gains distribution transferred to another account.</summary>
        public const string CGLongX = "CGLongX";

        /// <summary>Medium-term capital gains distribution received in the account.</summary>
        public const string CGMid = "CGMid";

        /// <summary>Medium-term capital gains distribution transferred to another account.</summary>
        public const string CGMidX = "CGMidX";

        /// <summary>Short-term capital gains distribution received in the account.</summary>
        public const string CGShort = "CGShort";

        /// <summary>Short-term capital gains transferred to another account.</summary>
        public const string CGShortX = "CGShortX";

        /// <summary>Dividend received in the account.</summary>
        public const string Div = "Div";

        /// <summary>Dividend transferred to another account.</summary>
        public const string DivX = "DivX";

        /// <summary>Interest Income received in the account.</summary>
        public const string IntInc = "IntInc";

        /// <summary>Interest Income transferred to another account.</summary>
        public const string IntIncX = "IntIncX";

        /// <summary>Dividend reinvested in additional shares of the security.</summary>
        public const string ReinvDiv = "ReinvDiv";

        /// <summary>Interest Income reinvested in additional shares of the security.</summary>
        public const string ReinvInt = "ReinvInt";

        /// <summary>Long-term capital gains reinvested in additional shares of the security.</summary>
        public const string ReinvLg = "ReinvLg";

        /// <summary>Medium-term capital gains reinvested in additional shares of the security.</summary>
        public const string ReinvMd = "ReinvMd";

        /// <summary>Short-term capital gains reinvested in additional shares of the security.</summary>
        public const string ReinvSh = "ReinvSh";

        /// <summary>Reprice employee stock options.</summary>
        public const string Reprice = "Reprice";

        /// <summary>Cash transferred into the account.</summary>
        public const string XIn = "XIn";

        /// <summary>Cash transferred out of the account.</summary>
        public const string XOut = "XOut";

        /// <summary>Miscellaneous expense.</summary>
        public const string MiscExp = "MiscExp";

        /// <summary>Miscellaneous expense covered by another account.</summary>
        public const string MiscExpX = "MiscExpX";

        /// <summary>Miscellaneous income, optionally associated with a security.</summary>
        public const string MiscInc = "MiscInc";

        /// <summary>Miscellaneous income, optionally associated with a security, transferred to another account.</summary>
        public const string MiscIncX = "MiscIncX";

        /// <summary>Interest paid on a margin loan received in the account.</summary>
        public const string MargInt = "MargInt";

        /// <summary>Interest paid on a margin loan transferred from another account.</summary>
        public const string MargIntX = "MargIntX";

        /// <summary>Return of capital received in the account.</summary>
        public const string RtrnCap = "RtrnCap";

        /// <summary>Return of capital transferred to another account.</summary>
        public const string RtrnCapX = "RtrnCapX";

        /// <summary>Change in the number of shares as a result of a stock split..</summary>
        public const string StkSplit = "StkSplit";

        /// <summary>Removal of shares from an account.</summary>
        public const string ShrsOut = "ShrsOut";

        /// <summary>Addition of shares into an account.</summary>
        public const string ShrsIn = "ShrsIn";
    }

    /// <summary>
    /// The names of each field that may appear in this record.
    /// </summary>
    internal static class FieldNames
    {
        internal const string Date = "D";
        internal const string Action = "N";
        internal const string Security = "Y";
        internal const string Price = "I";
        internal const string Quantity = "Q";
        internal const string TransactionAmount = "T";
        internal const string TransactionAmount2 = "U";
        internal const string ClearedStatus = "C";
        internal const string Payee = "P";
        internal const string Memo = "M";
        internal const string Commission = "O";
        internal const string AccountForTransfer = "L";
        internal const string AmountTransferred = "$";
        internal const string EndOfEntry = "^";
    }
}
