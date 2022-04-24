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
        BankTransaction transaction = Read(qifSource, r => this.serializer.ReadBankTransaction(r, AccountType.Bank));
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
        BankTransaction transaction = Read(qifSource, r => this.serializer.ReadBankTransaction(r, AccountType.Bank), "en-GB");
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
        BankTransaction transaction = Read(qifSource, r => this.serializer.ReadBankTransaction(r, AccountType.Bank));
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

    /// <summary>
    /// Asserts that when memos are omitted from a split line item, we still track subsequent memos the way Quicken intended.
    /// </summary>
    [Fact]
    public void ReadBankTransaction_FewerSplitMemos()
    {
        const string qifSource = @"D1/1/2008
T1500
SSplit1Cat
ESplit1Memo
$400
SSplit2Cat
ESplit2Memo
$600
SSplit3Cat
$300
SSplit4Cat
ESplit4Memo
$200
^
";
        BankTransaction transaction = Read(qifSource, r => this.serializer.ReadBankTransaction(r, AccountType.Bank));
        Assert.Equal(
            new BankSplit[]
            {
                new("Split1Cat", "Split1Memo") { Amount = 400 },
                new("Split2Cat", "Split2Memo") { Amount = 600 },
                new("Split3Cat", null) { Amount = 300 },
                new("Split4Cat", "Split4Memo") { Amount = 200 },
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
        BankTransaction transaction = Read(qifSource, r => this.serializer.ReadBankTransaction(r, AccountType.Bank));
        Assert.Equal("Paycheck", transaction.Payee);
        Assert.Equal("Income.Salary", transaction.Category);
    }

    /// <summary>
    /// Quicken may preceed bank transactions with the account details to indicate those transactions belong to that account.
    /// </summary>
    [Fact]
    public void CanImportAccountsWithTransactions()
    {
        string qifSource = @"!Account
NBank Account 1
TBank
^
!Type:Bank 
D01/1/18
T-400.00
^
!Account
NInv Account 1
TInvst
^
!Type:Invst
D01/1/19
T-400.00
^
";
        QifDocument document = Read(qifSource, this.serializer.ReadDocument);
        Assert.Empty(document.Transactions);
        Assert.Equal(2, document.Accounts.Count);
        BankAccount bankAccount = Assert.IsType<BankAccount>(document.Accounts[0]);
        InvestmentAccount investmentAccount = Assert.IsType<InvestmentAccount>(document.Accounts[1]);
        Assert.Equal(new DateTime(2018, 1, 1), Assert.Single(bankAccount.Transactions).Date);
        Assert.Equal(new DateTime(2019, 1, 1), Assert.Single(investmentAccount.Transactions).Date);
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
    public void ReadMemorizedTransaction_RequiredOnlyFields()
    {
        const string qifSource = @"KP
^
";
        MemorizedTransaction transaction = Read(qifSource, this.serializer.ReadMemorizedTransaction);
        Assert.Null(transaction.Date);
        Assert.Null(transaction.Amount);
        Assert.Null(transaction.Number);
        Assert.Null(transaction.Payee);
        Assert.Null(transaction.Category);
        Assert.Equal(MemorizedTransactionType.Payment, transaction.Type);
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

    /// <summary>
    /// Asserts that when memos are omitted from a split line item, we still track subsequent memos the way Quicken intended.
    /// </summary>
    [Fact]
    public void ReadMemorizedTransaction_FewerSplitMemos()
    {
        const string qifSource = @"D1/1/2008
KE
T1500
SSplit1Cat
ESplit1Memo
$400
SSplit2Cat
ESplit2Memo
$600
SSplit3Cat
$300
SSplit4Cat
ESplit4Memo
$200
^
";
        MemorizedTransaction transaction = Read(qifSource, r => this.serializer.ReadMemorizedTransaction(r));
        Assert.Equal(
            new BankSplit[]
            {
                new("Split1Cat", "Split1Memo") { Amount = 400 },
                new("Split2Cat", "Split2Memo") { Amount = 600 },
                new("Split3Cat", null) { Amount = 300 },
                new("Split4Cat", "Split4Memo") { Amount = 200 },
            },
            transaction.Splits);
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
        Assert.Null(category.BudgetAmount);
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
    public void ReadSecurity_Minimal()
    {
        const string qifSource = @"NMFS Growth Allocation Fund Class R4
^
";
        Security security = Read(qifSource, this.serializer.ReadSecurity);
        Assert.Equal("MFS Growth Allocation Fund Class R4", security.Name);
    }

    [Fact]
    public void ReadSecurity_Exhaustive()
    {
        const string qifSource = @"NMFS Growth Allocation Fund Class R4
SMAGJX
TMutual Fund
^
";
        Security security = Read(qifSource, this.serializer.ReadSecurity);
        Assert.Equal("MFS Growth Allocation Fund Class R4", security.Name);
        Assert.Equal("MAGJX", security.Symbol);
        Assert.Equal("Mutual Fund", security.Type);
    }

    [Fact]
    public void ReadPrice()
    {
        const string qifSource = @"""BEXFX"",11.84,"" 3/ 5'15""
^
";
        Price price = Read(qifSource, this.serializer.ReadPrice);
        Assert.Equal("BEXFX", price.Symbol);
        Assert.Equal(11.84m, price.Value);
        Assert.Equal(new DateTime(2015, 3, 5), price.Date);
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
    public void ReadAccount_InvestmentType()
    {
        const string qifSource = @"NMy name
TInvst
^
";
        Account account = Read(qifSource, this.serializer.ReadAccount);
        Assert.Equal("Invst", account.Type);
        Assert.Equal(AccountType.Investment, account.AccountType);
    }

    [Fact]
    public void ReadAccount_PortType()
    {
        const string qifSource = @"NMy name
TPort
^
";
        Account account = Read(qifSource, this.serializer.ReadAccount);
        Assert.Equal("Port", account.Type);
        Assert.Equal(AccountType.Investment, account.AccountType);
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
TBank
^
NAccount2
TBank
^
!Type:Security
Nsecurity1
^
Nsecurity2
^
!Type:Prices
""BEXFX"",11.52,"" 2/ 3'13""
^
""BEXFX"",11.53,"" 2/ 4'13""
^
";

        QifDocument actual = Read(qifSource, this.serializer.ReadDocument);
        QifDocument expected = CreateSampleDocument();
        Assert.Equal<Transaction>(expected.Transactions, actual.Transactions);
        Assert.Equal<Security>(expected.Securities, actual.Securities);
        Assert.Equal<Category>(expected.Categories, actual.Categories);
        Assert.Equal<Class>(expected.Classes, actual.Classes);
        Assert.Equal<Price>(expected.Prices, actual.Prices);
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
    public void Write_Security()
    {
        this.AssertSerialized(
            new Security("my name"),
            "Nmy name\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new Security("my name") { Symbol = "Sym", Type = "Typ" },
            "Nmy name\nSSym\nTTyp\n^\n",
            this.serializer.Write);
    }

    [Fact]
    public void Write_Price()
    {
        this.AssertSerialized(
            new Price("COMPX", 2756.42m, Date),
            "\"COMPX\",2756.42,\"02/03/2013\"\n^\n",
            this.serializer.Write);

        // Write another price to prove that the line will not begin with a comma.
        this.AssertSerialized(
            new Price("COMPX", 2756.43m, Date2),
            "\"COMPX\",2756.43,\"02/04/2013\"\n^\n",
            this.serializer.Write);
    }

    [Fact]
    public void Write_Account()
    {
        this.AssertSerialized(
            new BankAccount(Account.Types.Bank, "Account1"),
            "NAccount1\nTBank\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new BankAccount(Account.Types.Investment, "Account1"),
            "NAccount1\nTInvst\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new BankAccount(Account.Types.Investment2, "Account1"),
            "NAccount1\nTPort\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new BankAccount("Z", "Account1") { Description = "desc", CreditLimit = 5, StatementBalance = 6, StatementBalanceDate = Date },
            "NAccount1\nTZ\nDdesc\nL5\n/02/03/2013\n$6\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new BankAccount("Z", "Account1") { Description = "desc", CreditLimit = 5, StatementBalance = 6, StatementBalanceDate = Date },
            "NAccount1\nTZ\nDdesc\nL5\n/03/02/2013\n$6\n^\n",
            this.serializer.Write,
            "en-GB");
    }

    [Fact]
    public void Write_BankAccount_WithTransactions()
    {
        this.AssertSerialized(
            new BankAccount(Account.Types.Cash, "Account1")
            {
                Transactions =
                {
                    new(AccountType.Cash, Date, 15),
                    new(AccountType.Cash, Date2, 16),
                },
            },
            "NAccount1\nTCash\n^\n!Type:Cash\nD02/03/2013\nT15\n^\nD02/04/2013\nT16\n^\n",
            (writer, account) => this.serializer.Write(writer, account, includeTransactions: true));
    }

    [Fact]
    public void Write_InvestmentAccount_WithTransactions()
    {
        this.AssertSerialized(
            new InvestmentAccount(Account.Types.Investment, "Account1")
            {
                Transactions =
                {
                    new(Date),
                    new(Date2),
                },
            },
            "NAccount1\nTInvst\n^\n!Type:Invst\nD02/03/2013\n^\nD02/04/2013\n^\n",
            (writer, account) => this.serializer.Write(writer, account, includeTransactions: true));
    }

    [Fact]
    public void Write_BankTransaction()
    {
        this.AssertSerialized(
            new BankTransaction(AccountType.Bank, Date, 35),
            "D02/03/2013\nT35\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new BankTransaction(AccountType.Bank, Date, 35) { Address = ImmutableList.Create("addr", "city"), Category = "cat", ClearedStatus = ClearedState.Cleared, Memo = "memo", Number = "123", Payee = "payee" },
            "D02/03/2013\nT35\nN123\nMmemo\nLcat\nCC\nPpayee\nAaddr\nAcity\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new BankTransaction(AccountType.Bank, Date, 35) { Splits = ImmutableList.Create<BankSplit>(new("cat1", "memo1") { Amount = 10 }, new("cat2", "memo2") { Percentage = 15 }) },
            "D02/03/2013\nT35\nScat1\nEmemo1\n$10\nScat2\nEmemo2\n%15\n^\n",
            this.serializer.Write);
    }

    [Fact]
    public void Write_MemorizedTransaction()
    {
        this.AssertSerialized(
            new MemorizedTransaction(MemorizedTransactionType.Check) { Date = Date, Amount = 35 },
            "KC\nD02/03/2013\nT35\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new MemorizedTransaction(MemorizedTransactionType.Deposit) { Date = Date, Amount = 35, Address = ImmutableList.Create("addr", "city"), Category = "cat", ClearedStatus = ClearedState.Cleared, Memo = "memo", Number = "123", Payee = "payee", AmortizationCurrentLoanBalance = 14, AmortizationFirstPaymentDate = Date2, AmortizationInterestRate = .12m, AmortizationNumberOfPaymentsAlreadyMade = 13, AmortizationNumberOfPeriodsPerYear = 6, AmortizationOriginalLoanAmount = 10000, AmortizationTotalYearsForLoan = 30 },
            "KD\nD02/03/2013\nT35\nN123\nMmemo\nLcat\nCC\nPpayee\nAaddr\nAcity\n102/04/2013\n230\n313\n46\n50.12\n614\n710000\n^\n",
            this.serializer.Write);
        this.AssertSerialized(
            new MemorizedTransaction(MemorizedTransactionType.Payment) { Date = Date, Amount = 35, Splits = ImmutableList.Create<BankSplit>(new("cat1", "memo1") { Amount = 10 }, new("cat2", "memo2") { Percentage = 15 }) },
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
        string expected = @"!Type:Bank
D02/03/2013
T9
^
D02/03/2013
T10
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
!Type:Security
Nsecurity1
^
Nsecurity2
^
!Type:Prices
""BEXFX"",11.52,""02/03/2013""
^
""BEXFX"",11.53,""02/04/2013""
^
!Account
NAccount1
TBank
^
NAccount2
TBank
^
";
        this.AssertSerialized(
            CreateSampleDocument(),
            expected,
            this.serializer.Write);
    }

    [Fact]
    public void Write_Document_WithAccountAssociatedTransactions()
    {
        string expected = @"!Account
NAccount1
TCCard
^
!Type:CCard
D02/03/2013
T15
^
D02/04/2013
T16
^
!Account
NAccount2
TInvst
^
!Type:Invst
D02/03/2013
^
D02/04/2013
^
";
        QifDocument document = new()
        {
            Accounts =
            {
                new BankAccount(Account.Types.CreditCard, "Account1")
                {
                    Transactions =
                    {
                        new BankTransaction(AccountType.CreditCard, Date, 15),
                        new BankTransaction(AccountType.CreditCard, Date2, 16),
                    },
                },
                new InvestmentAccount(Account.Types.Investment, "Account2")
                {
                    Transactions =
                    {
                        new InvestmentTransaction(Date),
                        new InvestmentTransaction(Date2),
                    },
                },
            },
        };
        this.AssertSerialized(document, expected, this.serializer.Write);
    }

    private static QifDocument CreateSampleDocument()
    {
        return new()
        {
            Accounts =
            {
                new BankAccount(Account.Types.Bank, "Account1"),
                new BankAccount(Account.Types.Bank, "Account2"),
            },
            Transactions =
            {
                new BankTransaction(AccountType.Bank, Date, 9),
                new BankTransaction(AccountType.Bank, Date, 10),
                new BankTransaction(AccountType.Cash, Date, 5),
                new BankTransaction(AccountType.Cash, Date, 6),
                new BankTransaction(AccountType.CreditCard, Date, 7),
                new BankTransaction(AccountType.CreditCard, Date, 8),
                new InvestmentTransaction(Date),
                new InvestmentTransaction(Date2),
                new BankTransaction(AccountType.Asset, Date, 1),
                new BankTransaction(AccountType.Asset, Date, 2),
                new BankTransaction(AccountType.Liability, Date, 3),
                new BankTransaction(AccountType.Liability, Date, 4),
            },
            MemorizedTransactions =
            {
                new MemorizedTransaction(MemorizedTransactionType.Deposit) { Date = Date, Amount = 10 },
                new MemorizedTransaction(MemorizedTransactionType.Deposit) { Date = Date2, Amount = 10 },
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
            Securities =
            {
                new Security("security1"),
                new Security("security2"),
            },
            Prices =
            {
                new Price("BEXFX", 11.52m, Date),
                new Price("BEXFX", 11.53m, Date2),
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
