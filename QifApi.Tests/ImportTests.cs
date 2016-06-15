using System.Globalization;
using System.IO;
using NUnit.Framework;
using QifApi.Config;
using QifApi.Tests.Helpers;

namespace QifApi.Tests
{
    public class ImportTests
    {
        [Test]
        public void Can_import_sample_qif_when_current_culture_is_en_US()
        {
            var sample = ResourceHelpers.ExtractResourceToString("QifApi.Tests.SampleData.sample.qif");

            using (new SpoofCulture(new CultureInfo("en-US")))
            using (var reader = new StreamReader(sample.ToUTF8MemoryStream()))
            {
                new QifDom().Import(reader);
            }
        }

        [Test]
        public void Can_import_sample_qif_when_current_culture_is_en_CA()
        {
            var sample = ResourceHelpers.ExtractResourceToString("QifApi.Tests.SampleData.sample.qif");

            using (new SpoofCulture(new CultureInfo("en-CA")))
            using (var reader = new StreamReader(sample.ToUTF8MemoryStream()))
            {
                new QifDom().Import(reader);
            }
        }

        [Test]
        public void Can_import_sample_qif_when_current_culture_is_en_CA_with_CustomReadDateFormat()
        {
            var sample = ResourceHelpers.ExtractResourceToString("QifApi.Tests.SampleData.sample.qif");

            using (new SpoofCulture(new CultureInfo("en-CA")))
            using (var reader = new StreamReader(sample.ToUTF8MemoryStream()))
            {
                new QifDom(new Configuration
                    {
                        ReadDateFormatMode = ReadDateFormatMode.Custom,
                        CustomReadDateFormat = "M/d/yyyy"
                    }).Import(reader);
            }
        }
    }
}
