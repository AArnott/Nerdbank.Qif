// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif.Logic;

/// <summary>
/// The category list fields used in transactions.
/// </summary>
internal static class CategoryListFields
{
    /// <summary>
    /// Category name:subcategory name.
    /// </summary>
    internal const string CategoryName = "N";

    /// <summary>
    /// Description.
    /// </summary>
    internal const string Description = "D";

    /// <summary>
    /// Tax related if included, not tax related if omitted.
    /// </summary>
    internal const string TaxRelated = "T";

    /// <summary>
    /// Income category.
    /// </summary>
    internal const string IncomeCategory = "I";

    /// <summary>
    /// If category is unspecified, assume expense type.
    /// </summary>
    internal const string ExpenseCategory = "E";

    /// <summary>
    /// Only in a Budget Amounts QIF file.
    /// </summary>
    internal const string BudgetAmount = "B";

    /// <summary>
    /// Tax schedule information.
    /// </summary>
    internal const string TaxSchedule = "R";

    /// <summary>
    /// End of entry.
    /// </summary>
    internal const string EndOfEntry = "^";
}
