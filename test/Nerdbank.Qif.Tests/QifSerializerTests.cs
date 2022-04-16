// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Globalization;

public class QifSerializerTests : TestBase
{
    private static readonly DateTime Date = new(2013, 2, 3);
    private static readonly DateTime Date2 = new(2013, 2, 4);
    private readonly QifSerializer serializer = new QifSerializer();
    private readonly StringWriter stringWriter = new() { NewLine = "\n" };
    private readonly QifWriter qifWriter;

    public QifSerializerTests(ITestOutputHelper logger)
        : base(logger)
    {
        this.qifWriter = new(this.stringWriter) { FormatProvider = CultureInfo.InvariantCulture };
    }

    [Fact]
    public void ReadBankTransaction_Simple()
    {
        const string qifSource = @"D2/3/2013
T1500
N123
PPaycheck
LIncome.Salary
^
";
        BankTransaction transaction = Read(qifSource, this.serializer.ReadBankTransaction);
        Assert.Equal(Date, transaction.Date);
        Assert.Equal(1500, transaction.Amount);
        Assert.Equal("123", transaction.Number);
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
    }

    [Fact]
    public void ReadBankTransaction_Uk()
    {
        const string qifSource = @"D2/3/2013
T1500
N123
PPaycheck
LIncome.Salary
^
";
        BankTransaction transaction = Read(qifSource, this.serializer.ReadBankTransaction, "en-GB");
        Assert.Equal(new DateTime(2013, 3, 2), transaction.Date);
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
        BankTransaction transaction = Read(qifSource, this.serializer.ReadBankTransaction);
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
        BankTransaction transaction = Read(qifSource, this.serializer.ReadBankTransaction);
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
    }

    /// <summary>
    /// Quicken may preceed bank transactions with the account details to indicate those transactions belong to that account.
    /// </summary>
    [Fact]
    public void CanImportAccountWithTransactions()
    {
        string qifSource = @"!Clear:AutoSwitch
!Option:AutoSwitch
!Account
NBank Account 1
TBank
^
!Type:Bank 
D01/1/18
U-400.00
T-400.00
CX
NCard
PMr Land Lord
LRent
^";
        QifDocument document = Read(qifSource, this.serializer.ReadDocument);
        Assert.Single(document.Accounts);
        Assert.Single(document.BankTransactions);
        Assert.Equal("Bank Account 1", document.BankTransactions[0].AccountName);
    }

    [Fact]
    public void ReadMemorizedTransaction_Simple_TypeAtBottom()
    {
        const string qifSource = @"D2/3/2013
T1500
N123
PPaycheck
LIncome.Salary
KE
^
";
        MemorizedTransaction transaction = Read(qifSource, this.serializer.ReadMemorizedTransaction);
        Assert.Equal(Date, transaction.Date);
        Assert.Equal(1500, transaction.Amount);
        Assert.Equal("123", transaction.Number);
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
        Assert.Equal(MemorizedTransactionType.ElectronicPayee, transaction.Type);
    }

    [Fact]
    public void ReadMemorizedTransaction_Simple_TypeAtTop()
    {
        const string qifSource = @"D2/3/2013
KE
T1500
N123
PPaycheck
LIncome.Salary
^
";
        MemorizedTransaction transaction = Read(qifSource, this.serializer.ReadMemorizedTransaction);
        Assert.Equal(Date, transaction.Date);
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
102/04/2013
230
313
46
50.12
614
710000
^
";
        MemorizedTransaction transaction = Read(qifSource, this.serializer.ReadMemorizedTransaction);
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
        Assert.Equal(14, transaction.AmortizationCurrentLoanBalance);
        Assert.Equal(Date2, transaction.AmortizationFirstPaymentDate);
        Assert.Equal(.12m, transaction.AmortizationInterestRate);
        Assert.Equal(13, transaction.AmortizationNumberOfPaymentsAlreadyMade);
        Assert.Equal(6, transaction.AmortizationNumberOfPeriodsPerYear);
        Assert.Equal(10000, transaction.AmortizationOriginalLoanAmount);
        Assert.Equal(30, transaction.AmortizationTotalYearsForLoan);
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
        Category category = Read(qifSource, this.serializer.ReadCategory);
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
        Category category = Read(qifSource, this.serializer.ReadCategory);
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
        InvestmentTransaction transaction = Read(qifSource, this.serializer.ReadInvestmentTransaction);
        Assert.Equal(new DateTime(2006, 10, 27), transaction.Date);
        Assert.Equal("Cash", transaction.Action);
        Assert.Equal(ClearedState.Reconciled, transaction.ClearedStatus);
        Assert.Equal(1500, transaction.TransactionAmount);
        Assert.Equal("NetBank Checking", transaction.AccountForTransfer);
    }

