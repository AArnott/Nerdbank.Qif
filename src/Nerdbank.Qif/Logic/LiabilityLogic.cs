﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif.Logic;

internal static class LiabilityLogic
{
    /// <summary>
    /// Creates a collection of liability transactions.
    /// </summary>
    /// <param name="transactionItems">The transaction delimited string.</param>
    /// <param name="config">The configuration to use while importing raw data.</param>
    /// <returns>A collection of bank transactions.</returns>
    internal static List<BasicTransaction> Import(string transactionItems, Configuration config)
    {
        List<BasicTransaction> result = new List<BasicTransaction>();

        // Create a new bank transaction
        BasicTransaction? lt = new BasicTransaction();

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
                    case NonInvestmentAccountFields.Date:
                        // Set the date value
                        lt.Date = sEntry.Substring(1).ParseDateString(config);

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.Amount:
                        // Set the amount value
                        lt.Amount = sEntry.Substring(1).ParseDecimalString(config);

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.ClearedStatus:
                        // Set the cleared status value
                        lt.ClearedStatus = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.Number:
                        // Set the number value
                        lt.Number = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.Payee:
                        // Set the payee value
                        lt.Payee = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.Memo:
                        // Set the memo value
                        lt.Memo = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.Category:
                        // Set the category value
                        lt.Category = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.Address:
                        // Add the address line value
                        lt.Address.Add(lt.Address.Count, sEntry.Substring(1));

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.SplitCategory:
                        // Add the split category value
                        lt.SplitCategories.Add(lt.SplitCategories.Count, sEntry.Substring(1));

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.SplitMemo:
                        // Add the split memo value
                        // NOTE: Using split amount count because memo is optional
                        lt.SplitMemos.Add(lt.SplitAmounts.Count, sEntry.Substring(1));

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.SplitAmount:
                        // Add the split amount value
                        lt.SplitAmounts.Add(lt.SplitAmounts.Count, sEntry.Substring(1).ParseDecimalString(config));

                        // Stop processing
                        break;
                    case NonInvestmentAccountFields.EndOfEntry:
                        // Add the bank transaction instance to the collection
                        result.Add(lt);

                        // Call the destructor
                        lt = null;

                        // Create a new bank transaction
                        lt = new BasicTransaction();

                        // Stop processing
                        break;
                }
            }
        }

        return result;
    }

    internal static void Export(TextWriter writer, List<BasicTransaction> list, Configuration config)
    {
        if ((list != null) && (list.Count > 0))
        {
            writer.WriteLine(Headers.Liability);

            foreach (BasicTransaction item in list)
            {
                writer.WriteLine(NonInvestmentAccountFields.Date + item.Date.GetDateString(config));

                foreach (int i in item.Address.Keys)
                {
                    writer.WriteLine(NonInvestmentAccountFields.Address + item.Address[i]);
                }

                writer.WriteLine(NonInvestmentAccountFields.Amount + item.Amount.GetDecimalString(config));

                if (!string.IsNullOrEmpty(item.Category))
                {
                    writer.WriteLine(NonInvestmentAccountFields.Category + item.Category);
                }

                if (!string.IsNullOrEmpty(item.ClearedStatus))
                {
                    writer.WriteLine(NonInvestmentAccountFields.ClearedStatus + item.ClearedStatus);
                }

                if (!string.IsNullOrEmpty(item.Memo))
                {
                    writer.WriteLine(NonInvestmentAccountFields.Memo + item.Memo);
                }

                if (!string.IsNullOrEmpty(item.Number))
                {
                    writer.WriteLine(NonInvestmentAccountFields.Number + item.Number);
                }

                if (!string.IsNullOrEmpty(item.Payee))
                {
                    writer.WriteLine(NonInvestmentAccountFields.Payee + item.Payee);
                }

                foreach (int i in item.SplitCategories.Keys)
                {
                    writer.WriteLine(NonInvestmentAccountFields.SplitCategory + item.SplitCategories[i]);
                    writer.WriteLine(NonInvestmentAccountFields.SplitAmount + item.SplitAmounts[i]);

                    if (item.SplitMemos.ContainsKey(i))
                    {
                        writer.WriteLine(NonInvestmentAccountFields.SplitMemo + item.SplitMemos[i]);
                    }
                }

                writer.WriteLine(NonInvestmentAccountFields.EndOfEntry);
            }
        }
    }
}
