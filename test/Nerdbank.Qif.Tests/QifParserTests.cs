// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class QifParserTests : TestBase
{
    public QifParserTests(ITestOutputHelper logger)
        : base(logger)
    {
    }

    [Fact]
    public void DisposeDisposesReader()
    {
        StringReader sr = new(string.Empty);
        QifParser parser = new(sr);
        parser.Dispose();
        Assert.Throws<ObjectDisposedException>(() => sr.Read());
    }

    [Fact]
    public void Ctor_ValidatesArgs()
    {
        Assert.Throws<ArgumentNullException>(() => new QifParser(null!));
    }

    [Fact]
    public void Read()
    {
        using QifParser parser = new(new StreamReader(GetSampleDataStream(ReaderInputsDataFile)));

        ReadType("Tag");

        ReadField("N", "Market adjustment");
        AssertEqual("Tag", parser.CurrentHeader.Value);

        ReadEndOfRecord();
        AssertEqual("Tag", parser.CurrentHeader.Value);

        ReadField("N", "Reimbursable");
        ReadEndOfRecord();
        ReadType("Bank");
        ReadField("D", "1/1/2008");
        ReadField("T", "1500");
        ReadField("N", "123");
        ReadField("P", "Paycheck");
        ReadField("L", "Income.Salary");
        ReadEndOfRecord();
        ReadHeader("Option", "AutoSwitch");
        ReadHeader("Account");
        ReadField("N", "360 Checking");
        ReadField("T", "Bank");
        ReadEndOfRecord();
        ReadField("N", "Amazon Payments");
        ReadField("T", "Bank");
        ReadEndOfRecord();
        ReadHeader("Clear", "AutoSwitch");
        ReadHeader("Account");
        ReadField("N", "Nuther Account");
        ReadField("T", "Oth A");
        ReadEndOfRecord();
        ReadHeader("Type", "Prices");
        ReadCommaDelimitedValue("ACOM");
        ReadCommaDelimitedValue("33 3/4");
        ReadCommaDelimitedValue(" 3/ 4'11");
        ReadEndOfRecord();
        ReadHeader("Type", "Prices");
        ReadCommaDelimitedValue("XLM");
        ReadCommaDelimitedValue("1/4");
        ReadCommaDelimitedValue(" 1/21'21");
        ReadEndOfRecord();
        ReadKind(QifToken.EOF);

        void ReadKind(QifToken expectedKind)
        {
            Assert.Equal(expectedKind, parser.Read());
            Assert.Equal(expectedKind, parser.Kind);
        }

        void ReadHeader(string expectedName, string expectedValue = "")
        {
            ReadKind(QifToken.Header);
            AssertEqual(expectedName, parser.CurrentHeader.Name);
            AssertEqual(expectedValue, parser.CurrentHeader.Value);
            Assert.Equal(0, parser.Field.Name.Length);
        }

        void ReadType(string expectedType) => ReadHeader("Type", expectedType);

        void ReadField(string expectedHeader, string expectedValue)
        {
            ReadKind(QifToken.Field);
            AssertEqual(expectedHeader, parser.Field.Name);
            AssertEqual(expectedValue, parser.Field.Value);
        }

        void ReadCommaDelimitedValue(string expectedValue)
        {
            ReadKind(QifToken.CommaDelimitedValue);
            AssertEqual(expectedValue, parser.Field.Value);
        }

        void ReadEndOfRecord()
        {
            ReadKind(QifToken.EndOfRecord);
            Assert.Equal(0, parser.Field.Name.Length);
        }
    }

    [Fact]
    public void ReadTrimsHeaderValues()
    {
        using QifParser parser = new(new StringReader("!Type:Bank "));
        Assert.Equal(QifToken.Header, parser.Read());
        AssertEqual("Bank", parser.CurrentHeader.Value);
    }

    [Fact]
    public void ReadTrimsFieldValues()
    {
        using QifParser parser = new(new StringReader("!Type:Bank\nD1/1/2021 \nT12.00 \nXXAb \n"));
        Assert.Equal(QifToken.Header, parser.Read());
        Assert.Equal(QifToken.Field, parser.Read());
        AssertEqual("1/1/2021", parser.Field.Value);
        Assert.Equal(QifToken.Field, parser.Read());
        AssertEqual("12.00", parser.Field.Value);
        Assert.Equal(QifToken.Field, parser.Read());
        AssertEqual("Ab", parser.Field.Value);
    }

    [Fact]
    public void ReadCommaDelimitedValues()
    {
        using QifParser parser = new(new StringReader("!Type:Prices\n\"BEXFX\",11.91,\" 3/ 3'15\"\n^\n"));
        Assert.Equal(QifToken.Header, parser.Read());
        Assert.Equal(QifToken.CommaDelimitedValue, parser.Read());
        AssertEqual("BEXFX", parser.Field.Value);
        Assert.Equal(QifToken.CommaDelimitedValue, parser.Read());
        AssertEqual("11.91", parser.Field.Value);
        Assert.Equal(QifToken.CommaDelimitedValue, parser.Read());
        AssertEqual(" 3/ 3'15", parser.Field.Value);
        Assert.Equal(QifToken.EndOfRecord, parser.Read());
    }
}
