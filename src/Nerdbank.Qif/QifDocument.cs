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
    /// <param name="encoding">The text encoding to use. Defaults to <see cref="Encoding.UTF8"/>.</param>
    /// <returns>A QifDom object of transactions imported.</returns>
    public static QifDocument Load(string fileName, Encoding? encoding = null)
    {
        Requires.NotNullOrEmpty(fileName, nameof(fileName));
        using StreamReader streamReader = new StreamReader(File.OpenRead(fileName), encoding ?? Encoding.UTF8);
        return Load(streamReader);
    }

    /// <summary>
    /// Loads a <see cref="QifDocument"/> from a <see cref="QifReader"/>.
    /// </summary>
    /// <param name="reader">The reader with the QIF file to import. This will be disposed of when loading completes.</param>
    /// <returns>A new <see cref="QifDocument"/>.</returns>
    public static QifDocument Load(QifReader reader) => QifSerializer.Default.ReadDocument(reader);

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
    /// <param name="encoding">The text encoding to use. Defaults to <see cref="Encoding.UTF8"/>.</param>
    /// <remarks>This will overwrite an existing file.</remarks>
    public void Save(string fileName, Encoding? encoding = null)
    {
        Requires.NotNullOrEmpty(fileName, nameof(fileName));

        if (File.Exists(fileName))
        {
            File.SetAttributes(fileName, FileAttributes.Normal);
        }

        using StreamWriter writer = new StreamWriter(File.OpenWrite(fileName), encoding ?? Encoding.UTF8) { AutoFlush = true };
        this.Save(writer);
    }

    /// <summary>
    /// Writes the current document to a given <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The writer to serialize to.</param>
    public void Save(TextWriter writer) => QifSerializer.Default.Write(new QifWriter(writer), this);
}
