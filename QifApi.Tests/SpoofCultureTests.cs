using System.Globalization;
using System.Linq;
using NUnit.Framework;
using QifApi.Helpers;
using QifApi.Tests.Helpers;

namespace QifApi.Tests
{
    public class SpoofCultureTests
    {
        [Test]
        public void Can_spoof_the_current_culture_on_the_current_thread()
        {
            var cultureBeforeSpoofing = CultureInfo.CurrentCulture;

            var cultureToSpoof = CultureInfo.GetCultures(CultureTypes.SpecificCultures).First();
            if (cultureToSpoof.Equals(cultureBeforeSpoofing))
            {
                cultureToSpoof = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Skip(1).First();
            }

            Assert.AreEqual(CultureInfo.CurrentCulture, cultureBeforeSpoofing);
            Assert.AreNotEqual(CultureInfo.CurrentCulture, cultureToSpoof);

            using (new SpoofCulture(cultureToSpoof))
            {
                Assert.AreEqual(CultureInfo.CurrentCulture, cultureToSpoof);
                Assert.AreNotEqual(CultureInfo.CurrentCulture, cultureBeforeSpoofing);
            }

            Assert.AreEqual(CultureInfo.CurrentCulture, cultureBeforeSpoofing);
            Assert.AreNotEqual(CultureInfo.CurrentCulture, cultureToSpoof);
        }
    }
}