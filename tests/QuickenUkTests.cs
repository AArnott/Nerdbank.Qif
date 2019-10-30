using System;
using System.Globalization;
using System.IO;
using Xunit;

namespace QifApi.Tests
{
    public class QuickenUkTests
    {
        [Fact]
        public void CanImportCreditCardTransaction()
        {
            var sample = @"!Type:CCard
D31/1/18
U-6.25
T-6.25
CX
PStarbucks
MCoffee & cake
LDining
^";

            QifDom parser = null;
            using (new CultureContext(new CultureInfo("en-GB")))
            using (var reader = new StringReader(sample))
            {
                parser = QifDom.ImportFile(reader);
            }

            var transaction = parser.CreditCardTransactions[0];
            Assert.Equal(new DateTime(2018, 1, 31), transaction.Date);
            Assert.Equal(-6.25M, transaction.Amount);
            Assert.Equal("X", transaction.ClearedStatus);
            Assert.Equal("Starbucks", transaction.Payee);
            Assert.Equal("Coffee & cake", transaction.Memo);
            Assert.Equal("Dining", transaction.Category);
        }

        [Fact]
        public void CanImportCreditCardSplit()
        {
            var sample = @"!Type:CCard
D31/1/18
U-6.25
T-6.25
CX
PCaffe Nero
LDining
SDining
ECoffee
$-2.75
SDining
ECake
$-3.50
^";

            QifDom parser = null;
            using (new CultureContext(new CultureInfo("en-GB")))
            using (var reader = new StringReader(sample))
            {
                parser = QifDom.ImportFile(reader);
            }

            var transaction = parser.CreditCardTransactions[0];

            // Transaction values.
            Assert.Equal(new DateTime(2018, 1, 31), transaction.Date);
            Assert.Equal(-6.25M, transaction.Amount);
            Assert.Equal("X", transaction.ClearedStatus);
            Assert.Equal("Caffe Nero", transaction.Payee);
            Assert.Equal("Dining", transaction.Category);

            // Split 1.
            Assert.Equal("Dining", transaction.SplitCategories[0]);
            Assert.Equal("Coffee", transaction.SplitMemos[0]);
            Assert.Equal(-2.75M, transaction.SplitAmounts[0]);

            // Split 2.
            Assert.Equal("Dining", transaction.SplitCategories[1]);
            Assert.Equal("Cake", transaction.SplitMemos[1]);
            Assert.Equal(-3.50M, transaction.SplitAmounts[1]);
        }

        [Fact]
        public void CanImportClass()
        {
            var sample = @"!Type:Class
NMyClassName
DMyClassDescription
^";

            QifDom parser = null;
            using (new CultureContext(new CultureInfo("en-GB")))
            using (var reader = new StringReader(sample))
            {
                parser = QifDom.ImportFile(reader);
            }

            var @class = parser.ClassListTransactions[0];
            Assert.Equal("MyClassName", @class.ClassName);
            Assert.Equal("MyClassDescription", @class.Description);
        }

        [Fact]
        public void CanImportCategory()
        {
            var sample = @"!Type:Cat
NEmployment
DEmployment income
T
I
^";

            QifDom parser = null;
            using (new CultureContext(new CultureInfo("en-GB")))
            using (var reader = new StringReader(sample))
            {
                parser = QifDom.ImportFile(reader);
            }

            var category = parser.CategoryListTransactions[0];
            Assert.Equal("Employment", category.CategoryName);
            Assert.Equal("Employment income", category.Description);
            Assert.True(category.IncomeCategory);
            Assert.False(category.ExpenseCategory);
            Assert.True(category.TaxRelated);
        }

        /// <summary>
        /// Quicken may preceed bank transactions with the account details to indicate those transactions belong to that account.
        /// </summary>
        [Fact]
        public void CanImportAccountWithTransactions()
        {
            var sample = @"!Clear:AutoSwitch
!Option:AutoSwitch
!Account
NBank Account 1
TBank
^
!Type:Bank 
D01/1/18
U-400.00
T-400.00
CX
NCard
PMr Land Lord
LRent
^";

            QifDom parser = null;
            using (new CultureContext(new CultureInfo("en-GB")))
            using (var reader = new StringReader(sample))
            {
                parser = QifDom.ImportFile(reader);
            }

            Assert.Equal(1, parser.AccountListTransactions.Count);
            Assert.Equal(1, parser.BankTransactions.Count);
            Assert.Equal("Bank Account 1", parser.BankTransactions[0].AccountName);
        }

        [Fact]
        public void CanExport()
        {
            var dom = new QifDom();
            dom.AccountListTransactions.Add(new Transactions.AccountListTransaction()
            {
                Name = "Account1"
            });

            var exported = dom.Export();

            System.Diagnostics.Debug.WriteLine(exported);
            var expected = @"!Account
L0
NAccount1
$0
/01/01/0001
^
";
            Assert.Equal(expected, exported);
        }
    }
}
