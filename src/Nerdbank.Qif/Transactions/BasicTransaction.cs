// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Nerdbank.Qif;

/// <summary>
/// A basic transaction. It is used to describe non-investment transactions.
/// </summary>
public class BasicTransaction : TransactionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BasicTransaction"/> class.
    /// </summary>
    public BasicTransaction()
    {
        this.ClearedStatus = string.Empty;
        this.Number = string.Empty;
        this.Payee = string.Empty;
        this.Memo = string.Empty;
        this.Category = string.Empty;
        this.Address = new SortedList<int, string>();
        this.SplitCategories = new SortedList<int, string>();
        this.SplitMemos = new SortedList<int, string>();
        this.SplitAmounts = new SortedList<int, decimal>();
    }

    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    /// <value>The date.</value>
    public DateTime Date
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    /// <value>The amount.</value>
    public decimal Amount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the cleared status.
    /// </summary>
    /// <value>The cleared status.</value>
    public string ClearedStatus
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number.
    /// </summary>
    /// <value>The number.</value>
    public string Number
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the payee.
    /// </summary>
    /// <value>The payee.</value>
    public string Payee
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the memo.
    /// </summary>
    /// <value>The memo.</value>
    public string Memo
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    /// <value>The category.</value>
    public string Category
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the address.
    /// </summary>
    /// <value>The address.</value>
    public SortedList<int, string> Address
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the split categories.
    /// </summary>
    /// <value>The split categories.</value>
    public SortedList<int, string> SplitCategories
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the split memos.
    /// </summary>
    /// <value>The split memos.</value>
    public SortedList<int, string> SplitMemos
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the split amounts.
    /// </summary>
    /// <value>The split amounts.</value>
    public SortedList<int, decimal> SplitAmounts
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the account this transaction belongs to.
    /// </summary>
    /// <remarks>
    /// Not actually part of the file - inferred from its position in the file.
    /// </remarks>
    public string? AccountName { get; set; }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString()
    {
        return string.Format(Strings.Culture, Strings.BasicTransactionDisplay, this.Date.ToString("d", CultureInfo.CurrentCulture), this.Payee, this.Amount.ToString("C2", CultureInfo.CurrentCulture));
    }
}
