// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Nerdbank.Qif;

/// <summary>
/// A document that represents the data that may appear in a Quicken Interchange Format (QIF) file.
/// </summary>
public class QifDocument
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QifDocument"/> class.
    /// </summary>
    public QifDocument()
    {
    }

    /// <summary>
    /// Gets a collection of bank transactions.
    /// </summary>
    public List<BankTransaction> BankTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of cash transactions.
    /// </summary>
    public List<BankTransaction> CashTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of credit card transactions.
    /// </summary>
    public List<BankTransaction> CreditCardTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of asset transactions.
    /// </summary>
    public List<BankTransaction> AssetTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of liability transactions.
    /// </summary>
    public List<BankTransaction> LiabilityTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of investment transactions.
    /// </summary>
    public List<InvestmentTransaction> InvestmentTransactions { get; private set; } = new();

    /// <summary>
    /// Gets a collection of account list transactions.
    /// </summary>
    public List<Account> Accounts { get; private set; } = new();

    /// <summary>
    /// Gets a collection of category list transactions.
    /// </summary>
    public List<Category> Categories { get; private set; } = new();

    /// <summary>
    /// Gets a collection of class list transactions.
    /// </summary>
    public List<Class> Classes { get; private set; } = new();

    /// <summary>
    /// Gets a collection of memorized transaction list transactions.
    /// </summary>
    public List<MemorizedTransaction> MemorizedTransactions { get; private set; } = new();

    /// <summary>
    /// Imports a QIF file and returns a QifDom object.
    /// </summary>
    /// <param name="fileName">The QIF file to import.</param>
    /// <returns>A QifDom object of transactions imported.</returns>
    public static QifDocument Load(string fileName)
    {
        Requires.NotNull(fileName, nameof(fileName));
        using StreamReader sr = new StreamReader(File.OpenRead(fileName));
        return Load(sr);
    }

    /// <summary>
    /// Loads a <see cref="QifDocument"/> from a <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader with the QIF file to import. This will be disposed of when loading completes.</param>
    /// <returns>A new <see cref="QifDocument"/>.</returns>
    public static QifDocument Load(QifReader reader)
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
                            result.BankTransactions.Add(BankTransaction.Load(reader) with { AccountName = lastAccountRead?.Name });
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Cash", headerValue))
                    {
                        do
                        {
                            result.CashTransactions.Add(BankTransaction.Load(reader) with { AccountName = lastAccountRead?.Name });
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("CCard", headerValue))
                    {
                        do
                        {
                            result.CreditCardTransactions.Add(BankTransaction.Load(reader) with { AccountName = lastAccountRead?.Name });
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Oth A", headerValue))
                    {
                        do
                        {
                            result.AssetTransactions.Add(BankTransaction.Load(reader) with { AccountName = lastAccountRead?.Name });
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Oth L", headerValue))
                    {
                        do
                        {
                            result.LiabilityTransactions.Add(BankTransaction.Load(reader) with { AccountName = lastAccountRead?.Name });
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Cat", headerValue))
                    {
                        do
                        {
                            result.Categories.Add(Category.Load(reader));
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Class", headerValue))
                    {
                        do
                        {
                            result.Classes.Add(Class.Load(reader));
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Invst", headerValue))
                    {
                        do
                        {
                            result.InvestmentTransactions.Add(InvestmentTransaction.Load(reader));
                        }
                        while (reader.Kind == QifParser.TokenKind.Field);
                    }
                    else if (QifUtilities.Equals("Memorized", headerValue))
                    {
                        do
                        {
                            result.MemorizedTransactions.Add(MemorizedTransaction.Load(reader));
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
                    Account account = Account.Load(reader);
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
    /// Loads a <see cref="QifDocument"/> from a <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">A text reader for the QIF content.</param>
    /// <returns>A new <see cref="QifDocument"/>.</returns>
    public static QifDocument Load(TextReader reader) => Load(new QifReader(reader));

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
        throw new NotImplementedException();
        ////AccountListLogic.Export(writer, this.Accounts, this.Configuration);
        ////AssetLogic.Export(writer, this.AssetTransactions, this.Configuration);
        ////BankLogic.Export(writer, this.BankTransactions, this.Configuration);
        ////CashLogic.Export(writer, this.CashTransactions, this.Configuration);
        ////CategoryListLogic.Export(writer, this.Categories, this.Configuration);
        ////ClassListLogic.Export(writer, this.Classes, this.Configuration);
        ////CreditCardLogic.Export(writer, this.CreditCardTransactions, this.Configuration);
        ////InvestmentLogic.Export(writer, this.InvestmentTransactions, this.Configuration);
        ////LiabilityLogic.Export(writer, this.LiabilityTransactions, this.Configuration);
        ////MemorizedTransactionListLogic.Export(writer, this.MemorizedTransactions, this.Configuration);
    }
}
