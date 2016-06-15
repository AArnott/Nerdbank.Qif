﻿using System.Collections.Generic;
using QifApi.Transactions;
using System.IO;
using QifApi.Logic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using QifApi.Config;
using System.ComponentModel;
using System.Text;

namespace QifApi
{
    /// <summary>
    /// Represents a Document Object Model for a QIF file.
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    public class QifDom
    {
        /// <summary>
        /// Represents a collection of bank transactions.
        /// </summary>
        public List<BasicTransaction> BankTransactions
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a collection of cash transactions.
        /// </summary>
        public List<BasicTransaction> CashTransactions
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a collection of credit card transactions.
        /// </summary>
        public List<BasicTransaction> CreditCardTransactions
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a collection of investment transactions.
        /// </summary>
        public List<InvestmentTransaction> InvestmentTransactions
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a collection of asset transactions.
        /// </summary>
        public List<BasicTransaction> AssetTransactions
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a collection of liability transactions.
        /// </summary>
        public List<BasicTransaction> LiabilityTransactions
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a collection of account list transactions.
        /// </summary>
        public List<AccountListTransaction> AccountListTransactions
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a collection of category list transactions.
        /// </summary>
        public List<CategoryListTransaction> CategoryListTransactions
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a collection of class list transactions.
        /// </summary>
        public List<ClassListTransaction> ClassListTransactions
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a collection of memorized transaction list transactions.
        /// </summary>
        public List<MemorizedTransactionListTransaction> MemorizedTransactionListTransactions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the configuration to use while processing the QIF file.
        /// </summary>
        /// <value>The configuration to use while processing the QIF file.</value>
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Configuration Configuration
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new QIF DOM.
        /// </summary>
        public QifDom(Configuration config = null)
        {
            BankTransactions = new List<BasicTransaction>();
            CashTransactions = new List<BasicTransaction>();
            CreditCardTransactions = new List<BasicTransaction>();
            InvestmentTransactions = new List<InvestmentTransaction>();
            AssetTransactions = new List<BasicTransaction>();
            LiabilityTransactions = new List<BasicTransaction>();
            AccountListTransactions = new List<AccountListTransaction>();
            CategoryListTransactions = new List<CategoryListTransaction>();
            ClassListTransactions = new List<ClassListTransaction>();
            MemorizedTransactionListTransactions = new List<MemorizedTransactionListTransaction>();
            Configuration = config ?? new Configuration();
        }

        /// <summary>
        /// Imports the specified file and replaces the current instance properties with details found in the import file.
        /// </summary>
        /// <param name="fileName">Name of the file to import.</param>
        public void Import(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                Import(reader);
            }
        }

        /// <summary>
        /// Imports a stream in a QIF format and replaces the current instance properties with details found in the import stream.
        /// </summary>
        /// <param name="reader">The import reader stream.</param>
        public void Import(StreamReader reader)
        {
            QifDom import = ImportFile(reader, Configuration);

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

        /// <summary>
        /// Exports the current instance properties to the specified file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="encoding">
        /// The encoding to use when exporting the QIF file. This defaults to UTF8
        /// when not specified.
        /// </param>
        /// <remarks>This will overwrite an existing file.</remarks>
        public void Export(string fileName, Encoding encoding = null)
        {
            ExportFile(this, fileName, encoding);
        }

        /// <summary>
        /// Exports the specified instance properties to the specified file.
        /// </summary>
        /// <param name="qif">The <seealso cref="T:QifDom"/> to export.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="encoding">
        /// The encoding to use when exporting the QIF file. This defaults to UTF8
        /// when not specified.
        /// </param>
        /// <remarks>This will overwrite an existing file.</remarks>
        public static void ExportFile(QifDom qif, string fileName, Encoding encoding = null)
        {
            if (File.Exists(fileName))
            {
                File.SetAttributes(fileName, FileAttributes.Normal);
            }

            using (StreamWriter writer = new StreamWriter(fileName, false, encoding ?? Encoding.UTF8))
            {
                writer.AutoFlush = true;

                AccountListLogic.Export(writer, qif.AccountListTransactions, qif.Configuration);
                AssetLogic.Export(writer, qif.AssetTransactions, qif.Configuration);
                BankLogic.Export(writer, qif.BankTransactions, qif.Configuration);
                CashLogic.Export(writer, qif.CashTransactions, qif.Configuration);
                CategoryListLogic.Export(writer, qif.CategoryListTransactions, qif.Configuration);
                ClassListLogic.Export(writer, qif.ClassListTransactions, qif.Configuration);
                CreditCardLogic.Export(writer, qif.CreditCardTransactions, qif.Configuration);
                InvestmentLogic.Export(writer, qif.InvestmentTransactions, qif.Configuration);
                LiabilityLogic.Export(writer, qif.LiabilityTransactions, qif.Configuration);
                MemorizedTransactionListLogic.Export(writer, qif.MemorizedTransactionListTransactions, qif.Configuration);
            }
        }

        /// <summary>
        /// Imports a QIF file and returns a QifDom object.
        /// </summary>
        /// <param name="fileName">The QIF file to import.</param>
        /// <returns>A QifDom object of transactions imported.</returns>
        public static QifDom ImportFile(string fileName)
        {
            QifDom result = null;

            // If the file doesn't exist
            if (File.Exists(fileName) == false)
            {
                // Identify the file doesn't exist
                throw new FileNotFoundException();
            }

            // Open the file
            using (StreamReader sr = new StreamReader(fileName))
            {
                result = ImportFile(sr);
            }

            return result;
        }

        /// <summary>
        /// Imports a QIF file stream reader and returns a QifDom object.
        /// </summary>
        /// <param name="reader">The stream reader pointing to an underlying QIF file to import.</param>
        /// <param name="config">The configuration to use while importing raw data</param> 
        /// <returns>A QifDom object of transactions imported.</returns>
        public static QifDom ImportFile(StreamReader reader, Configuration config = null)
        {
            QifDom result = new QifDom(config);

            // Read the entire file
            string input = reader.ReadToEnd();

            // Split the file by header types
            string[] transactionTypes = Regex.Split(input, @"^(!.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

            // Loop through the transaction types
            for (int i = 0; i < transactionTypes.Length; i++)
            {
                // Get the exact transaction type
                string transactionType = transactionTypes[i].Replace("\r", "").Replace("\n", "").Trim();

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
                            result.BankTransactions.AddRange(BankLogic.Import(bankItems, result.Configuration));

                            // All done
                            break;
                        case Headers.AccountList:
                            // Increment the array counter
                            i++;

                            // Extract the transaction items
                            string accountListItems = transactionTypes[i];

                            // Import all transaction types
                            result.AccountListTransactions.AddRange(AccountListLogic.Import(accountListItems, result.Configuration));

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
    }
}
