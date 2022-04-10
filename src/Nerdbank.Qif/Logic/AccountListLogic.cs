// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif.Logic;

internal static class AccountListLogic
{
    /// <summary>
    /// Creates a collection of account list transactions.
    /// </summary>
    /// <param name="transactionItems">The transaction delimited string.</param>
    /// <param name="config">The configuration to use while importing raw data.</param>
    /// <returns>A collection of bank transactions.</returns>
    public static List<AccountListTransaction> Import(string transactionItems, Configuration config)
    {
        List<AccountListTransaction> result = new List<AccountListTransaction>();

        // Create a new transaction
        AccountListTransaction? alt = new AccountListTransaction();

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
                    case AccountInformationFields.AccountType:
                        // Set the date value
                        alt.Type = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case AccountInformationFields.CreditLimit:
                        // Set the amount value
                        alt.CreditLimit = sEntry.Substring(1).ParseDecimalString(config);

                        // Stop processing
                        break;
                    case AccountInformationFields.Description:
                        // Set the cleared status value
                        alt.Description = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case AccountInformationFields.Name:
                        // Set the number value
                        alt.Name = sEntry.Substring(1);

                        // Stop processing
                        break;
                    case AccountInformationFields.StatementBalance:
                        // Set the payee value
                        alt.StatementBalance = sEntry.Substring(1).ParseDecimalString(config);

                        // Stop processing
                        break;
                    case AccountInformationFields.StatementBalanceDate:
                        // Set the memo value
                        alt.StatementBalanceDate = sEntry.Substring(1).ParseDateString(config);

                        // Stop processing
                        break;
                    case AccountInformationFields.EndOfEntry:
                        // Add the transaction instance to the collection
                        result.Add(alt);

                        // Call the destructor
                        alt = null;

                        // Create a new bank transaction
                        alt = new AccountListTransaction();

                        // Stop processing
                        break;
                }
            }
        }

        // Return the populated collection
        return result;
    }

    public static void Export(TextWriter writer, List<AccountListTransaction> list, Configuration config)
    {
        if ((list != null) && (list.Count > 0))
        {
            writer.WriteLine(Headers.AccountList);

            foreach (AccountListTransaction item in list)
            {
                if (!string.IsNullOrEmpty(item.Type))
                {
                    writer.WriteLine(AccountInformationFields.AccountType + item.Type);
                }

                writer.WriteLine(AccountInformationFields.CreditLimit + item.CreditLimit.GetDecimalString(config));

                if (!string.IsNullOrEmpty(item.Description))
                {
                    writer.WriteLine(AccountInformationFields.Description + item.Description);
                }

                if (!string.IsNullOrEmpty(item.Name))
                {
                    writer.WriteLine(AccountInformationFields.Name + item.Name);
                }

                writer.WriteLine(AccountInformationFields.StatementBalance + item.StatementBalance.GetDecimalString(config));

                writer.WriteLine(AccountInformationFields.StatementBalanceDate + item.StatementBalanceDate.GetDateString(config));

                writer.WriteLine(AccountInformationFields.EndOfEntry);
            }
        }
    }
}
