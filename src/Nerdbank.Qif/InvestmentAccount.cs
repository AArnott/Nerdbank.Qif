// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// An investment account.
/// </summary>
/// <param name="Name">The name of the account.</param>
public record InvestmentAccount(string Name) : Account(Name)
{
    /// <summary>
    /// Gets a collection of investment transactions that belong to this account.
    /// </summary>
    public List<InvestmentTransaction> Transactions { get; } = new();

    /// <inheritdoc/>
    public override AccountType? AccountType => Qif.AccountType.Investment;

    /// <inheritdoc/>
    public override string Type
    {
        get => Types.Investment;
        init
        {
            if (value != Types.Investment)
            {
                throw new ArgumentException();
            }
        }
    }
}
