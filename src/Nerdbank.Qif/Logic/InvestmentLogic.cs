// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif.Logic;

internal static class InvestmentLogic
{
    /// <summary>
    /// Creates a collection of investment transactions.
    /// </summary>
    /// <param name="transactionItems">The transaction delimited string.</param>
    /// <param name="config">The configuration to use while importing raw data.</param>
    /// <returns>A collection of bank transactions.</returns>
    public static List<InvestmentTransaction> Import(string transactionItems, Configuration config)
    {
        List<InvestmentTransaction> result = new List<InvestmentTransaction>();

        // Create a new bank transaction
        InvestmentTransaction? it = new InvestmentTransaction();

        // Split the string by new lines
        string[] sEntries = Regex.Split(transactionItems, "$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

        // Iterate over the array
        for (int i = 0; i < sEntries.Length; i++)
        {
            // Extract a line entry
            string sEntry = sEntries[i].Replace("\r", string.Empty).Replace("\n", string.Empty);

            // If the string has a value
            if (sEntry.Length > 0)
            {
                // Test the first value of the string
                switch (sEntry[0].ToString())
                {
                    case InvestmentAccountFields.Action:
                        // Set the date value
                        it.Action = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.ClearedStatus:
                        // Set the amount value
                        it.ClearedStatus = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.Commission:
                        // Set the cleared status value
                        it.Commission = sEntry.Substring(1).ParseDecimalString(config);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.Date:
                        // Set the number value
                        it.Date = sEntry.Substring(1).ParseDateString(config);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.Memo:
                        // Set the payee value
                        it.Memo = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.Price:
                        // Set the memo value
                        it.Price = sEntry.Substring(1).ParseDecimalString(config);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.Quantity:
                        // Set the memo value
                        it.Quantity = sEntry.Substring(1).ParseDecimalString(config);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.Security:
                        // Set the memo value
                        it.Security = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.TextFirstLine:
                        // Set the memo value
                        it.TextFirstLine = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.TransactionAmount:
                        // Set the memo value
                        it.TransactionAmount = sEntry.Substring(1).ParseDecimalString(config);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.AccountForTransfer:
                        // Set the memo value
                        it.AccountForTransfer = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case InvestmentAccountFields.AmountTransferred:
                        // Set the memo value
                        it.AmountTransferred = sEntry.Substring(1).ParseDecimalString(config);

                        // Stop processing
                        break;
                    case AccountInformationFields.EndOfEntry:
                        // Add the bank transaction instance to the collection
                        result.Add(it);

                        // Call the destructor
                        it = null;

                        // Create a new bank transaction
                        it = new InvestmentTransaction();

                        // Stop processing
                        break;
                }
            }
        }

        return result;
    }

    internal static void Export(TextWriter writer, List<InvestmentTransaction> list, Configuration config)
    {
        if ((list != null) && (list.Count > 0))
        {
            writer.WriteLine(Headers.Investment);

            foreach (InvestmentTransaction item in list)
            {
                if (!string.IsNullOrEmpty(item.AccountForTransfer))
                {
                    writer.WriteLine(InvestmentAccountFields.AccountForTransfer + item.AccountForTransfer);
                }

                if (!string.IsNullOrEmpty(item.Action))
                {
                    writer.WriteLine(InvestmentAccountFields.Action + item.Action);
                }

                writer.WriteLine(InvestmentAccountFields.AmountTransferred + item.AmountTransferred.GetDecimalString(config));

                if (!string.IsNullOrEmpty(item.ClearedStatus))
                {
                    writer.WriteLine(InvestmentAccountFields.ClearedStatus + item.ClearedStatus);
                }

                writer.WriteLine(InvestmentAccountFields.Commission + item.Commission.GetDecimalString(config));

                writer.WriteLine(InvestmentAccountFields.Date + item.Date.GetDateString(config));

                if (!string.IsNullOrEmpty(item.Memo))
                {
                    writer.WriteLine(InvestmentAccountFields.Memo + item.Memo);
                }

                writer.WriteLine(InvestmentAccountFields.Price + item.Price.GetDecimalString(config));

                writer.WriteLine(InvestmentAccountFields.Quantity + item.Quantity.GetDecimalString(config));

                if (!string.IsNullOrEmpty(item.Security))
                {
                    writer.WriteLine(InvestmentAccountFields.Security + item.Security);
                }

                if (!string.IsNullOrEmpty(item.TextFirstLine))
                {
                    writer.WriteLine(InvestmentAccountFields.TextFirstLine + item.TextFirstLine);
                }

                writer.WriteLine(InvestmentAccountFields.TransactionAmount + item.TransactionAmount.GetDecimalString(config));

                writer.WriteLine(InvestmentAccountFields.EndOfEntry);
            }
        }
    }
}
