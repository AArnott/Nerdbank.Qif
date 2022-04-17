// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// An investment account.
/// </summary>
/// <param name="Name">The name of the account.</param>
public record InvestmentAccount(string Name) : Account(Name)
{
    /// <inheritdoc/>
    public override List<InvestmentTransaction> Transactions { get; } = new();

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

    /// <inheritdoc/>
    public virtual bool Equals(InvestmentAccount? other)
    {
        return (object)this == other || (base.Equals(other)
            && EqualityComparer<string>.Default.Equals(this.Type, other!.Type)
            && ByValueCollectionComparer<InvestmentTransaction>.Default.Equals(this.Transactions, other!.Transactions));
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return (((base.GetHashCode() * -1521134295)
            + EqualityComparer<string>.Default.GetHashCode(this.Type)) * -1521134295)
            + ByValueCollectionComparer<InvestmentTransaction>.Default.GetHashCode(this.Transactions);
    }
}
