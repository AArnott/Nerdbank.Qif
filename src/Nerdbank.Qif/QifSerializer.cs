// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static Nerdbank.Qif.QifUtilities;

namespace Nerdbank.Qif;

/// <summary>
/// Serializes and deserializes QIF records.
/// </summary>
public class QifSerializer
{
    internal static readonly QifSerializer Default = new();

    /// <summary>
    /// Writes an entire <see cref="QifDocument"/>.
    /// </summary>
    /// <param name="writer">The writer to serialize to.</param>
    /// <param name="value">The document to be serialized.</param>
    public virtual void Write(QifWriter writer, QifDocument value)
    {
        WriteRecord("Account", null, value.Accounts, this.Write);
        WriteRecord("Type", "Bank", value.BankTransactions, this.Write);
        WriteRecord("Type", "Oth A", value.AssetTransactions, this.Write);
        WriteRecord("Type", "Oth L", value.LiabilityTransactions, this.Write);
        WriteRecord("Type", "Cat", value.Categories, this.Write);
        // More here

        void WriteRecord<T>(string headerName, string? headerValue, IReadOnlyCollection<T> records, Action<QifWriter, T> recordWriter)
        {
            if (records.Count > 0)
            {
                writer.WriteHeader(headerName, headerValue);
                foreach (T record in records)
                {
                    recordWriter(writer, record);
                }
            }
        }
    }

