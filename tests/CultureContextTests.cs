using System.Globalization;
using System.Linq;
using Xunit;

namespace QifApi.Tests
{
    public class CultureContextTests
    {
        [Fact]
        public void Can_spoof_the_current_culture_on_the_current_thread()
        {
            var cultureBeforeSpoofing = new CultureInfo("en-US");
            var cultureToSpoof = new CultureInfo("es-MX");

            //Assert.Equal(CultureInfo.CurrentCulture, cultureBeforeSpoofing);
            Assert.NotEqual(CultureInfo.CurrentCulture, cultureToSpoof);

            using (new CultureContext(cultureToSpoof))
            {
                Assert.Equal(CultureInfo.CurrentCulture, cultureToSpoof);
                Assert.NotEqual(CultureInfo.CurrentCulture, cultureBeforeSpoofing);
            }

            //Assert.Equal(CultureInfo.CurrentCulture, cultureBeforeSpoofing);
            Assert.NotEqual(CultureInfo.CurrentCulture, cultureToSpoof);
        }
    }
}