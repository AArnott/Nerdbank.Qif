// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class BankTransactionTests : TestBase
{
    public BankTransactionTests(ITestOutputHelper logger)
        : base(logger)
    {
    }

    [Fact]
    public void Load_Simple()
    {
        const string qifSource = @"D1/1/2008
T1500
N123
PPaycheck
LIncome.Salary
^
";
        using QifReader reader = new(new StringReader(qifSource));
        BankTransaction transaction = BankTransaction.Load(reader);
        Assert.Equal(new DateTime(2008, 1, 1), transaction.Date);
        Assert.Equal(1500, transaction.Amount);
        Assert.Equal("123", transaction.Number);
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
    }

    [Fact]
    public void Load_Exhaustive()
    {
        const string qifSource = @"D1/1/2008
T1500
N123
CC
PPaycheck
LIncome.Salary
MMy memo
AName of receiver
A123 Deplorable Lane
ANowhere, CA 12345
SSplit1Cat
ESplit1Memo
$400
SSplit2Cat
ESplit2Memo
$600
SSplit3Cat
ESplit3Memo
$500
^
";
        using QifReader reader = new(new StringReader(qifSource));
        BankTransaction transaction = BankTransaction.Load(reader);
        Assert.Equal(1500, transaction.Amount);
        Assert.Equal("123", transaction.Number);
        Assert.Equal(ClearedState.Cleared, transaction.ClearedStatus);
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
        Assert.Equal("My memo", transaction.Memo);
        Assert.Equal(new[] { "Name of receiver", "123 Deplorable Lane", "Nowhere, CA 12345" }, transaction.Address);
        Assert.Equal(
            new BankSplit[]
            {
                new("Split1Cat", "Split1Memo") { Amount = 400 },
                new("Split2Cat", "Split2Memo") { Amount = 600 },
                new("Split3Cat", "Split3Memo") { Amount = 500 },
            },
            transaction.Splits);
    }

    [Fact]
    public void Load_UnknownFields()
    {
        const string qifSource = @"D1/1/2008
T1500
N123
PPaycheck
ZNotta # unrecognized field that should be skipped over
LIncome.Salary
^
";
        using QifReader reader = new(new StringReader(qifSource));
        BankTransaction transaction = BankTransaction.Load(reader);
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
    }
}
