// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using static Nerdbank.Qif.QifUtilities;

namespace Nerdbank.Qif;

/// <summary>
/// A category.
/// </summary>
/// <param name="Name">The name of the category.</param>
public record Category(string Name)
{
    /// <summary>
    /// Gets the description for this category.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the item is tax related.
    /// </summary>
    public bool TaxRelated { get; init; }

    /// <summary>
    /// Gets a value indicating whether the item is an income category.
    /// </summary>
    public bool IncomeCategory { get; init; }

    /// <summary>
    /// Gets a value indicating whether the item is an expense category.
    /// </summary>
    public bool ExpenseCategory { get; init; }

    /// <summary>
    /// Gets the budget amount.
    /// </summary>
    public decimal BudgetAmount { get; init; }

    /// <summary>
    /// Gets the tax schedule.
    /// </summary>
    public string? TaxSchedule { get; init; }

    /// <summary>
    /// The names of each field that may appear in this record.
    /// </summary>
    internal static class FieldNames
    {
        internal const string Name = "N";
        internal const string Description = "D";
        internal const string TaxRelated = "T";
        internal const string TaxSchedule = "R";
        internal const string IncomeCategory = "I";
        internal const string ExpenseCategory = "E";
        internal const string BudgetAmount = "B";
    }
}
