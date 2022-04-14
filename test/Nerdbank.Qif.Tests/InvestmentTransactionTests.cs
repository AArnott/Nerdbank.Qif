// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class InvestmentTransactionTests : TestBase
{
    public InvestmentTransactionTests(ITestOutputHelper logger)
        : base(logger)
    {
    }

    [Fact]
    public void Load_Simple()
    {
        const string qifSource = @"D10/27' 6
NCash
CR
U1,500.00
T1,500.00
LNetBank Checking
^";
        using QifReader reader = new(new StringReader(qifSource));
        InvestmentTransaction transaction = InvestmentTransaction.Load(reader);
        Assert.Equal(new DateTime(2006, 10, 27), transaction.Date);
        Assert.Equal("Cash", transaction.Action);
        Assert.Equal(ClearedState.Reconciled, transaction.ClearedStatus);
        Assert.Equal(1500, transaction.TransactionAmount);
        Assert.Equal("NetBank Checking", transaction.AccountForTransfer);
    }

    [Fact]
    public void Load_Exhaustive()
    {
        const string qifSource = @"D10/27' 6
NCash
CR
U1,500.00
T1,500.00
$2,500.00
O19.95
I16.06
Q0.611
PCash Contribution Prior Year
YFidelity International Real Estate
MCASH CONTRIBUTION PRIOR YEAR
LNetBank Checking
^";
        using QifReader reader = new(new StringReader(qifSource));
        InvestmentTransaction transaction = InvestmentTransaction.Load(reader);
        Assert.Equal(new DateTime(2006, 10, 27), transaction.Date);
        Assert.Equal("Cash", transaction.Action);
        Assert.Equal(ClearedState.Reconciled, transaction.ClearedStatus);
        Assert.Equal(1500, transaction.TransactionAmount);
        Assert.Equal("NetBank Checking", transaction.AccountForTransfer);
        Assert.Equal("CASH CONTRIBUTION PRIOR YEAR", transaction.Memo);
        Assert.Equal(19.95m, transaction.Commission);
        Assert.Equal(16.06m, transaction.Price);
        Assert.Equal(0.611m, transaction.Quantity);
        Assert.Equal("Fidelity International Real Estate", transaction.Security);
        Assert.Equal("Cash Contribution Prior Year", transaction.Payee);
        Assert.Equal(2500, transaction.AmountTransferred);
    }

    [Fact]
    public void Load_UnknownFields()
    {
        const string qifSource = @"D10/27' 6
NCash
CR
ZNotta # unrecognized field that should be skipped over
U1,500.00
T1,500.00
LNetBank Checking
^";
        using QifReader reader = new(new StringReader(qifSource));
        InvestmentTransaction transaction = InvestmentTransaction.Load(reader);
        Assert.Equal(new DateTime(2006, 10, 27), transaction.Date);
        Assert.Equal(1500, transaction.TransactionAmount);
    }
}
