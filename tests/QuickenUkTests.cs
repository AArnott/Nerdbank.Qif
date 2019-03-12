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
    }
}