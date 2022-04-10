// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Nerdbank.Qif;

/// <summary>
/// An investment transaction.
/// </summary>
public class InvestmentTransaction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvestmentTransaction"/> class.
    /// </summary>
    public InvestmentTransaction()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvestmentTransaction"/> class
    /// using values from an existing instance.
    /// </summary>
    /// <param name="template">The object to copy from.</param>
    public InvestmentTransaction(InvestmentTransaction template)
    {
        this.AccountForTransfer = template.AccountForTransfer;
        this.Action = template.Action;
        this.AmountTransferred = template.AmountTransferred;
        this.ClearedStatus = template.ClearedStatus;
        this.Commission = template.Commission;
        this.Date = template.Date;
        this.Memo = template.Memo;
        this.Price = template.Price;
        this.Quantity = template.Quantity;
        this.Security = template.Security;
        this.TextFirstLine = template.TextFirstLine;
        this.TransactionAmount = template.TransactionAmount;
    }

    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    /// <value>The date.</value>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the action.
    /// </summary>
    /// <value>The action.</value>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the security.
    /// </summary>
    /// <value>The security.</value>
    public string Security { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    /// <value>The price.</value>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    /// <value>The quantity.</value>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount.
    /// </summary>
    /// <value>The transaction amount.</value>
    public decimal TransactionAmount { get; set; }

    /// <summary>
    /// Gets or sets the cleared status.
    /// </summary>
    /// <value>The cleared status.</value>
    public string ClearedStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text first line.
    /// </summary>
    /// <value>The text first line.</value>
    public string TextFirstLine { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the memo.
    /// </summary>
    /// <value>The memo.</value>
    public string Memo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the commission.
    /// </summary>
    /// <value>The commission.</value>
    public decimal Commission { get; set; }

    /// <summary>
    /// Gets or sets the account for transfer.
    /// </summary>
    /// <value>The account for transfer.</value>
    public string AccountForTransfer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount transferred.
    /// </summary>
    /// <value>The amount transferred.</value>
    public decimal AmountTransferred { get; set; }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString()
    {
        return string.Format(Strings.Culture, Strings.InvestmentTransactionDisplay, this.Date.ToString("d", CultureInfo.CurrentCulture), this.TextFirstLine, this.TransactionAmount.ToString("C2", CultureInfo.CurrentCulture));
    }
}
