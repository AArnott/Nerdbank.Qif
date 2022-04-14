// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class LocalizationTests
{
    [Fact]
    public void Can_read_resources()
    {
        Assert.NotNull(new BankTransaction(DateTime.Now, 10).ToString());
    }

    [Fact]
    public void Can_fallback_with_missing_culture()
    {
        using (new CultureContext("es-MX"))
        {
            Assert.NotNull(new BankTransaction(DateTime.Now, 10).ToString());
        }
    }
}
