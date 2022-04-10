// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// An account list transaction. This is used to describe an account.
/// </summary>
public class AccountListTransaction : TransactionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountListTransaction"/> class.
    /// </summary>
    public AccountListTransaction()
    {
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>The type.</value>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credit limit.
    /// </summary>
    /// <value>The credit limit.</value>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Gets or sets the statement balance date.
    /// </summary>
    /// <value>The statement balance date.</value>
    public DateTime StatementBalanceDate { get; set; }

    /// <summary>
    /// Gets or sets the statement balance.
    /// </summary>
    /// <value>The statement balance.</value>
    public decimal StatementBalance { get; set; }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString()
    {
        return this.Name;
    }
}
