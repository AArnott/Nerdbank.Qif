// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

public class ImportTests : TestBase
{
    public ImportTests(ITestOutputHelper logger)
        : base(logger)
    {
    }

    [Fact]
    public void Can_import_sample_qif_when_current_culture_is_en_US()
    {
        QifDocument qif = Load("en-US");
        Assert.NotEmpty(qif.BankTransactions);
    }

    [Fact]
    public void Cannot_import_sample_qif_when_current_culture_is_ar_SA()
    {
        Assert.Throws<FormatException>(() => Load("ar-SA"));
    }

    [Fact]
    public void Can_import_sample_qif_when_current_culture_is_en_CA_with_CustomReadDateFormat()
    {
        QifDocument qif = Load("en-CA");
        Assert.NotEmpty(qif.BankTransactions);
    }

    [Fact]
    public void Does_not_change_income_category()
    {
        var sample = new QifDocument();
        sample.Categories.Add(new("Some Name")
        {
            IncomeCategory = true,
        });

        string? file = Path.GetTempFileName();
        sample.Save(file);

        var test = QifDocument.Load(file);

        Assert.Equal(sample.Categories.Count, test.Categories.Count);
        Assert.Equal(sample.Categories[0].IncomeCategory, test.Categories[0].IncomeCategory);
    }

    [Fact]
    public void Does_not_change_expense_category()
    {
        var sample = new QifDocument();
        sample.Categories.Add(new Category("name")
        {
            ExpenseCategory = true,
        });

        string? file = Path.GetTempFileName();
        sample.Save(file);

        var test = QifDocument.Load(file);

        Assert.Equal(sample.Categories.Count, test.Categories.Count);
        Assert.Equal(sample.Categories[0].ExpenseCategory, test.Categories[0].ExpenseCategory);
    }

    [Fact]
    public void Does_not_change_tax_related()
    {
        var sample = new QifDocument();
        sample.Categories.Add(new Category("name")
        {
            TaxRelated = true,
        });

        string? file = Path.GetTempFileName();
        sample.Save(file);

        var test = QifDocument.Load(file);

        Assert.Equal(sample.Categories.Count, test.Categories.Count);
        Assert.Equal(sample.Categories[0].TaxRelated, test.Categories[0].TaxRelated);
    }

    private static QifDocument Load(string cultureName) => QifDocument.Load(new QifReader(new StreamReader(GetSampleDataStream(SampleDataFile))) { FormatProvider = new CultureInfo(cultureName) });
}
