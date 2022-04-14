// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static Nerdbank.Qif.QifUtilities;

namespace Nerdbank.Qif;

/// <summary>
/// An account.
/// </summary>
/// <param name="Name">The name of the account.</param>
public record Account(string Name)
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the credit limit.
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Gets or sets the statement balance date.
    /// </summary>
    public DateTime? StatementBalanceDate { get; set; }

    /// <summary>
    /// Gets or sets the statement balance.
    /// </summary>
    public decimal? StatementBalance { get; set; }

    /// <summary>
    /// Deserializes a <see cref="Account"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    public static Account Load(QifReader reader)
    {
        string? name = null;
        string? type = null;
        string? description = null;
        decimal? creditLimit = null;
        DateTime? statementBalanceDate = null;
        decimal? statementBalance = null;
        while (reader.TryReadField(out ReadOnlyMemory<char> fieldName, out _))
        {
            if (QifUtilities.Equals(FieldNames.Name, fieldName))
            {
                name = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.Type, fieldName))
            {
                type = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.Description, fieldName))
            {
                description = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(FieldNames.CreditLimit, fieldName))
            {
                creditLimit = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(FieldNames.StatementBalanceDate, fieldName))
            {
                statementBalanceDate = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(FieldNames.StatementBalance, fieldName))
            {
                statementBalance = reader.ReadFieldAsDecimal();
            }
        }

        reader.ReadEndOfRecord();

        return new(ValueOrThrow(name, FieldNames.Name))
        {
            Type = type,
            Description = description,
            CreditLimit = creditLimit,
            StatementBalanceDate = statementBalanceDate,
            StatementBalance = statementBalance,
        };
    }

    private static class FieldNames
    {
        internal const string Name = "N";
        internal const string Type = "T";
        internal const string Description = "D";
        internal const string CreditLimit = "L";
        internal const string StatementBalanceDate = "/";
        internal const string StatementBalance = "$";
    }
}
