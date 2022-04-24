// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Validation;

public class QifReaderTests : TestBase
{
    private QifReader reader = new(new StreamReader(GetSampleDataStream(ReaderInputsDataFile)));

    public QifReaderTests(ITestOutputHelper logger)
            : base(logger)
    {
        Assert.Equal(QifToken.BOF, this.reader.Kind);
        this.reader.MoveNext();
    }

    [Fact]
    public void DisposeDisposesReader()
    {
        StringReader sr = new(string.Empty);
        QifReader reader = new(new QifParser(sr));
        reader.Dispose();
        Assert.Throws<ObjectDisposedException>(() => sr.Read());
    }

    [Fact]
    public void Ctor_ValidatesArgs()
    {
        Assert.Throws<ArgumentNullException>(() => new QifReader((QifParser)null!));
        Assert.Throws<ArgumentNullException>(() => new QifReader((TextReader)null!));
    }

    [Fact]
    public void TryRead_AnticipateKinds()
    {
        this.ReadHeader("Type", "Tag");
        this.ReadField("N", "Market adjustment");
        this.reader.ReadEndOfRecord();
        this.ReadField("N", "Reimbursable");
        this.reader.ReadEndOfRecord();
        this.ReadHeader("Type", "Bank");
        this.ReadField("D", "1/1/2008");
        this.ReadField("T", "1500");
        this.ReadField("N", "123");
        this.ReadField("P", "Paycheck");
        this.ReadField("L", "Income.Salary");
        this.reader.ReadEndOfRecord();
        this.ReadHeader("Option", "AutoSwitch");
        this.ReadHeader("Account");
        this.ReadField("N", "360 Checking");
        this.ReadField("T", "Bank");
        this.reader.ReadEndOfRecord();
        this.ReadField("N", "Amazon Payments");
        this.ReadField("T", "Bank");
        this.reader.ReadEndOfRecord();
        this.ReadHeader("Clear", "AutoSwitch");
        this.ReadHeader("Account");
        this.ReadField("N", "Nuther Account");
        this.ReadField("T", "Oth A");
        this.reader.ReadEndOfRecord();
        this.ReadHeader("Type", "Prices");
        this.ReadField(string.Empty, "ACOM");
        this.ReadField(string.Empty, "33 3/4");
        this.ReadField(string.Empty, " 3/ 4'11");
        this.reader.ReadEndOfRecord();
        this.ReadHeader("Type", "Prices");
        this.ReadField(string.Empty, "XLM");
        this.ReadField(string.Empty, "1/4");
        this.ReadField(string.Empty, " 1/21'21");
        this.reader.ReadEndOfRecord();
        Assert.Equal(QifToken.EOF, this.reader.Kind);
    }

    [Fact]
    public void TryRead_RealWorldLoop()
    {
        while (this.reader.Kind == QifToken.Header)
        {
            this.Logger.WriteLine($"{this.reader.Header.Name} {this.reader.Header.Value} records:");
            this.reader.MoveNext();
            while (this.reader.Kind is QifToken.Field or QifToken.CommaDelimitedValue)
            {
                while (this.reader.Kind is QifToken.Field or QifToken.CommaDelimitedValue)
                {
                    this.Logger.WriteLine($"\t{this.reader.Field.Name} = {this.reader.Field.Value}");
                    this.reader.MoveNext();
                }

                this.reader.ReadEndOfRecord();
                this.Logger.WriteLine(string.Empty);
            }
        }

        Assert.Equal(QifToken.EOF, this.reader.Kind);
    }

    [Fact]
    public void MoveToNext()
    {
        for (int i = 0; i < 7; i++)
        {
            Assert.Equal(QifToken.Header, this.reader.Kind);
            Assert.True(this.reader.MoveToNext(QifToken.Header));
        }

        Assert.False(this.reader.MoveToNext(QifToken.Header));
        Assert.Equal(QifToken.EOF, this.reader.Kind);
    }

    [Fact]
    public void ReadFieldAsString()
    {
        Assumes.True(this.reader.Kind == QifToken.Header);
        Assert.Throws<InvalidOperationException>(() => this.reader.ReadFieldAsString());
        this.reader.MovePast(QifToken.Header);
        Assert.Equal("Market adjustment", this.reader.ReadFieldAsString());
    }

    [Fact]
    public void ReadFieldAsDate()
    {
        Assert.Throws<InvalidOperationException>(() => this.reader.ReadFieldAsDate());
        Assert.True(this.TrySkipToType("Bank"));
        Assert.True(this.TrySkipToField("D"));
        Assert.Equal(new DateTime(2008, 1, 1), this.reader.ReadFieldAsDate());
    }

    [Fact]
    public void ReadFieldAsInteger()
    {
        Assert.Throws<InvalidOperationException>(() => this.reader.ReadFieldAsInt64());
        Assert.True(this.TrySkipToType("Bank"));
        Assert.True(this.TrySkipToField("N"));
        Assert.Equal(123, this.reader.ReadFieldAsInt64());
    }

    [Fact]
    public void ReadFieldAsDecimal()
    {
        Assert.Throws<InvalidOperationException>(() => this.reader.ReadFieldAsDecimal());
        Assert.True(this.TrySkipToType("Bank"));
        Assert.True(this.TrySkipToField("T"));
        Assert.Equal(1500m, this.reader.ReadFieldAsDecimal());
    }

    [Fact]
    public void ReadFieldAsDecimal_MixedNumber()
    {
        this.TrySkipToType("Prices");
        this.reader.MoveNext();
        this.reader.MoveNext();
        Assert.Equal(33.75m, this.reader.ReadFieldAsDecimal());
    }

    [Fact]
    public void ReadFieldAsDecimal_Fraction()
    {
        this.TrySkipToType("Prices");
        this.TrySkipToType("Prices");
        this.reader.MoveNext();
        this.reader.MoveNext();
        Assert.Equal(0.25m, this.reader.ReadFieldAsDecimal());
    }

    [Fact]
    public void FormatProviderDefaultsToCurrentCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("en-MX");
        try
        {
            Assert.Same(CultureInfo.CurrentCulture, new QifReader(new StringReader(string.Empty)).FormatProvider);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    private bool TrySkipToType(string type)
    {
        while (this.reader.MoveToNext(QifToken.Header))
        {
            if (QifUtilities.Equals("Type", this.reader.Header.Name) && QifUtilities.Equals(type, this.reader.Header.Value))
            {
                return true;
            }
        }

        return false;
    }

    private bool TrySkipToField(string fieldName)
    {
        while (this.reader.MoveToNext(QifToken.Field))
        {
            if (QifUtilities.Equals(fieldName, this.reader.Field.Name))
            {
                return true;
            }
        }

        return false;
    }

    private void ReadHeader(string expectedName, string expectedValue = "")
    {
        AssertEqual(expectedName, this.reader.Header.Name);
        AssertEqual(expectedValue, this.reader.Header.Value);
        this.reader.MoveNext();
    }

    private void ReadField(string expectedHeader, string expectedValue)
    {
        AssertEqual(expectedHeader, this.reader.Field.Name);
        AssertEqual(expectedValue, this.reader.Field.Value);
        this.reader.MoveNext();
    }
}
