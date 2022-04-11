// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class CategoryTests : TestBase
{
    public CategoryTests(ITestOutputHelper logger)
        : base(logger)
    {
    }

    [Fact]
    public void Load_Income()
    {
        const string qifSource = @"NBonus
DBonus Income
T
R460
I
^";
        using QifReader reader = new(new StringReader(qifSource));
        Category category = Category.Load(reader);
        Assert.Equal("Bonus", category.Name);
        Assert.Equal("Bonus Income", category.Description);
        Assert.True(category.TaxRelated);
        Assert.True(category.IncomeCategory);
        Assert.False(category.ExpenseCategory);
        Assert.Equal("460", category.TaxSchedule);
    }

    [Fact]
    public void Load_Expense()
    {
        const string qifSource = @"NGardening
E
B30.20
^";
        using QifReader reader = new(new StringReader(qifSource));
        Category category = Category.Load(reader);
        Assert.Equal("Gardening", category.Name);
        Assert.Null(category.Description);
        Assert.False(category.TaxRelated);
        Assert.True(category.ExpenseCategory);
        Assert.False(category.IncomeCategory);
        Assert.Equal(30.20m, category.BudgetAmount);
    }
}
