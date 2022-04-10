// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Nerdbank.Qif;

/// <summary>
/// Used to describe an invalid transaction.
/// </summary>
[Serializable]
public class InvalidTransactionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTransactionException"/> class.
    /// </summary>
    public InvalidTransactionException()
        : this(null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTransactionException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public InvalidTransactionException(string? message)
        : this(message, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTransactionException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidTransactionException(string? message, Exception innerException)
        : this(message, innerException, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTransactionException"/> class.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public InvalidTransactionException(TransactionBase transaction)
        : this(string.Format(CultureInfo.CurrentCulture, Strings.InvalidBankTransaction, transaction), transaction)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTransactionException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="transaction">The transaction.</param>
    public InvalidTransactionException(string message, TransactionBase transaction)
        : this(message, null, transaction)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTransactionException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="transaction">The transaction.</param>
    public InvalidTransactionException(string? message, Exception? innerException, TransactionBase? transaction)
        : base(message, innerException)
    {
        this.Transaction = transaction;
    }

    /// <summary>
    /// Gets the transaction.
    /// </summary>
    /// <value>The transaction.</value>
    public TransactionBase? Transaction
    {
        get;
        private set;
    }
}