    [Fact]
    public void ReadInvestmentTransaction_Exhaustive()
    {
        const string qifSource = @"D2/3/2013
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
        InvestmentTransaction transaction = Read(qifSource, this.serializer.ReadInvestmentTransaction);
        Assert.Equal(Date, transaction.Date);
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
        const string qifSource = @"D2/3/2013
NCash
CR
ZNotta # unrecognized field that should be skipped over
U1,500.00
T1,500.00
LNetBank Checking
^";
        InvestmentTransaction transaction = Read(qifSource, this.serializer.ReadInvestmentTransaction);
        Assert.Equal(Date, transaction.Date);
        Assert.Equal(1500, transaction.TransactionAmount);
    }

    [Fact]
    public void ReadAccount_Simple()
    {
        const string qifSource = @"NMy name
TSome Type
^
";
        Account account = Read(qifSource, this.serializer.ReadAccount);
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
/02/03/2013
$1800
^
";
        Account account = Read(qifSource, this.serializer.ReadAccount);
        Assert.Equal("My name", account.Name);
        Assert.Equal("My description", account.Description);
        Assert.Equal("Some Type", account.Type);
        Assert.Equal(1500, account.CreditLimit);
        Assert.Equal(Date, account.StatementBalanceDate);
        Assert.Equal(1800, account.StatementBalance);
    }

    [Fact]
    public void ReadClass_Simple()
    {
        const string qifSource = @"NBonus
^";
        Class clazz = Read(qifSource, this.serializer.ReadClass);
        Assert.Equal("Bonus", clazz.Name);
        Assert.Null(clazz.Description);
    }

    [Fact]
    public void ReadClass_Exhaustive()
    {
        const string qifSource = @"NBonus
DA bonus
^";
        Class clazz = Read(qifSource, this.serializer.ReadClass);
        Assert.Equal("Bonus", clazz.Name);
        Assert.Equal("A bonus", clazz.Description);
    }

    [Fact]
    public void ReadDocument()
    {
        const string qifSource = @"!Type:Bank
D02/03/2013
T9
^
D02/03/2013
T10
^
!Type:Oth A
D02/03/2013
T1
^
D02/03/2013
T2
^
!Type:Oth L
D02/03/2013
T3
^
D02/03/2013
T4
^
!Type:Cash
D02/03/2013
T5
^
D02/03/2013
T6
^
!Type:CCard
D02/03/2013
T7
^
D02/03/2013
T8
^
!Type:Invst
D02/03/2013
^
D02/04/2013
^
!Type:Memorized
KD
D02/03/2013
T10
^
KD
D02/04/2013
T10
^
!Type:Cat
Ncat1
^
Ncat2
^
!Type:Class
Nclass1
^
Nclass2
^
!Account
NAccount1
^
NAccount2
^
";

        QifDocument actual = Read(qifSource, this.serializer.ReadDocument);
        QifDocument expected = CreateSampleDocument();
        Assert.Equal<BankTransaction>(expected.BankTransactions, actual.BankTransactions);
    }

    [Fact]
    public void Write_Class()
    {
        this.AssertSerialized(
            new Class("my name"),
            "Nmy name\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new Class("my name") { Description = "my description" },
            "Nmy name\nDmy description\n^\n",
            this.serializer.Write);
    }

    [Fact]
    public void Write_Category()
    {
        this.AssertSerialized(
            new Category("my name"),
            "Nmy name\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new Category("my name") { BudgetAmount = 10, Description = "desc", ExpenseCategory = true, IncomeCategory = true, TaxRelated = true, TaxSchedule = "S" },
            "Nmy name\nDdesc\nT\nRS\nE\nI\nB10\n^\n",
            this.serializer.Write);
    }

    [Fact]
    public void Write_Account()
    {
        this.AssertSerialized(
            new Account("Account1"),
            "NAccount1\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new Account("Account1") { Description = "desc", Type = "Z", CreditLimit = 5, StatementBalance = 6, StatementBalanceDate = Date },
            "NAccount1\nTZ\nDdesc\nL5\n/02/03/2013\n$6\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new Account("Account1") { Description = "desc", Type = "Z", CreditLimit = 5, StatementBalance = 6, StatementBalanceDate = Date },
            "NAccount1\nTZ\nDdesc\nL5\n/03/02/2013\n$6\n^\n",
            this.serializer.Write,
            "en-GB");
    }

    [Fact]
    public void Write_BankTransaction()
    {
        this.AssertSerialized(
            new BankTransaction(Date, 35),
            "D02/03/2013\nT35\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new BankTransaction(Date, 35) { Address = ImmutableList.Create("addr", "city"), Category = "cat", ClearedStatus = ClearedState.Cleared, Memo = "memo", Number = "123", Payee = "payee" },
            "D02/03/2013\nT35\nN123\nMmemo\nLcat\nCC\nPpayee\nAaddr\nAcity\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new BankTransaction(Date, 35) { Splits = ImmutableList.Create<BankSplit>(new("cat1", "memo1") { Amount = 10 }, new("cat2", "memo2") { Percentage = 15 }) },
            "D02/03/2013\nT35\nScat1\nEmemo1\n$10\nScat2\nEmemo2\n%15\n^\n",
            this.serializer.Write);
    }

