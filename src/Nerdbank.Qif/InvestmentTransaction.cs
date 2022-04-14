// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static Nerdbank.Qif.QifUtilities;

namespace Nerdbank.Qif;

/// <summary>
/// An investment transaction.
/// </summary>
/// <param name="Date">The date of the transaction.</param>
public record InvestmentTransaction(DateTime Date)
{
    /// <summary>
    /// Gets the action.
    /// </summary>
    public string? Action { get; init; }

    /// <summary>
    /// Gets the security.
    /// </summary>
    public string? Security { get; init; }

    /// <summary>
    /// Gets the price.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Gets the quantity.
    /// </summary>
    public decimal Quantity { get; init; }

    /// <summary>
    /// Gets the transaction amount.
    /// </summary>
    public decimal TransactionAmount { get; init; }

    /// <summary>
    /// Gets the cleared status.
    /// </summary>
    public ClearedState ClearedStatus { get; init; }

    /// <summary>
    /// Gets the payee, or a description for deposits, transfers, etc.
    /// </summary>
    public string? Payee { get; init; }

    /// <summary>
    /// Gets the memo.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    /// Gets the commission.
    /// </summary>
    public decimal Commission { get; init; }

    /// <summary>
    /// Gets the account for transfer.
    /// </summary>
    public string? AccountForTransfer { get; init; }

    /// <summary>
    /// Gets the amount transferred, if cash is moved between accounts.
    /// </summary>
    public decimal AmountTransferred { get; init; }

    /// <summary>
    /// Deserializes a <see cref="BankTransaction"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    public static InvestmentTransaction Load(QifReader reader)
    {
        DateTime? date = null;
        ClearedState clearedStatus = ClearedState.None;
        string? memo = null;
        string? action = null;
        decimal commission = 0;
        decimal price = 0;
        decimal quantity = 0;
        string? security = null;
        string? payee = null;
        decimal transactionAmount = 0;
        string? accountForTransfer = null;
        decimal amountTransferred = 0;
        while (reader.TryReadField(out ReadOnlyMemory<char> fieldName, out _))
        {
            if (QifUtilities.Equals(FieldNames.Date, fieldName))
            {
                date = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(FieldNames.ClearedStatus, fieldName))
            {
                clearedStatus = reader.ReadFieldAsClearedState();
            }
            else if (QifUtilities.Equals(FieldNames.Memo, fieldName))
            {
                memo = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.Action, fieldName))
            {
                action = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.Commission, fieldName))
            {
                commission = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(FieldNames.Price, fieldName))
            {
                price = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(FieldNames.Quantity, fieldName))
            {
                quantity = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(FieldNames.Security, fieldName))
            {
                security = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.Payee, fieldName))
            {
                payee = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.TransactionAmount, fieldName))
            {
                transactionAmount = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(FieldNames.AccountForTransfer, fieldName))
            {
                accountForTransfer = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.AmountTransferred, fieldName))
            {
                amountTransferred = reader.ReadFieldAsDecimal();
            }
        }

        reader.ReadEndOfRecord();

        return new(
            ValueOrThrow(date, FieldNames.Date))
        {
            ClearedStatus = clearedStatus,
            Memo = memo,
            Action = action,
            Commission = commission,
            Price = price,
            Quantity = quantity,
            Security = security,
            Payee = payee,
            TransactionAmount = transactionAmount,
            AccountForTransfer = accountForTransfer,
            AmountTransferred = amountTransferred,
        };
    }

    /// <summary>
    /// The names of each field that may appear in this record.
    /// </summary>
    private static class FieldNames
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
