// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text;
using Nerdbank.Qif;

public class ImportTests
{
    [Fact]
    public void Can_import_sample_qif_when_current_culture_is_en_US()
    {
        string? sample = File.ReadAllText("sample.qif");

        using (new CultureContext(new CultureInfo("en-US")))
        using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sample))))
        {
            var qif = QifDocument.ImportFile(reader);
            Assert.NotEmpty(qif.BankTransactions);
        }
    }

    [Fact]
    public void Cannot_import_sample_qif_when_explicitly_using_ar_SA()
    {
        string? sample = File.ReadAllText("sample.qif");

        using (new CultureContext(new CultureInfo("en-US")))
        using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sample))))
        {
            Assert.Throws<InvalidCastException>(() =>
            {
                QifDocument.ImportFile(reader, new Configuration
                {
                    CustomReadCultureInfo = new CultureInfo("ar-SA"),
                });
            });
        }
    }

    [Fact]
    public void Cannot_import_sample_qif_when_current_culture_is_ar_SA()
    {
        string? sample = File.ReadAllText("sample.qif");

        using (new CultureContext(new CultureInfo("ar-SA")))
        using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sample))))
        {
            Assert.Throws<InvalidCastException>(() => QifDocument.ImportFile(reader));
        }
    }

    [Fact]
    public void Can_import_sample_qif_when_current_culture_is_ar_SA_with_Custom()
    {
        string? sample = File.ReadAllText("sample.qif");

        using (new CultureContext(new CultureInfo("ar-SA")))
        using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sample))))
        {
            var qif = QifDocument.ImportFile(reader, new Configuration
            {
                CustomReadCultureInfo = new CultureInfo("en-US"),
            });
            Assert.NotEmpty(qif.BankTransactions);
        }
    }

    [Fact]
    public void Can_import_sample_qif_when_current_culture_is_en_CA_with_CustomReadDateFormat()
    {
        string? sample = File.ReadAllText("sample.qif");

        using (new CultureContext(new CultureInfo("en-CA")))
        using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sample))))
        {
            var qif = QifDocument.ImportFile(reader, new Configuration
            {
                ReadDateFormatMode = ReadDateFormatMode.Custom,
                CustomReadDateFormat = "M/d/yyyy",
            });
            Assert.NotEmpty(qif.BankTransactions);
        }
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
        sample.Export(file);

        var test = QifDocument.ImportFile(file);

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
        sample.Export(file);

        var test = QifDocument.ImportFile(file);

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
        sample.Export(file);

        var test = QifDocument.ImportFile(file);

        Assert.Equal(sample.CategoryListTransactions.Count, test.CategoryListTransactions.Count);
        Assert.Equal(sample.CategoryListTransactions[0].TaxRelated, test.CategoryListTransactions[0].TaxRelated);
    }
}