    [Fact]
    public void Write_MemorizedTransaction()
    {
        this.AssertSerialized(
            new MemorizedTransaction(MemorizedTransactionType.Check, Date, 35),
            "KC\nD02/03/2013\nT35\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new MemorizedTransaction(MemorizedTransactionType.Deposit, Date, 35) { Address = ImmutableList.Create("addr", "city"), Category = "cat", ClearedStatus = ClearedState.Cleared, Memo = "memo", Number = "123", Payee = "payee", AmortizationCurrentLoanBalance = 14, AmortizationFirstPaymentDate = Date2, AmortizationInterestRate = .12m, AmortizationNumberOfPaymentsAlreadyMade = 13, AmortizationNumberOfPeriodsPerYear = 6, AmortizationOriginalLoanAmount = 10000, AmortizationTotalYearsForLoan = 30 },
            "KD\nD02/03/2013\nT35\nN123\nMmemo\nLcat\nCC\nPpayee\nAaddr\nAcity\n102/04/2013\n230\n313\n46\n50.12\n614\n710000\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new MemorizedTransaction(MemorizedTransactionType.Payment, Date, 35) { Splits = ImmutableList.Create<BankSplit>(new("cat1", "memo1") { Amount = 10 }, new("cat2", "memo2") { Percentage = 15 }) },
            "KP\nD02/03/2013\nT35\nScat1\nEmemo1\n$10\nScat2\nEmemo2\n%15\n^\n",
            this.serializer.Write);
    }

    [Fact]
    public void Write_InvestmentTransaction()
    {
        this.AssertSerialized(
            new InvestmentTransaction(Date),
            "D02/03/2013\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new InvestmentTransaction(Date) { Commission = 10, ClearedStatus = ClearedState.Reconciled, AmountTransferred = 4, Action = "BUY", Memo = "memo", Security = "sec", Quantity = 3, Price = 11, Payee = "payee", AccountForTransfer = "ta", TransactionAmount = 22 },
            "D02/03/2013\nNBUY\nPpayee\nMmemo\nCR\nQ3\nYsec\nT22\nI11\nO10\n$4\nLta\n^\n",
            this.serializer.Write);
    }

    [Fact]
    public void Write_Document()
    {
        string qifSource = @"!Type:Bank
D02/03/2013
T9
^
D02/03/2013
T10
^
!Type:Oth A
D02/03/2013
T1
^
D02/03/2013
T2
^
!Type:Oth L
D02/03/2013
T3
^
D02/03/2013
T4
^
!Type:Cash
D02/03/2013
T5
^
D02/03/2013
T6
^
!Type:CCard
D02/03/2013
T7
^
D02/03/2013
T8
^
!Type:Invst
D02/03/2013
^
D02/04/2013
^
!Type:Memorized
KD
D02/03/2013
T10
^
KD
D02/04/2013
T10
^
!Type:Cat
Ncat1
^
Ncat2
^
!Type:Class
Nclass1
^
Nclass2
^
!Account
NAccount1
^
NAccount2
^
";
        this.AssertSerialized(
            CreateSampleDocument(),
            qifSource,
            this.serializer.Write);
    }

    private static QifDocument CreateSampleDocument()
    {
        return new()
        {
            Accounts =
            {
                new Account("Account1"),
                new Account("Account2"),
            },
            AssetTransactions =
            {
                new BankTransaction(Date, 1),
                new BankTransaction(Date, 2),
            },
            LiabilityTransactions =
            {
                new BankTransaction(Date, 3),
                new BankTransaction(Date, 4),
            },
            CashTransactions =
            {
                new BankTransaction(Date, 5),
                new BankTransaction(Date, 6),
            },
            CreditCardTransactions =
            {
                new BankTransaction(Date, 7),
                new BankTransaction(Date, 8),
            },
            BankTransactions =
            {
                new BankTransaction(Date, 9),
                new BankTransaction(Date, 10),
            },
            MemorizedTransactions =
            {
                new MemorizedTransaction(MemorizedTransactionType.Deposit, Date, 10),
                new MemorizedTransaction(MemorizedTransactionType.Deposit, Date2, 10),
            },
            InvestmentTransactions =
            {
                new InvestmentTransaction(Date),
                new InvestmentTransaction(Date2),
            },
            Categories =
            {
                new Category("cat1"),
                new Category("cat2"),
            },
            Classes =
            {
                new Class("class1"),
                new Class("class2"),
            },
        };
    }

    private static T Read<T>(string qifSource, Func<QifReader, T> readMethod, string? culture = null)
    {
        using QifReader reader = new(new StringReader(qifSource))
        {
            FormatProvider = culture is null ? CultureInfo.InvariantCulture : new CultureInfo(culture),
        };
        reader.MoveNext();
        return readMethod(reader);
    }

    private void AssertSerialized<T>(T record, string expected, Action<QifWriter, T> recordWriter, string? culture = null)
    {
        this.stringWriter.GetStringBuilder().Clear();
        this.qifWriter.FormatProvider = culture is null ? CultureInfo.InvariantCulture : new CultureInfo(culture);
        recordWriter(this.qifWriter, record);
        Assert.Equal(expected.Replace("\r\n", "\n"), this.stringWriter.ToString());
    }
}
