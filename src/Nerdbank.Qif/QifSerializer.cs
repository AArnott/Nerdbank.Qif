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
#pragma warning disable IDE0008 // Use explicit type
        var transactionsByType = from tx in value.Transactions
                                 group tx by tx.AccountType into txTypes
                                 orderby txTypes.Key
                                 select txTypes;
        foreach (var txGroup in transactionsByType)
#pragma warning restore IDE0008 // Use explicit type
        {
            WriteRecord("Type", GetAccountTypeString(txGroup.Key), txGroup, this.Write);
        }

        WriteRecord("Type", Account.Types.Memorized, value.MemorizedTransactions, this.Write);
        WriteRecord("Type", "Cat", value.Categories, this.Write);
        WriteRecord("Type", "Class", value.Classes, this.Write);

        // Finish with all account details at the end so that no transactions follow the last account
        // which would be misinterpreted by importers as associating all those transactions with that account.
        foreach (Account account in value.Accounts)
        {
            if (!QifUtilities.Equals("Account", writer.LastWrittenHeader.Name))
            {
                writer.WriteHeader("Account");
            }

            this.Write(writer, account, includeTransactions: true);
        }

        void WriteRecord<T>(string headerName, string? headerValue, IEnumerable<T> records, Action<QifWriter, T> recordWriter)
        {
            if (records.Any())
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

            if (reader.Kind == QifToken.BOF)
            {
                reader.MoveNext();
            }

            while (reader.Kind == QifToken.Header)
            {
                if (QifUtilities.Equals("Type", reader.Header.Name))
                {
                    if (GetAccountTypeFromString(reader.Header.Value.Span) is AccountType accountType)
                    {
                        reader.MoveNext();
                        while (reader.Kind == QifToken.Field)
                        {
                            switch (accountType)
                            {
                                case AccountType.Investment:
                                    InvestmentTransaction investmentTx = this.ReadInvestmentTransaction(reader);
                                    if (lastAccountRead is InvestmentAccount investmentAccount)
                                    {
                                        investmentAccount.Transactions.Add(investmentTx);
                                    }
                                    else
                                    {
                                        result.Transactions.Add(investmentTx);
                                    }

                                    break;
                                case AccountType.Memorized:
                                    result.MemorizedTransactions.Add(this.ReadMemorizedTransaction(reader));
                                    break;
                                default:
                                    BankTransaction bankTx = this.ReadBankTransaction(reader, accountType);
                                    if (lastAccountRead is BankAccount bankAccount)
                                    {
                                        bankAccount.Transactions.Add(bankTx);
                                    }
                                    else
                                    {
                                        result.Transactions.Add(bankTx);
                                    }

                                    break;
                            }
                        }
                    }
                    else if (QifUtilities.Equals("Cat", reader.Header.Value))
                    {
                        reader.MoveNext();
                        while (reader.Kind == QifToken.Field)
                        {
                            result.Categories.Add(this.ReadCategory(reader));
                        }
                    }
                    else if (QifUtilities.Equals("Class", reader.Header.Value))
                    {
                        reader.MoveNext();
                        while (reader.Kind == QifToken.Field)
                        {
                            result.Classes.Add(this.ReadClass(reader));
                        }
                    }
                    else
                    {
                        // We don't recognize this header, so skip its entire content.
                        reader.MoveToNext(QifToken.Header);
                    }
                }
                else if (QifUtilities.Equals("Account", reader.Header.Name))
                {
                    reader.MoveNext();
                    while (reader.Kind == QifToken.Field)
                    {
                        Account account = this.ReadAccount(reader);
                        lastAccountRead = account;
                        result.Accounts.Add(account);
                    }
                }
                else
                {
                    // We don't recognize this header, so skip its entire content.
                    reader.MoveToNext(QifToken.Header);
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

        foreach ((ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) field in reader.ReadTheseFields())
        {
            if (QifUtilities.Equals(Class.FieldNames.Name, field.Name))
            {
                name = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Class.FieldNames.Description, field.Name))
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
    /// <param name="type">The type of the transaction being read.</param>
    /// <returns>The deserialized record.</returns>
    /// <devremarks>
    /// Keep this in close sync with <see cref="ReadMemorizedTransaction(QifReader)"/>.
    /// </devremarks>
    public virtual BankTransaction ReadBankTransaction(QifReader reader, AccountType type)
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

        foreach ((ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) field in reader.ReadTheseFields())
        {
            if (QifUtilities.Equals(BankTransaction.FieldNames.Date, field.Name))
            {
                date = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Amount, field.Name))
            {
                amount = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.ClearedStatus, field.Name))
            {
                clearedStatus = reader.ReadFieldAsClearedState();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Number, field.Name))
            {
                number = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Payee, field.Name))
            {
                payee = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Memo, field.Name))
            {
                memo = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Category, field.Name))
            {
                category = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.Address, field.Name))
            {
                address = address.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.SplitCategory, field.Name))
            {
                splitCategories = splitCategories.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.SplitMemo, field.Name))
            {
                splitMemos = splitMemos.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.SplitAmount, field.Name))
            {
                splitAmounts = splitAmounts.Add(reader.ReadFieldAsDecimal());
            }
            else if (QifUtilities.Equals(BankTransaction.FieldNames.SplitPercent, field.Name))
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

        return new(
            type,
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
        writer.WriteField(MemorizedTransaction.FieldNames.AmortizationFirstPaymentDate, value.AmortizationFirstPaymentDate);
        writer.WriteField(MemorizedTransaction.FieldNames.AmortizationTotalYearsForLoan, value.AmortizationTotalYearsForLoan);
        writer.WriteField(MemorizedTransaction.FieldNames.AmortizationNumberOfPaymentsAlreadyMade, value.AmortizationNumberOfPaymentsAlreadyMade);
        writer.WriteField(MemorizedTransaction.FieldNames.AmortizationNumberOfPeriodsPerYear, value.AmortizationNumberOfPeriodsPerYear);
        writer.WriteField(MemorizedTransaction.FieldNames.AmortizationInterestRate, value.AmortizationInterestRate);
        writer.WriteField(MemorizedTransaction.FieldNames.AmortizationCurrentLoanBalance, value.AmortizationCurrentLoanBalance);
        writer.WriteField(MemorizedTransaction.FieldNames.AmortizationOriginalLoanAmount, value.AmortizationOriginalLoanAmount);
        writer.WriteEndOfRecord();
    }

    /// <summary>
    /// Deserializes a <see cref="MemorizedTransaction"/> from the given <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader to deserialize from.</param>
    /// <returns>The deserialized record.</returns>
    /// <devremarks>
    /// Keep this in close sync with <see cref="ReadBankTransaction"/>.
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
        DateTime? amortizationFirstPaymentDate = null;
        int? amortizationTotalYearsForLoan = null;
        int? amortizationNumberOfPaymentsAlreadyMade = null;
        int? amortizationNumberOfPeriodsPerYear = null;
        decimal? amortizationInterestRate = null;
        decimal? amortizationCurrentLoanBalance = null;
        decimal? amortizationOriginalLoanAmount = null;

        foreach ((ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) field in reader.ReadTheseFields())
        {
            if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Date, field.Name))
            {
                date = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Amount, field.Name))
            {
                amount = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.ClearedStatus, field.Name))
            {
                clearedStatus = reader.ReadFieldAsClearedState();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Number, field.Name))
            {
                number = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Payee, field.Name))
            {
                payee = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Memo, field.Name))
            {
                memo = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Category, field.Name))
            {
                category = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Address, field.Name))
            {
                address = address.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.SplitCategory, field.Name))
            {
                splitCategories = splitCategories.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.SplitMemo, field.Name))
            {
                splitMemos = splitMemos.Add(reader.ReadFieldAsString());
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.SplitAmount, field.Name))
            {
                splitAmounts = splitAmounts.Add(reader.ReadFieldAsDecimal());
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.SplitPercent, field.Name))
            {
                splitPercentage = splitPercentage.Add(reader.ReadFieldAsDecimal());
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.Type, field.Name))
            {
                if (field.Value.Span.Length != 1)
                {
                    throw new InvalidTransactionException("Unexpected length in exception type.");
                }

                type = field.Value.Span[0] switch
                {
                    MemorizedTransaction.TransactionTypeCodes.Check => MemorizedTransactionType.Check,
                    MemorizedTransaction.TransactionTypeCodes.Deposit => MemorizedTransactionType.Deposit,
                    MemorizedTransaction.TransactionTypeCodes.Payment => MemorizedTransactionType.Payment,
                    MemorizedTransaction.TransactionTypeCodes.Investment => MemorizedTransactionType.Investment,
                    MemorizedTransaction.TransactionTypeCodes.ElectronicPayee => MemorizedTransactionType.ElectronicPayee,
                    _ => throw new InvalidTransactionException("Unsupported memorized transaction type."),
                };
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.AmortizationInterestRate, field.Name))
            {
                amortizationInterestRate = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.AmortizationCurrentLoanBalance, field.Name))
            {
                amortizationCurrentLoanBalance = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.AmortizationFirstPaymentDate, field.Name))
            {
                amortizationFirstPaymentDate = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.AmortizationNumberOfPaymentsAlreadyMade, field.Name))
            {
                amortizationNumberOfPaymentsAlreadyMade = (int)reader.ReadFieldAsInt64();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.AmortizationNumberOfPeriodsPerYear, field.Name))
            {
                amortizationNumberOfPeriodsPerYear = (int)reader.ReadFieldAsInt64();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.AmortizationTotalYearsForLoan, field.Name))
            {
                amortizationTotalYearsForLoan = (int)reader.ReadFieldAsInt64();
            }
            else if (QifUtilities.Equals(MemorizedTransaction.FieldNames.AmortizationOriginalLoanAmount, field.Name))
            {
                amortizationOriginalLoanAmount = (int)reader.ReadFieldAsInt64();
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
            AmortizationFirstPaymentDate = amortizationFirstPaymentDate,
            AmortizationTotalYearsForLoan = amortizationTotalYearsForLoan,
            AmortizationNumberOfPaymentsAlreadyMade = amortizationNumberOfPaymentsAlreadyMade,
            AmortizationNumberOfPeriodsPerYear = amortizationNumberOfPeriodsPerYear,
            AmortizationInterestRate = amortizationInterestRate,
            AmortizationCurrentLoanBalance = amortizationCurrentLoanBalance,
            AmortizationOriginalLoanAmount = amortizationOriginalLoanAmount,
        };
    }

    /// <inheritdoc cref="Write(QifWriter, Class)" />
    public virtual void Write(QifWriter writer, Category value)
    {
        writer.WriteField(Category.FieldNames.Name, value.Name);
        writer.WriteField(Category.FieldNames.Description, value.Description);
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

        foreach ((ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) field in reader.ReadTheseFields())
        {
            if (QifUtilities.Equals(Category.FieldNames.Name, reader.Field.Name))
            {
                name = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Category.FieldNames.Description, reader.Field.Name))
            {
                description = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Category.FieldNames.TaxSchedule, reader.Field.Name))
            {
                taxSchedule = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Category.FieldNames.TaxRelated, reader.Field.Name))
            {
                taxRelated = true;
            }
            else if (QifUtilities.Equals(Category.FieldNames.IncomeCategory, reader.Field.Name))
            {
                incomeCategory = true;
            }
            else if (QifUtilities.Equals(Category.FieldNames.ExpenseCategory, reader.Field.Name))
            {
                expenseCategory = true;
            }
            else if (QifUtilities.Equals(Category.FieldNames.BudgetAmount, reader.Field.Name))
            {
                budgetAmount = reader.ReadFieldAsDecimal();
            }
        }

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
        decimal? commission = null;
        decimal? price = null;
        decimal? quantity = null;
        string? security = null;
        string? payee = null;
        decimal? transactionAmount = null;
        string? accountForTransfer = null;
        decimal? amountTransferred = null;

        foreach ((ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) field in reader.ReadTheseFields())
        {
            if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Date, reader.Field.Name))
            {
                date = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.ClearedStatus, reader.Field.Name))
            {
                clearedStatus = reader.ReadFieldAsClearedState();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Memo, reader.Field.Name))
            {
                memo = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Action, reader.Field.Name))
            {
                action = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Commission, reader.Field.Name))
            {
                commission = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Price, reader.Field.Name))
            {
                price = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Quantity, reader.Field.Name))
            {
                quantity = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Security, reader.Field.Name))
            {
                security = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.Payee, reader.Field.Name))
            {
                payee = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.TransactionAmount, reader.Field.Name))
            {
                transactionAmount = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.AccountForTransfer, reader.Field.Name))
            {
                accountForTransfer = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(InvestmentTransaction.FieldNames.AmountTransferred, reader.Field.Name))
            {
                amountTransferred = reader.ReadFieldAsDecimal();
            }
        }

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

    /// <inheritdoc cref="Write(QifWriter, Account, bool)" />
    public virtual void Write(QifWriter writer, Account value) => this.Write(writer, value, includeTransactions: false);

    /// <inheritdoc cref="Write(QifWriter, Class)" />
    /// <param name="writer"><inheritdoc cref="Write(QifWriter, Class)" path="/param[@name='writer']"/></param>
    /// <param name="value"><inheritdoc cref="Write(QifWriter, Class)" path="/param[@name='value']"/></param>
    /// <param name="includeTransactions"><see langword="true" /> to include <see cref="Account.Transactions"/> in the serialized output; <see langword="false" /> otherwise.</param>
    public virtual void Write(QifWriter writer, Account value, bool includeTransactions)
    {
        writer.WriteField(Account.FieldNames.Name, value.Name);
        writer.WriteField(Account.FieldNames.Type, value.Type);
        writer.WriteField(Account.FieldNames.Description, value.Description);
        writer.WriteField(Account.FieldNames.CreditLimit, value.CreditLimit);
        writer.WriteField(Account.FieldNames.StatementBalanceDate, value.StatementBalanceDate);
        writer.WriteField(Account.FieldNames.StatementBalance, value.StatementBalance);
        writer.WriteEndOfRecord();

        if (includeTransactions && value.Transactions.Count > 0)
        {
            writer.WriteHeader("Type", value.Type);
            if (value is BankAccount bankAccount)
            {
                foreach (BankTransaction tx in bankAccount.Transactions)
                {
                    this.Write(writer, tx);
                }
            }
            else if (value is InvestmentAccount investmentAccount)
            {
                foreach (InvestmentTransaction tx in investmentAccount.Transactions)
                {
                    this.Write(writer, tx);
                }
            }
        }
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

        foreach ((ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) field in reader.ReadTheseFields())
        {
            if (QifUtilities.Equals(Account.FieldNames.Name, reader.Field.Name))
            {
                name = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Account.FieldNames.Type, reader.Field.Name))
            {
                type = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Account.FieldNames.Description, reader.Field.Name))
            {
                description = reader.ReadFieldAsString();
            }
            else if (QifUtilities.Equals(Account.FieldNames.CreditLimit, reader.Field.Name))
            {
                creditLimit = reader.ReadFieldAsDecimal();
            }
            else if (QifUtilities.Equals(Account.FieldNames.StatementBalanceDate, reader.Field.Name))
            {
                statementBalanceDate = reader.ReadFieldAsDate();
            }
            else if (QifUtilities.Equals(Account.FieldNames.StatementBalance, reader.Field.Name))
            {
                statementBalance = reader.ReadFieldAsDecimal();
            }
        }

        Account result = type == Account.Types.Investment
            ? new InvestmentAccount(ValueOrThrow(name, Account.FieldNames.Name))
            : new BankAccount(ValueOrThrow(type, Account.FieldNames.Type), ValueOrThrow(name, Account.FieldNames.Name));

        return result with
        {
            Description = description,
            CreditLimit = creditLimit,
            StatementBalanceDate = statementBalanceDate,
            StatementBalance = statementBalance,
        };
    }

    /// <inheritdoc cref="Write(QifWriter, Class)" />
    public void Write(QifWriter writer, Transaction value)
    {
        if (value is BankTransaction bankTx)
        {
            this.Write(writer, bankTx);
        }
        else if (value is InvestmentTransaction investmentTx)
        {
            this.Write(writer, investmentTx);
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    internal static AccountType? GetAccountTypeFromString(ReadOnlySpan<char> type)
    {
        if (QifUtilities.Equals(Account.Types.Bank, type))
        {
            return AccountType.Bank;
        }
        else if (QifUtilities.Equals(Account.Types.Cash, type))
        {
            return AccountType.Cash;
        }
        else if (QifUtilities.Equals(Account.Types.CreditCard, type))
        {
            return AccountType.CreditCard;
        }
        else if (QifUtilities.Equals(Account.Types.Asset, type))
        {
            return AccountType.Asset;
        }
        else if (QifUtilities.Equals(Account.Types.Liability, type))
        {
            return AccountType.Liability;
        }
        else if (QifUtilities.Equals(Account.Types.Investment, type))
        {
            return AccountType.Investment;
        }
        else if (QifUtilities.Equals(Account.Types.Memorized, type))
        {
            return AccountType.Memorized;
        }

        return null;
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

    private static string GetAccountTypeString(AccountType type)
    {
        return type switch
        {
            AccountType.Bank => Account.Types.Bank,
            AccountType.CreditCard => Account.Types.CreditCard,
            AccountType.Liability => Account.Types.Liability,
            AccountType.Asset => Account.Types.Asset,
            AccountType.Cash => Account.Types.Cash,
            AccountType.Memorized => Account.Types.Memorized,
            AccountType.Investment => Account.Types.Investment,
            _ => throw new ArgumentException(),
        };
    }
}