    /// <summary>
    /// Reads an entire QIF file.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="QifDocument"/>.</returns>
    public virtual QifDocument ReadDocument(QifReader reader)
    {
        Requires.NotNull(reader, nameof(reader));
        try
        {
            QifDocument result = new();

            // Remember the last account we saw so we can link its transactions to it.
            Account? lastAccountRead = null;

            while (reader.TryReadHeader(out ReadOnlyMemory<char> headerName, out ReadOnlyMemory<char> headerValue))
            {
                if (QifUtilities.Equals("Type", headerName))
                {
                    if (QifUtilities.Equals("Bank", headerValue))
                    {
                        do
                        {
                            result.BankTransactions.Add(this.ReadBankTransaction(reader) with { AccountName = lastAccountRead?.Name });
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Cash", headerValue))
                    {
                        do
                        {
                            result.CashTransactions.Add(this.ReadBankTransaction(reader) with { AccountName = lastAccountRead?.Name });
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("CCard", headerValue))
                    {
                        do
                        {
                            result.CreditCardTransactions.Add(this.ReadBankTransaction(reader) with { AccountName = lastAccountRead?.Name });
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Oth A", headerValue))
                    {
                        do
                        {
                            result.AssetTransactions.Add(this.ReadBankTransaction(reader) with { AccountName = lastAccountRead?.Name });
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Oth L", headerValue))
                    {
                        do
                        {
                            result.LiabilityTransactions.Add(this.ReadBankTransaction(reader) with { AccountName = lastAccountRead?.Name });
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Cat", headerValue))
                    {
                        do
                        {
                            result.Categories.Add(this.ReadCategory(reader));
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Class", headerValue))
                    {
                        do
                        {
                            result.Classes.Add(this.ReadClass(reader));
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Invst", headerValue))
                    {
                        do
                        {
                            result.InvestmentTransactions.Add(this.ReadInvestmentTransaction(reader));
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Memorized", headerValue))
                    {
                        do
                        {
                            result.MemorizedTransactions.Add(this.ReadMemorizedTransaction(reader));
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else
                    {
                        // We don't recognize this header, so skip its entire content.
                        reader.TrySkipToToken(QifParser.TokenKind.Header);
                    }
                }
                else if (QifUtilities.Equals("Account", headerName))
                {
                    Account account = this.ReadAccount(reader);
                    lastAccountRead = account;
                    result.Accounts.Add(account);
                }
                else
                {
                    // We don't recognize this header, so skip its entire content.
                    reader.TrySkipToToken(QifParser.TokenKind.Header);
                }
            }

            return result;
        }
        finally
        {
            reader.Dispose();
        }
    }

    /// <summary>
    /// Saves a record in QIF format.
    /// </summary>
    /// <param name="writer">The writer to serialize the record to.</param>
    /// <param name="value">The record to serialize.</param>
    public virtual void Write(QifWriter writer, Class value)
    {
        writer.WriteField(Class.FieldNames.Name, value.Name);
        writer.WriteFieldIfNotEmpty(Class.FieldNames.Description, value.Description);
        writer.WriteEndOfRecord();
    }

    /// <summary>
    /// Deserializes a <see cref="Class"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    public virtual Class ReadClass(QifReader reader)
    {
        string? name = null;
        string? description = null;
        while (reader.TryReadField(out ReadOnlyMemory<char> fieldName, out _))
        {
            if (QifUtilities.Equals(Class.FieldNames.Name, fieldName))
            {
                name = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Class.FieldNames.Description, fieldName))
            {
                description = reader.ReadFieldAsString();
            }
        }

        return new(ValueOrThrow(name, Class.FieldNames.Name))
        {
            Description = description,
        };
    }

    /// <inheritdoc cref="Write(QifWriter, Class)" />
    public virtual void Write(QifWriter writer, BankTransaction value)
    {
        WriteBankTransactionHelper(writer, value);
        writer.WriteEndOfRecord();
    }

    /// <summary>
    /// Deserializes a <see cref="BankTransaction"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    /// <devremarks>
    /// Keep this in close sync with <see cref="ReadMemorizedTransaction(QifReader)"/>.
    /// </devremarks>
    public virtual BankTransaction ReadBankTransaction(QifReader reader)
    {
        DateTime? date = null;
        decimal? amount = null;
        ClearedState clearedStatus = ClearedState.None;
        string? number = null;
        string? payee = null;
        string? memo = null;
        string? category = null;
        ImmutableList<string> address = ImmutableList<string>.Empty;
        ImmutableList<string> splitCategories = ImmutableList<string>.Empty;
        ImmutableList<string> splitMemos = ImmutableList<string>.Empty;
        ImmutableList<decimal> splitAmounts = ImmutableList<decimal>.Empty;
        ImmutableList<decimal> splitPercentage = ImmutableList<decimal>.Empty;
        while (reader.TryReadField(out ReadOnlyMemory<char> fieldName, out _))
        {
            if (QifUtilities.Equals(BankTransaction.FieldNames.Date, fieldName))
            {
                date = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Amount, fieldName))
            {
                amount = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.ClearedStatus, fieldName))
            {
                clearedStatus = reader.ReadFieldAsClearedState();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Number, fieldName))
            {
                number = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Payee, fieldName))
            {
                payee = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Memo, fieldName))
            {
                memo = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Category, fieldName))
            {
                category = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Address, fieldName))
            {
                address = address.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.SplitCategory, fieldName))
            {
                splitCategories = splitCategories.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.SplitMemo, fieldName))
            {
                splitMemos = splitMemos.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.SplitAmount, fieldName))
            {
                splitAmounts = splitAmounts.Add(reader.ReadFieldAsDecimal());
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.SplitPercent, fieldName))
            {
                splitPercentage = splitPercentage.Add(reader.ReadFieldAsDecimal());
            }
        }

        if (splitCategories.Count != splitMemos.Count ||
            splitCategories.Count != Math.Max(splitAmounts.Count, splitPercentage.Count))
        {
            throw new InvalidTransactionException("Inconsistent number of fields for splits.");
        }

        ImmutableList<BankSplit> splits = ImmutableList<BankSplit>.Empty;
        if (splitCategories.Count > 0)
        {
            var splitsBuilder = splits.ToBuilder();
            for (int i = 0; i < splitCategories.Count; i++)
            {
                BankSplit split = new(splitCategories[i], splitMemos[i])
                {
                    Amount = splitAmounts.Count > i ? splitAmounts[i] : null,
                    Percentage = splitPercentage.Count > i ? splitPercentage[i] : null,
                };
                splitsBuilder.Add(split);
            }

            splits = splitsBuilder.ToImmutable();
        }

        reader.ReadEndOfRecord();

        return new(
            ValueOrThrow(date, BankTransaction.FieldNames.Date),
            ValueOrThrow(amount, BankTransaction.FieldNames.Amount))
        {
            ClearedStatus = clearedStatus,
            Number = number,
            Payee = payee,
            Memo = memo,
            Category = category,
            Address = address,
            Splits = splits,
        };
    }

    /// <inheritdoc cref="Write(QifWriter, Class)" />
    public virtual void Write(QifWriter writer, MemorizedTransaction value)
    {
        char typeCode = value.Type switch
        {
            MemorizedTransactionType.ElectronicPayee => MemorizedTransaction.TransactionTypeCodes.ElectronicPayee,
            MemorizedTransactionType.Deposit => MemorizedTransaction.TransactionTypeCodes.Deposit,
            MemorizedTransactionType.Payment => MemorizedTransaction.TransactionTypeCodes.Payment,
            MemorizedTransactionType.Investment => MemorizedTransaction.TransactionTypeCodes.Investment,
            MemorizedTransactionType.Check => MemorizedTransaction.TransactionTypeCodes.Check,
            _ => throw new InvalidTransactionException("Unsupported type."),
        };
        writer.WriteField(MemorizedTransaction.FieldNames.Type, typeCode);
        WriteBankTransactionHelper(writer, value);
        writer.WriteEndOfRecord();
    }

    /// <summary>
    /// Deserializes a <see cref="MemorizedTransaction"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    /// <devremarks>
    /// Keep this in close sync with <see cref="ReadBankTransaction(QifReader)"/>.
    /// </devremarks>
    public virtual MemorizedTransaction ReadMemorizedTransaction(QifReader reader)
    {
        MemorizedTransactionType? type = null;
        DateTime? date = null;
        decimal? amount = null;
        ClearedState clearedStatus = ClearedState.None;
        string? number = null;
        string? payee = null;
        string? memo = null;
        string? category = null;
        ImmutableList<string> address = ImmutableList<string>.Empty;
        ImmutableList<string> splitCategories = ImmutableList<string>.Empty;
        ImmutableList<string> splitMemos = ImmutableList<string>.Empty;
        ImmutableList<decimal> splitAmounts = ImmutableList<decimal>.Empty;
        ImmutableList<decimal> splitPercentage = ImmutableList<decimal>.Empty;
        while (reader.TryReadField(out ReadOnlyMemory<char> fieldName, out _))
        {
            if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Date, fieldName))
            {
                date = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Amount, fieldName))
            {
                amount = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.ClearedStatus, fieldName))
            {
                clearedStatus = reader.ReadFieldAsClearedState();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Number, fieldName))
            {
                number = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Payee, fieldName))
            {
                payee = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Memo, fieldName))
            {
                memo = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Category, fieldName))
            {
                category = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Address, fieldName))
            {
                address = address.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.SplitCategory, fieldName))
            {
                splitCategories = splitCategories.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.SplitMemo, fieldName))
            {
                splitMemos = splitMemos.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.SplitAmount, fieldName))
            {
                splitAmounts = splitAmounts.Add(reader.ReadFieldAsDecimal());
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.SplitPercent, fieldName))
            {
                splitPercentage = splitPercentage.Add(reader.ReadFieldAsDecimal());
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Type, fieldName))
            {
                if (reader.Field.Span.Length != 1)
                {
                    throw new InvalidTransactionException("Unexpected length in exception type.");
                }

                type = reader.Field.Span[0] switch
                {
                    MemorizedTransaction.TransactionTypeCodes.Check => MemorizedTransactionType.Check,
                    MemorizedTransaction.TransactionTypeCodes.Deposit => MemorizedTransactionType.Deposit,
                    MemorizedTransaction.TransactionTypeCodes.Payment => MemorizedTransactionType.Payment,
                    MemorizedTransaction.TransactionTypeCodes.Investment => MemorizedTransactionType.Investment,
                    MemorizedTransaction.TransactionTypeCodes.ElectronicPayee => MemorizedTransactionType.ElectronicPayee,
                    _ => throw new InvalidTransactionException("Unsupported memorized transaction type."),
                };
            }
        }

        if (splitCategories.Count != splitMemos.Count ||
            splitCategories.Count != Math.Max(splitAmounts.Count, splitPercentage.Count))
        {
            throw new InvalidTransactionException("Inconsistent number of fields for splits.");
        }

        ImmutableList<BankSplit> splits = ImmutableList<BankSplit>.Empty;
        if (splitCategories.Count > 0)
        {
            var splitsBuilder = splits.ToBuilder();
            for (int i = 0; i < splitCategories.Count; i++)
            {
                BankSplit split = new(splitCategories[i], splitMemos[i])
                {
                    Amount = splitAmounts.Count > i ? splitAmounts[i] : null,
                    Percentage = splitPercentage.Count > i ? splitPercentage[i] : null,
                };
                splitsBuilder.Add(split);
            }

            splits = splitsBuilder.ToImmutable();
        }

        reader.ReadEndOfRecord();

        return new(
            ValueOrThrow(type, MemorizedTransaction.FieldNames.Type),
            ValueOrThrow(date, MemorizedTransaction.FieldNames.Date),
            ValueOrThrow(amount, MemorizedTransaction.FieldNames.Amount))
        {
            ClearedStatus = clearedStatus,
            Number = number,
            Payee = payee,
            Memo = memo,
            Category = category,
            Address = address,
            Splits = splits,
        };
    }

    /// <inheritdoc cref="Write(QifWriter, Class)" />
    public virtual void Write(QifWriter writer, Category value)
    {
        writer.WriteField(Category.FieldNames.Name, value.Name);
        writer.WriteField(Category.FieldNames.TaxRelated, value.Description);
        writer.WriteFieldIf(Category.FieldNames.TaxRelated, value.TaxRelated);
        writer.WriteField(Category.FieldNames.TaxSchedule, value.TaxSchedule);
        writer.WriteFieldIf(Category.FieldNames.ExpenseCategory, value.ExpenseCategory);
        writer.WriteFieldIf(Category.FieldNames.IncomeCategory, value.IncomeCategory);
        writer.WriteField(Category.FieldNames.BudgetAmount, value.BudgetAmount);
        writer.WriteEndOfRecord();
    }

    /// <summary>
    /// Deserializes a <see cref="BankTransaction"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    public virtual Category ReadCategory(QifReader reader)
    {
        string? name = null;
        string? description = null;
        bool taxRelated = false;
        bool incomeCategory = false;
        bool expenseCategory = false;
        string? taxSchedule = null;
        decimal budgetAmount = 0;
        while (reader.TryReadField(out ReadOnlyMemory<char> fieldName, out _))
        {
            if (QifUtilities.Equals(Category.FieldNames.Name, fieldName))
            {
                name = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Category.FieldNames.Description, fieldName))
            {
                description = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Category.FieldNames.TaxSchedule, fieldName))
            {
                taxSchedule = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Category.FieldNames.TaxRelated, fieldName))
            {
                taxRelated = true;
            }
            else if (QifUtilities.Equals(Category.FieldNames.IncomeCategory, fieldName))
            {
                incomeCategory = true;
            }
            else if (QifUtilities.Equals(Category.FieldNames.ExpenseCategory, fieldName))
            {
                expenseCategory = true;
            }
            else if (QifUtilities.Equals(Category.FieldNames.BudgetAmount, fieldName))
            {
                budgetAmount = reader.ReadFieldAsDecimal();
            }
        }

        reader.ReadEndOfRecord();

        return new(ValueOrThrow(name, Category.FieldNames.Name))
        {
            Description = description,
            TaxRelated = taxRelated,
            IncomeCategory = incomeCategory,
            ExpenseCategory = expenseCategory,
            TaxSchedule = taxSchedule,
            BudgetAmount = budgetAmount,
        };
    }

    /// <inheritdoc cref="Write(QifWriter, Class)" />
    public virtual void Write(QifWriter writer, InvestmentTransaction value)
    {
        writer.WriteField(InvestmentTransaction.FieldNames.Date, value.Date);
        writer.WriteField(InvestmentTransaction.FieldNames.Action, value.Action);
        writer.WriteField(InvestmentTransaction.FieldNames.Payee, value.Payee);
        writer.WriteField(InvestmentTransaction.FieldNames.Memo, value.Memo);
        writer.WriteField(InvestmentTransaction.FieldNames.ClearedStatus, value.ClearedStatus);
        writer.WriteField(InvestmentTransaction.FieldNames.Quantity, value.Quantity);
        writer.WriteField(InvestmentTransaction.FieldNames.Security, value.Security);
        writer.WriteField(InvestmentTransaction.FieldNames.TransactionAmount, value.TransactionAmount);
        writer.WriteField(InvestmentTransaction.FieldNames.Price, value.Price);
        writer.WriteField(InvestmentTransaction.FieldNames.Commission, value.Commission);
        writer.WriteField(InvestmentTransaction.FieldNames.AmountTransferred, value.AmountTransferred);
        writer.WriteField(InvestmentTransaction.FieldNames.AccountForTransfer, value.AccountForTransfer);
        writer.WriteEndOfRecord();
    }

    /// <summary>
    /// Deserializes a <see cref="BankTransaction"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    public virtual InvestmentTransaction ReadInvestmentTransaction(QifReader reader)
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
            if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Date, fieldName))
            {
                date = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.ClearedStatus, fieldName))
            {
                clearedStatus = reader.ReadFieldAsClearedState();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Memo, fieldName))
            {
                memo = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Action, fieldName))
            {
                action = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Commission, fieldName))
            {
                commission = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Price, fieldName))
            {
                price = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Quantity, fieldName))
            {
                quantity = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Security, fieldName))
            {
                security = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Payee, fieldName))
            {
                payee = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.TransactionAmount, fieldName))
            {
                transactionAmount = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.AccountForTransfer, fieldName))
            {
                accountForTransfer = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.AmountTransferred, fieldName))
            {
                amountTransferred = reader.ReadFieldAsDecimal();
            }
        }

        reader.ReadEndOfRecord();

        return new(
            ValueOrThrow(date, InvestmentTransaction.FieldNames.Date))
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

    /// <inheritdoc cref="Write(QifWriter, Class)" />
    public virtual void Write(QifWriter writer, Account value)
    {
        writer.WriteField(Account.FieldNames.Name, value.Name);
        writer.WriteField(Account.FieldNames.Type, value.Type);
        writer.WriteField(Account.FieldNames.Description, value.Description);
        writer.WriteField(Account.FieldNames.CreditLimit, value.CreditLimit);
        writer.WriteField(Account.FieldNames.StatementBalanceDate, value.StatementBalanceDate);
        writer.WriteField(Account.FieldNames.StatementBalance, value.StatementBalance);
        writer.WriteEndOfRecord();
    }

    /// <summary>
    /// Deserializes a <see cref="Account"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    public virtual Account ReadAccount(QifReader reader)
    {
        string? name = null;
        string? type = null;
        string? description = null;
        decimal? creditLimit = null;
        DateTime? statementBalanceDate = null;
        decimal? statementBalance = null;
        while (reader.TryReadField(out ReadOnlyMemory<char> fieldName, out _))
        {
            if (QifUtilities.Equals(Account.FieldNames.Name, fieldName))
            {
                name = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Account.FieldNames.Type, fieldName))
            {
                type = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Account.FieldNames.Description, fieldName))
            {
                description = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Account.FieldNames.CreditLimit, fieldName))
            {
                creditLimit = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(Account.FieldNames.StatementBalanceDate, fieldName))
            {
                statementBalanceDate = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(Account.FieldNames.StatementBalance, fieldName))
            {
                statementBalance = reader.ReadFieldAsDecimal();
            }
        }

        reader.ReadEndOfRecord();

        return new(ValueOrThrow(name, Account.FieldNames.Name))
        {
            Type = type,
            Description = description,
            CreditLimit = creditLimit,
            StatementBalanceDate = statementBalanceDate,
            StatementBalance = statementBalance,
        };
    }

    private static void WriteBankTransactionHelper(QifWriter writer, BankTransaction value)
    {
        writer.WriteField(BankTransaction.FieldNames.Date, value.Date);
        writer.WriteField(BankTransaction.FieldNames.Amount, value.Amount);
        writer.WriteField(BankTransaction.FieldNames.Number, value.Number);
        writer.WriteField(BankTransaction.FieldNames.Memo, value.Memo);
        writer.WriteField(BankTransaction.FieldNames.Category, value.Category);
        writer.WriteField(BankTransaction.FieldNames.ClearedStatus, value.ClearedStatus);
        writer.WriteField(BankTransaction.FieldNames.Payee, value.Payee);
        foreach (string address in value.Address)
        {
            writer.WriteField(BankTransaction.FieldNames.Address, address);
        }

        foreach (BankSplit split in value.Splits)
        {
            writer.WriteField(BankTransaction.FieldNames.SplitCategory, split.Category);
            writer.WriteField(BankTransaction.FieldNames.SplitMemo, split.Memo);
            writer.WriteField(BankTransaction.FieldNames.SplitAmount, split.Amount);
            writer.WriteField(BankTransaction.FieldNames.SplitPercent, split.Percentage);
        }
    }
}
