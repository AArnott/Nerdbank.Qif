using System;
using System.Globalization;
using System.IO;
using System.Threading;
using NUnit.Framework;
using QifApi.Config;
using QifApi.Helpers;
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
        public void Cannot_import_sample_qif_when_explicitly_using_en_CA()
        {
            var sample = ResourceHelpers.ExtractResourceToString("QifApi.Tests.SampleData.sample.qif");

            using (new SpoofCulture(new CultureInfo("en-US")))
            using (var reader = new StreamReader(sample.ToUTF8MemoryStream()))
            {
                        Console.WriteLine(Thread.CurrentThread.CurrentCulture.DisplayName);
                        new QifDom(new Configuration
                            {
                                CustomReadCultureInfo = new CultureInfo("en-CA")
                            })
                            .Import(reader);
            }
        }

        [Test]
        public void Cannot_import_sample_qif_when_current_culture_is_en_CA()
        {
            var sample = ResourceHelpers.ExtractResourceToString("QifApi.Tests.SampleData.sample.qif");

            using (new SpoofCulture(new CultureInfo("en-CA")))
            using (var reader = new StreamReader(sample.ToUTF8MemoryStream()))
            {
                    Console.WriteLine(Thread.CurrentThread.CurrentCulture.DisplayName);
                    new QifDom().Import(reader);
            }
        }

        [Test]
        public void Can_import_sample_qif_when_current_culture_is_en_CA_with_Custom()
        {
            var sample = ResourceHelpers.ExtractResourceToString("QifApi.Tests.SampleData.sample.qif");

            using (new SpoofCulture(new CultureInfo("en-CA")))
            using (var reader = new StreamReader(sample.ToUTF8MemoryStream()))
            {
                new QifDom(new Configuration
                    {
                        CustomReadCultureInfo = new CultureInfo("en-US")
                    })
                    .Import(reader);
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
                    })
                    .Import(reader);
            }
        }
    }
}
