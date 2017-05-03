using System;
using QifApi.Transactions;
using Xunit;

namespace QifApi.Tests
{
    public class LocalizationTests
    {
        [Fact]
        public void Can_read_resources()
        {
            Assert.NotNull(new BasicTransaction().ToString());
        }

        [Fact]
        public void Can_fallback_with_missing_culture()
        {
            using (new CultureContext("es-MX"))
            {
                Assert.NotNull(new BasicTransaction().ToString());
            }
        }
    }
}