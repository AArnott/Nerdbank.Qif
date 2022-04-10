// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

public class ImportTests : TestBase
{
    [Fact]
    public void Can_import_sample_qif_when_current_culture_is_en_US()
    {
        using CultureContext culture = new(new CultureInfo("en-US"));
        using StreamReader reader = new(GetSampleDataStream(SampleDataFile));
        var qif = QifDocument.Load(reader);
        Assert.NotEmpty(qif.BankTransactions);
    }

    [Fact]
    public void Cannot_import_sample_qif_when_explicitly_using_ar_SA()
    {
        using CultureContext culture = new(new CultureInfo("en-US"));
        using StreamReader reader = new(GetSampleDataStream(SampleDataFile));
        Assert.Throws<InvalidCastException>(() => QifDocument.Load(reader, new Configuration { CustomReadCultureInfo = new CultureInfo("ar-SA") }));
    }

    [Fact]
    public void Cannot_import_sample_qif_when_current_culture_is_ar_SA()
    {
        using CultureContext culture = new(new CultureInfo("ar-SA"));
        using StreamReader reader = new(GetSampleDataStream(SampleDataFile));
        Assert.Throws<InvalidCastException>(() => QifDocument.Load(reader));
    }

    [Fact]
    public void Can_import_sample_qif_when_current_culture_is_ar_SA_with_Custom()
    {
        using CultureContext culture = new(new CultureInfo("ar-SA"));
        using StreamReader reader = new(GetSampleDataStream(SampleDataFile));
        var qif = QifDocument.Load(reader, new Configuration
        {
            CustomReadCultureInfo = new CultureInfo("en-US"),
        });
        Assert.NotEmpty(qif.BankTransactions);
    }

    [Fact]
    public void Can_import_sample_qif_when_current_culture_is_en_CA_with_CustomReadDateFormat()
    {
        using CultureContext culture = new(new CultureInfo("en-CA"));
        using StreamReader reader = new(GetSampleDataStream(SampleDataFile));
        var qif = QifDocument.Load(reader, new Configuration
        {
            ReadDateFormatMode = ReadDateFormatMode.Custom,
            CustomReadDateFormat = "M/d/yyyy",
        });
        Assert.NotEmpty(qif.BankTransactions);
    }

    [Fact]
    public void Does_not_change_income_category()
    {
        var sample = new QifDocument();
        sample.CategoryListTransactions.Add(new CategoryListTransaction()
        {
            IncomeCategory = true,
        });

        string? file = Path.GetTempFileName();
        sample.Save(file);

        var test = QifDocument.Load(file);

        Assert.Equal(sample.CategoryListTransactions.Count, test.CategoryListTransactions.Count);
        Assert.Equal(sample.CategoryListTransactions[0].IncomeCategory, test.CategoryListTransactions[0].IncomeCategory);
    }

    [Fact]
    public void Does_not_change_expense_category()
    {
        var sample = new QifDocument();
        sample.CategoryListTransactions.Add(new CategoryListTransaction()
        {
            ExpenseCategory = true,
        });

        string? file = Path.GetTempFileName();
        sample.Save(file);

        var test = QifDocument.Load(file);

        Assert.Equal(sample.CategoryListTransactions.Count, test.CategoryListTransactions.Count);
        Assert.Equal(sample.CategoryListTransactions[0].ExpenseCategory, test.CategoryListTransactions[0].ExpenseCategory);
    }

    [Fact]
    public void Does_not_change_tax_related()
    {
        var sample = new QifDocument();
        sample.CategoryListTransactions.Add(new CategoryListTransaction()
        {
            TaxRelated = true,
        });

        string? file = Path.GetTempFileName();
        sample.Save(file);

        var test = QifDocument.Load(file);

        Assert.Equal(sample.CategoryListTransactions.Count, test.CategoryListTransactions.Count);
        Assert.Equal(sample.CategoryListTransactions[0].TaxRelated, test.CategoryListTransactions[0].TaxRelated);
    }
}
