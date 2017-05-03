using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using QifApi.Config;
using Xunit;

namespace QifApi.Tests
{
    public class ImportTests
    {
        [Fact]
        public void Can_import_sample_qif_when_current_culture_is_en_US()
        {
            var sample = File.ReadAllText("sample.qif");

            using (new CultureContext(new CultureInfo("en-US")))
            using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sample))))
            {
                var qif = QifDom.ImportFile(reader);
                Assert.NotEmpty(qif.BankTransactions);
            }
        }

        [Fact]
        public void Cannot_import_sample_qif_when_explicitly_using_ar_SA()
        {
            var sample = File.ReadAllText("sample.qif");

            using (new CultureContext(new CultureInfo("en-US")))
            using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sample))))
            {
                Assert.Throws<InvalidCastException>(() =>
                {
                    QifDom.ImportFile(reader, new Configuration
                    {
                        CustomReadCultureInfo = new CultureInfo("ar-SA")
                    });
                });
            }
        }

        [Fact]
        public void Cannot_import_sample_qif_when_current_culture_is_ar_SA()
        {
            var sample = File.ReadAllText("sample.qif");

            using (new CultureContext(new CultureInfo("ar-SA")))
            using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sample))))
            {
                Assert.Throws<InvalidCastException>(() => QifDom.ImportFile(reader));
            }
        }

        [Fact]
        public void Can_import_sample_qif_when_current_culture_is_ar_SA_with_Custom()
        {
            var sample = File.ReadAllText("sample.qif");

            using (new CultureContext(new CultureInfo("ar-SA")))
            using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sample))))
            {
                var qif = QifDom.ImportFile(reader, new Configuration
                {
                    CustomReadCultureInfo = new CultureInfo("en-US")
                });
                Assert.NotEmpty(qif.BankTransactions);
            }
        }

        [Fact]
        public void Can_import_sample_qif_when_current_culture_is_en_CA_with_CustomReadDateFormat()
        {
            var sample = File.ReadAllText("sample.qif");

            using (new CultureContext(new CultureInfo("en-CA")))
            using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(sample))))
            {
                var qif = QifDom.ImportFile(reader, new Configuration
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
            var sample = new QifDom();
            sample.CategoryListTransactions.Add(new Transactions.CategoryListTransaction()
            {
                IncomeCategory = true,
            });

            var file = Path.GetTempFileName();
            sample.Export(file);

            var test = QifDom.ImportFile(file);

            Assert.Equal(sample.CategoryListTransactions.Count, test.CategoryListTransactions.Count);
            Assert.Equal(sample.CategoryListTransactions[0].IncomeCategory, test.CategoryListTransactions[0].IncomeCategory);
        }

        [Fact]
        public void Does_not_change_expense_category()
        {
            var sample = new QifDom();
            sample.CategoryListTransactions.Add(new Transactions.CategoryListTransaction()
            {
                ExpenseCategory = true,
            });

            var file = Path.GetTempFileName();
            sample.Export(file);

            var test = QifDom.ImportFile(file);

            Assert.Equal(sample.CategoryListTransactions.Count, test.CategoryListTransactions.Count);
            Assert.Equal(sample.CategoryListTransactions[0].ExpenseCategory, test.CategoryListTransactions[0].ExpenseCategory);
        }

        [Fact]
        public void Does_not_change_tax_related()
        {
            var sample = new QifDom();
            sample.CategoryListTransactions.Add(new Transactions.CategoryListTransaction()
            {
                TaxRelated = true,
            });

            var file = Path.GetTempFileName();
            sample.Export(file);

            var test = QifDom.ImportFile(file);

            Assert.Equal(sample.CategoryListTransactions.Count, test.CategoryListTransactions.Count);
            Assert.Equal(sample.CategoryListTransactions[0].TaxRelated, test.CategoryListTransactions[0].TaxRelated);
        }
    }
}
