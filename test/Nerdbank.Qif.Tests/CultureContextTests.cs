// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

public class CultureContextTests
{
    [Fact]
    public void Can_spoof_the_current_culture_on_the_current_thread()
    {
        var cultureBeforeSpoofing = new CultureInfo("en-US");
        var cultureToSpoof = new CultureInfo("es-MX");

        ////Assert.Equal(CultureInfo.CurrentCulture, cultureBeforeSpoofing);
        Assert.NotEqual(CultureInfo.CurrentCulture, cultureToSpoof);

        using (new CultureContext(cultureToSpoof))
        {
            Assert.Equal(CultureInfo.CurrentCulture, cultureToSpoof);
            Assert.NotEqual(CultureInfo.CurrentCulture, cultureBeforeSpoofing);
        }

        ////Assert.Equal(CultureInfo.CurrentCulture, cultureBeforeSpoofing);
        Assert.NotEqual(CultureInfo.CurrentCulture, cultureToSpoof);
    }
}
