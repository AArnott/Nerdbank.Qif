using QifApi.Config;
using QifApi.Transactions;
using QifApi.Transactions.Fields;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace QifApi.Logic
{
    internal static class CategoryListLogic
    {
        /// <summary>
        /// Creates a collection of category list transactions
        /// </summary>
        /// <param name="transactionItems">The transaction delimited string</param>
        /// <param name="config">The configuration to use while importing raw data</param> 
        /// <returns>A collection of bank transactions</returns>
        public static List<CategoryListTransaction> Import(string transactionItems, Configuration config)
        {
            List<CategoryListTransaction> result = new List<CategoryListTransaction>();

            // Create a new bank transaction
            CategoryListTransaction clt = new CategoryListTransaction()
            {
                ExpenseCategory = true, // Default per spec
            };

            // Split the string by new lines
            string[] sEntries = Regex.Split(transactionItems, "$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

            // Iterate over the array
            for (int i = 0; i < sEntries.Length; i++)
            {
                // Extract a line entry
                string sEntry = sEntries[i].Replace("\r", "").Replace("\n", "");

                // If the string has a value
                if (sEntry.Length > 0)
                {
                    // Test the first value of the string
                    switch (sEntry[0].ToString())
                    {
                        case CategoryListFields.BudgetAmount:
                            // Set the date value
                            clt.BudgetAmount = sEntry.Substring(1).ParseDecimalString(config);

                            // Stop processing
                            break;
                        case CategoryListFields.CategoryName:
                            // Set the amount value
                            clt.CategoryName = sEntry.Substring(1);

                            // Stop processing
                            break;
                        case CategoryListFields.Description:
                            // Set the cleared status value
                            clt.Description = sEntry.Substring(1);

                            // Stop processing
                            break;
                        case CategoryListFields.ExpenseCategory:
                            // Set the number value
                            clt.ExpenseCategory = true;
                            clt.IncomeCategory = false;

                            // Stop processing
                            break;
                        case CategoryListFields.IncomeCategory:
                            // Set the payee value
                            clt.IncomeCategory = true;
                            clt.ExpenseCategory = false;

                            // Stop processing
                            break;
                        case CategoryListFields.TaxRelated:
                            // Set the memo value
                            clt.TaxRelated = true;

                            // Stop processing
                            break;
                        case CategoryListFields.TaxSchedule:
                            // Set the memo value
                            clt.TaxSchedule = sEntry.Substring(1);

                            // Stop processing
                            break;
                        case AccountInformationFields.EndOfEntry:
                            // Add the bank transaction instance to the collection
                            result.Add(clt);

                            // Call the destructor
                            clt = null;

                            // Create a new bank transaction
                            clt = new CategoryListTransaction();

                            // Stop processing
                            break;
                    }
                }
            }

            return result;
        }

        internal static void Export(TextWriter writer, List<CategoryListTransaction> list, Configuration config)
        {
            if ((list != null) && (list.Count > 0))
            {
                writer.WriteLine(Headers.CategoryList);

                foreach (CategoryListTransaction item in list)
                {
                    writer.WriteLine(CategoryListFields.BudgetAmount + item.BudgetAmount.GetDecimalString(config));

                    if (!string.IsNullOrEmpty(item.CategoryName))
                    {
                        writer.WriteLine(CategoryListFields.CategoryName + item.CategoryName);
                    }

                    if (!string.IsNullOrEmpty(item.Description))
                    {
                        writer.WriteLine(CategoryListFields.Description + item.Description);
                    }

                    if (item.ExpenseCategory)
                    {
                        writer.WriteLine(CategoryListFields.ExpenseCategory + item.ExpenseCategory.ToString());
                    }

                    if (item.IncomeCategory)
                    {
                        writer.WriteLine(CategoryListFields.IncomeCategory + item.IncomeCategory.ToString());
                    }

                    writer.WriteLine(CategoryListFields.TaxRelated + item.TaxRelated.ToString());

                    if (!string.IsNullOrEmpty(item.TaxSchedule))
                    {
                        writer.WriteLine(CategoryListFields.TaxSchedule + item.TaxSchedule);
                    }

                    writer.WriteLine(CategoryListFields.EndOfEntry);
                }
            }
        }
    }
}