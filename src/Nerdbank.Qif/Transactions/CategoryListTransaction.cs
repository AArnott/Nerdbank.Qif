﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// A category list transaction.
/// </summary>
public class CategoryListTransaction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryListTransaction"/> class.
    /// </summary>
    public CategoryListTransaction()
    {
        this.CategoryName = string.Empty;
        this.Description = string.Empty;
        this.TaxSchedule = string.Empty;
    }

    /// <summary>
    /// Gets or sets the name of the category.
    /// </summary>
    /// <value>The name of the category.</value>
    public string CategoryName { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is tax related.
    /// </summary>
    /// <value><c>true</c> if tax related; otherwise, <c>false</c>.</value>
    public bool TaxRelated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is an income category.
    /// </summary>
    /// <value><c>true</c> if income category; otherwise, <c>false</c>.</value>
    public bool IncomeCategory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is an expense category.
    /// </summary>
    /// <value><c>true</c> if expense category; otherwise, <c>false</c>.</value>
    public bool ExpenseCategory { get; set; }

    /// <summary>
    /// Gets or sets the budget amount.
    /// </summary>
    /// <value>The budget amount.</value>
    public decimal BudgetAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax schedule.
    /// </summary>
    /// <value>The tax schedule.</value>
    public string TaxSchedule { get; set; }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString() => this.CategoryName;
}
