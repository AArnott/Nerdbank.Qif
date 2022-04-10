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
        ReadKind(QifParser.TokenKind.EOF);

        void ReadKind(QifParser.TokenKind expectedKind)
        {
            Assert.Equal(expectedKind, parser.Read());
            Assert.Equal(expectedKind, parser.Kind);
        }

        void ReadHeader(string expectedName, string expectedValue = "")
        {
            ReadKind(QifParser.TokenKind.Header);
            AssertEqual(expectedName, parser.CurrentHeader.Name);
            AssertEqual(expectedValue, parser.CurrentHeader.Value);
            Assert.Equal(0, parser.Field.Name.Length);
        }

        void ReadType(string expectedType) => ReadHeader("Type", expectedType);

        void ReadField(string expectedHeader, string expectedValue)
        {
            ReadKind(QifParser.TokenKind.Field);
            AssertEqual(expectedHeader, parser.Field.Name);
            AssertEqual(expectedValue, parser.Field.Value);
        }

        void ReadEndOfRecord()
        {
            ReadKind(QifParser.TokenKind.EndOfRecord);
            Assert.Equal(0, parser.Field.Name.Length);
        }
    }
}
