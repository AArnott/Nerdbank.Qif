// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class AccountTests : TestBase
{
    public AccountTests(ITestOutputHelper logger)
        : base(logger)
    {
    }

    [Fact]
    public void Load_Simple()
    {
        const string qifSource = @"NMy name
TSome Type
^
";
        using QifReader reader = new(new StringReader(qifSource));
        Account account = Account.Load(reader);
        Assert.Equal("My name", account.Name);
        Assert.Equal("Some Type", account.Type);
    }

    [Fact]
    public void Load_Exhaustive()
    {
        const string qifSource = @"NMy name
TSome Type
DMy description
L1500
/03/01/2021
$1800
^
";
        using QifReader reader = new(new StringReader(qifSource));
        Account account = Account.Load(reader);
        Assert.Equal("My name", account.Name);
        Assert.Equal("My description", account.Description);
        Assert.Equal("Some Type", account.Type);
        Assert.Equal(1500, account.CreditLimit);
        Assert.Equal(new DateTime(2021, 3, 1), account.StatementBalanceDate);
        Assert.Equal(1800, account.StatementBalance);
    }
}
