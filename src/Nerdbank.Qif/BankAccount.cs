// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static Nerdbank.Qif.Account;

namespace Nerdbank.Qif;

/// <summary>
/// A non-investment account (e.g. bank, cash, credit card, asset or loan).
/// </summary>
/// <param name="Type">The type of the account. Typically one of the values found in the <see cref="Types"/> class. This should <em>not</em> be <see cref="Types.Investment"/> as that should lead to creation of an <see cref="InvestmentAccount"/>.</param>
/// <param name="Name">The name of the account.</param>
public record BankAccount(string Type, string Name) : Account(Name)
{
    /// <inheritdoc/>
    public override List<BankTransaction> Transactions { get; } = new();

    /// <inheritdoc/>
    public override AccountType? AccountType => this.Type is null ? null : QifSerializer.GetAccountTypeFromString(this.Type);

    /// <inheritdoc/>
    public virtual bool Equals(BankAccount? other)
    {
        return (object)this == other || (base.Equals(other)
            && EqualityComparer<string>.Default.Equals(this.Type, other!.Type)
            && ByValueCollectionComparer<BankTransaction>.Default.Equals(this.Transactions, other!.Transactions));
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return (((base.GetHashCode() * -1521134295)
            + EqualityComparer<string>.Default.GetHashCode(this.Type)) * -1521134295)
            + ByValueCollectionComparer<BankTransaction>.Default.GetHashCode(this.Transactions);
    }
}
