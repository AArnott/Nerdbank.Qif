// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Nerdbank.Qif.Logic;

namespace Nerdbank.Qif;

/// <summary>
/// A document that represents the data that may appear in a Quicken Interchange Format (QIF) file.
/// </summary>
public class QifDocument
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QifDocument"/> class.
    /// </summary>
    /// <param name="config">Configuration for the new object.</param>
    public QifDocument(Configuration? config = null)
    {
        this.Configuration = config ?? new Configuration();
    }

    /// <summary>
    /// Gets a collection of bank transactions.
    /// </summary>
    public List<BasicTransaction> BankTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of cash transactions.
    /// </summary>
    public List<BasicTransaction> CashTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of credit card transactions.
    /// </summary>
    public List<BasicTransaction> CreditCardTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of investment transactions.
    /// </summary>
    public List<InvestmentTransaction> InvestmentTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of asset transactions.
    /// </summary>
    public List<BasicTransaction> AssetTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of liability transactions.
    /// </summary>
    public List<BasicTransaction> LiabilityTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of account list transactions.
    /// </summary>
    public List<AccountListTransaction> AccountListTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of category list transactions.
    /// </summary>
    public List<CategoryListTransaction> CategoryListTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of class list transactions.
    /// </summary>
    public List<ClassListTransaction> ClassListTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of memorized transaction list transactions.
    /// </summary>
    public List<MemorizedTransactionListTransaction> MemorizedTransactionListTransactions { get; private set; } = new();

    /// <summary>
    /// Gets or sets the configuration to use while processing the QIF file.
    /// </summary>
    /// <value>The configuration to use while processing the QIF file.</value>
    public Configuration Configuration
    {
        get;
        set;
    }

    /// <summary>
    /// Imports a QIF file and returns a QifDom object.
    /// </summary>
    /// <param name="fileName">The QIF file to import.</param>
    /// <returns>A QifDom object of transactions imported.</returns>
    public static QifDocument Load(string fileName)
    {
        using StreamReader sr = new StreamReader(File.OpenRead(fileName));
        return Load(sr);
    }

    /// <summary>
    /// Imports a QIF file stream reader and returns a QifDom object.
    /// </summary>
    /// <param name="reader">The stream reader pointing to an underlying QIF file to import.</param>
    /// <param name="config">The configuration to use while importing raw data.</param>
    /// <returns>A QifDom object of transactions imported.</returns>
    public static QifDocument Load(TextReader reader, Configuration? config = null)
    {
        QifDocument result = new QifDocument(config);

        // Read the entire file
        string input = reader.ReadToEnd();

        // Split the file by header types
        string[] transactionTypes = Regex.Split(input, @"^(!.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

        // Remember the last account name we saw so we can link its transactions to it.
        string currentAccountName = string.Empty;

        // Loop through the transaction types
        for (int i = 0; i < transactionTypes.Length; i++)
        {
            // Get the exact transaction type
            string transactionType = transactionTypes[i].Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();

            // If the string has a value
            if (transactionType.Length > 0)
            {
                // Check the transaction type
                switch (transactionType)
                {
                    case Headers.Bank:
                        // Increment the array counter
                        i++;

                        // Extract the transaction items
                        string bankItems = transactionTypes[i];

                        // Import all transaction types
                        List<BasicTransaction>? transactions = BankLogic.Import(bankItems, result.Configuration);

                        // Associate the transactions with last account we saw.
                        foreach (BasicTransaction? transaction in transactions)
                        {
                            transaction.AccountName = currentAccountName;
                        }

                        result.BankTransactions.AddRange(transactions);

                        // All done
                        break;
                    case Headers.AccountList:
                        // Increment the array counter
                        i++;

                        // Extract the transaction items
                        string accountListItems = transactionTypes[i];

                        // Import all transaction types
                        List<AccountListTransaction>? accounts = AccountListLogic.Import(accountListItems, result.Configuration);

                        // Remember account so transaction following can be linked to it.
                        currentAccountName = accounts.Last().Name;

                        result.AccountListTransactions.AddRange(accounts);

                        // All done
                        break;
                    case Headers.Asset:
                        // Increment the array counter
                        i++;

                        // Extract the transaction items
                        string assetItems = transactionTypes[i];

                        // Import all transaction types
                        result.AssetTransactions.AddRange(AssetLogic.Import(assetItems, result.Configuration));

                        // All done
                        break;
                    case Headers.Cash:
                        // Increment the array counter
                        i++;

                        // Extract the transaction items
                        string cashItems = transactionTypes[i];

                        // Import all transaction types
                        result.CashTransactions.AddRange(CashLogic.Import(cashItems, result.Configuration));

                        // All done
                        break;
                    case Headers.CategoryList:
                        // Increment the array counter
                        i++;

                        // Extract the transaction items
                        string catItems = transactionTypes[i];

                        // Import all transaction types
                        result.CategoryListTransactions.AddRange(CategoryListLogic.Import(catItems, result.Configuration));

                        // All done
                        break;
                    case Headers.ClassList:
                        // Increment the array counter
                        i++;

                        // Extract the transaction items
                        string classItems = transactionTypes[i];

                        // Import all transaction types
                        result.ClassListTransactions.AddRange(ClassListLogic.Import(classItems, result.Configuration));

                        // All done
                        break;
                    case Headers.CreditCard:
                        // Increment the array counter
                        i++;

                        // Extract the transaction items
                        string ccItems = transactionTypes[i];

                        // Import all transaction types
                        result.CreditCardTransactions.AddRange(CreditCardLogic.Import(ccItems, result.Configuration));

                        // All done
                        break;
                    case Headers.Investment:
                        // Increment the array counter
                        i++;

                        // Extract the transaction items
                        string investItems = transactionTypes[i];

                        // Import all transaction types
                        result.InvestmentTransactions.AddRange(InvestmentLogic.Import(investItems, result.Configuration));

                        // All done
                        break;
                    case Headers.Liability:
                        // Increment the array counter
                        i++;

                        // Extract the transaction items
                        string liabilityItems = transactionTypes[i];

                        // Import all transaction types
                        result.LiabilityTransactions.AddRange(LiabilityLogic.Import(liabilityItems, result.Configuration));

                        // All done
                        break;
                    case Headers.MemorizedTransactionList:
                        // Increment the array counter
                        i++;

                        // Extract the transaction items
                        string memItems = transactionTypes[i];

                        // Import all transaction types
                        result.MemorizedTransactionListTransactions.AddRange(MemorizedTransactionListLogic.Import(memItems, result.Configuration));

                        // All done
                        break;
                    default:
                        // Don't do any processing
                        break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Imports the specified file and replaces the current instance properties with details found in the import file.
    /// </summary>
    /// <param name="fileName">Name of the file to import.</param>
    /// <param name="append">If set to <c>true</c> the import will append records rather than overwrite. Defaults to legacy behavior, which overwrites.</param>
    public void Load(string fileName, bool append = false)
    {
        using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
        {
            this.Load(reader, append);
        }
    }

    /// <summary>
    /// Imports a stream in a QIF format and replaces the current instance properties with details found in the import stream.
    /// </summary>
    /// <param name="reader">The import reader stream.</param>
    /// <param name="append">If set to <c>true</c> the import will append records rather than overwrite. Defaults to legacy behavior, which overwrites.</param>
    public void Load(StreamReader reader, bool append = false)
    {
        QifDocument import = Load(reader, this.Configuration);

        if (append)
        {
            this.AccountListTransactions.AddRange(import.AccountListTransactions);
            this.AssetTransactions.AddRange(import.AssetTransactions);
            this.BankTransactions.AddRange(import.BankTransactions);
            this.CashTransactions.AddRange(import.CashTransactions);
            this.CategoryListTransactions.AddRange(import.CategoryListTransactions);
            this.ClassListTransactions.AddRange(import.ClassListTransactions);
            this.CreditCardTransactions.AddRange(import.CreditCardTransactions);
            this.InvestmentTransactions.AddRange(import.InvestmentTransactions);
            this.LiabilityTransactions.AddRange(import.LiabilityTransactions);
            this.MemorizedTransactionListTransactions.AddRange(import.MemorizedTransactionListTransactions);
        }
        else
        {
            this.AccountListTransactions = import.AccountListTransactions;
            this.AssetTransactions = import.AssetTransactions;
            this.BankTransactions = import.BankTransactions;
            this.CashTransactions = import.CashTransactions;
            this.CategoryListTransactions = import.CategoryListTransactions;
            this.ClassListTransactions = import.ClassListTransactions;
            this.CreditCardTransactions = import.CreditCardTransactions;
            this.InvestmentTransactions = import.InvestmentTransactions;
            this.LiabilityTransactions = import.LiabilityTransactions;
            this.MemorizedTransactionListTransactions = import.MemorizedTransactionListTransactions;
        }
    }

    /// <summary>
    /// Exports the current instance properties to the specified file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="encoding">
    /// The encoding to use when exporting the QIF file. This defaults to UTF8
    /// when not specified.
    /// </param>
    /// <remarks>This will overwrite an existing file.</remarks>
    public void Save(string fileName, Encoding? encoding = null)
    {
        if (File.Exists(fileName))
        {
            File.SetAttributes(fileName, FileAttributes.Normal);
        }

        using StreamWriter writer = new StreamWriter(File.OpenWrite(fileName), encoding ?? Encoding.UTF8) { AutoFlush = true };
        this.Save(writer);
    }

    /// <summary>
    /// Writes the current QIF content to a given <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The writer to serialize to.</param>
    public void Save(TextWriter writer)
    {
        AccountListLogic.Export(writer, this.AccountListTransactions, this.Configuration);
        AssetLogic.Export(writer, this.AssetTransactions, this.Configuration);
        BankLogic.Export(writer, this.BankTransactions, this.Configuration);
        CashLogic.Export(writer, this.CashTransactions, this.Configuration);
        CategoryListLogic.Export(writer, this.CategoryListTransactions, this.Configuration);
        ClassListLogic.Export(writer, this.ClassListTransactions, this.Configuration);
        CreditCardLogic.Export(writer, this.CreditCardTransactions, this.Configuration);
        InvestmentLogic.Export(writer, this.InvestmentTransactions, this.Configuration);
        LiabilityLogic.Export(writer, this.LiabilityTransactions, this.Configuration);
        MemorizedTransactionListLogic.Export(writer, this.MemorizedTransactionListTransactions, this.Configuration);
    }
}
