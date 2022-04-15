// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class QifSerializerTests : TestBase
{
    private readonly QifSerializer serializer = new QifSerializer();
    private readonly StringWriter stringWriter = new() { NewLine = "\n" };
    private readonly QifWriter qifWriter;

    public QifSerializerTests(ITestOutputHelper logger)
        : base(logger)
    {
        this.qifWriter = new(this.stringWriter);
    }

    [Fact]
    public void ReadBankTransaction_Simple()
    {
        const string qifSource = @"D1/1/2008
T1500
N123
PPaycheck
LIncome.Salary
^
";
        using QifReader reader = new(new StringReader(qifSource));
        BankTransaction transaction = this.serializer.ReadBankTransaction(reader);
        Assert.Equal(new DateTime(2008, 1, 1), transaction.Date);
        Assert.Equal(1500, transaction.Amount);
        Assert.Equal("123", transaction.Number);
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
    }

    [Fact]
    public void ReadBankTransaction_Exhaustive()
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
        BankTransaction transaction = this.serializer.ReadBankTransaction(reader);
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
    public void ReadBankTransaction_UnknownFields()
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
        BankTransaction transaction = this.serializer.ReadBankTransaction(reader);
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
    }

    [Fact]
    public void ReadMemorizedTransaction_Simple_TypeAtBottom()
    {
        const string qifSource = @"D1/1/2008
T1500
N123
PPaycheck
LIncome.Salary
KE
^
";
        using QifReader reader = new(new StringReader(qifSource));
        MemorizedTransaction transaction = this.serializer.ReadMemorizedTransaction(reader);
        Assert.Equal(new DateTime(2008, 1, 1), transaction.Date);
        Assert.Equal(1500, transaction.Amount);
        Assert.Equal("123", transaction.Number);
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
        Assert.Equal(MemorizedTransactionType.ElectronicPayee, transaction.Type);
    }

    [Fact]
    public void ReadMemorizedTransaction_Simple_TypeAtTop()
    {
        const string qifSource = @"D1/1/2008
KE
T1500
N123
PPaycheck
LIncome.Salary
^
";
        using QifReader reader = new(new StringReader(qifSource));
        MemorizedTransaction transaction = this.serializer.ReadMemorizedTransaction(reader);
        Assert.Equal(new DateTime(2008, 1, 1), transaction.Date);
        Assert.Equal(1500, transaction.Amount);
        Assert.Equal("123", transaction.Number);
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
        Assert.Equal(MemorizedTransactionType.ElectronicPayee, transaction.Type);
    }

    [Fact]
    public void ReadMemorizedTransaction_Exhaustive()
    {
        const string qifSource = @"KP
D1/1/2008
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
        MemorizedTransaction transaction = this.serializer.ReadMemorizedTransaction(reader);
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
        Assert.Equal(MemorizedTransactionType.Payment, transaction.Type);
    }

    [Fact]
    public void ReadCategory_Income()
    {
        const string qifSource = @"NBonus
DBonus Income
T
R460
I
^";
        using QifReader reader = new(new StringReader(qifSource));
        Category category = this.serializer.ReadCategory(reader);
        Assert.Equal("Bonus", category.Name);
        Assert.Equal("Bonus Income", category.Description);
        Assert.True(category.TaxRelated);
        Assert.True(category.IncomeCategory);
        Assert.False(category.ExpenseCategory);
        Assert.Equal("460", category.TaxSchedule);
    }

    [Fact]
    public void ReadCategory_Expense()
    {
        const string qifSource = @"NGardening
E
B30.20
^";
        using QifReader reader = new(new StringReader(qifSource));
        Category category = this.serializer.ReadCategory(reader);
        Assert.Equal("Gardening", category.Name);
        Assert.Null(category.Description);
        Assert.False(category.TaxRelated);
        Assert.True(category.ExpenseCategory);
        Assert.False(category.IncomeCategory);
        Assert.Equal(30.20m, category.BudgetAmount);
    }

    [Fact]
    public void ReadInvestmentTransaction_Simple()
    {
        const string qifSource = @"D10/27' 6
NCash
CR
U1,500.00
T1,500.00
LNetBank Checking
^";
        using QifReader reader = new(new StringReader(qifSource));
        InvestmentTransaction transaction = this.serializer.ReadInvestmentTransaction(reader);
        Assert.Equal(new DateTime(2006, 10, 27), transaction.Date);
        Assert.Equal("Cash", transaction.Action);
        Assert.Equal(ClearedState.Reconciled, transaction.ClearedStatus);
        Assert.Equal(1500, transaction.TransactionAmount);
        Assert.Equal("NetBank Checking", transaction.AccountForTransfer);
    }

    [Fact]
    public void ReadInvestmentTransaction_Exhaustive()
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
        InvestmentTransaction transaction = this.serializer.ReadInvestmentTransaction(reader);
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
    public void ReadInvestmentTransaction_UnknownFields()
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
        InvestmentTransaction transaction = this.serializer.ReadInvestmentTransaction(reader);
        Assert.Equal(new DateTime(2006, 10, 27), transaction.Date);
        Assert.Equal(1500, transaction.TransactionAmount);
    }

    [Fact]
    public void ReadAccount_Simple()
    {
        const string qifSource = @"NMy name
TSome Type
^
";
        using QifReader reader = new(new StringReader(qifSource));
        Account account = this.serializer.ReadAccount(reader);
        Assert.Equal("My name", account.Name);
        Assert.Equal("Some Type", account.Type);
    }

    [Fact]
    public void ReadAccount_Exhaustive()
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
        Account account = this.serializer.ReadAccount(reader);
        Assert.Equal("My name", account.Name);
        Assert.Equal("My description", account.Description);
        Assert.Equal("Some Type", account.Type);
        Assert.Equal(1500, account.CreditLimit);
        Assert.Equal(new DateTime(2021, 3, 1), account.StatementBalanceDate);
        Assert.Equal(1800, account.StatementBalance);
    }

    [Fact]
    public void ReadClass_Simple()
    {
        const string qifSource = @"NBonus
^";
        using QifReader reader = new(new StringReader(qifSource));
        Class clazz = this.serializer.ReadClass(reader);
        Assert.Equal("Bonus", clazz.Name);
        Assert.Null(clazz.Description);
    }

    [Fact]
    public void ReadClass_Exhaustive()
    {
        const string qifSource = @"NBonus
DA bonus
^";
        using QifReader reader = new(new StringReader(qifSource));
        Class clazz = this.serializer.ReadClass(reader);
        Assert.Equal("Bonus", clazz.Name);
        Assert.Equal("A bonus", clazz.Description);
    }

    [Fact]
    public void Write_Class_Exhaustive()
    {
        this.AssertSerialized(
            new Class("my name") { Description = "my description" },
            "Nmy name\nDmy description\n^\n",
            this.serializer.Write);
    }

    [Fact]
    public void Write_Class_Minimal()
    {
        this.AssertSerialized(
            new Class("my name"),
            "Nmy name\n^\n",
            this.serializer.Write);
    }

    [Fact]
    public void Write_Account_Minimal()
    {
        this.AssertSerialized(
            new Account("Account1"),
            "NAccount1\n^\n",
            this.serializer.Write);
    }

    private void AssertSerialized<T>(T record, string expected, Action<QifWriter, T> recordWriter)
    {
        this.stringWriter.GetStringBuilder().Clear();
        recordWriter(this.qifWriter, record);
        Assert.Equal(expected, this.stringWriter.ToString());
    }
}
