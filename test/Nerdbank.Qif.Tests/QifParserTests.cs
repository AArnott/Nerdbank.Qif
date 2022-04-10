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
        Assert.Equal("Tag", parser.CurrentType.ToString());

        ReadEndOfRecord();
        Assert.Equal("Tag", parser.CurrentType.ToString());

        ReadField("N", "Reimbursable");
        ReadEndOfRecord();
        ReadType("Bank");
        ReadField("D", "1/1/2008");
        ReadField("T", "1500");
        ReadField("N", "123");
        ReadField("P", "Paycheck");
        ReadField("L", "Income.Salary");
        ReadEndOfRecord();
        ReadKind(QifParser.TokenKind.EOF);

        void ReadKind(QifParser.TokenKind expectedKind)
        {
            Assert.Equal(expectedKind, parser.Read());
            Assert.Equal(expectedKind, parser.Kind);
        }

        void ReadType(string expectedType)
        {
            ReadKind(QifParser.TokenKind.Type);
            Assert.Equal(expectedType, parser.CurrentType!.Value.ToString());
            Assert.Null(parser.Field);
        }

        void ReadField(string expectedHeader, string expectedValue)
        {
            ReadKind(QifParser.TokenKind.Field);
            Assert.Equal(expectedHeader, parser.Field!.Value.Header.ToString());
            Assert.Equal(expectedValue, parser.Field!.Value.Value.ToString());
        }

        void ReadEndOfRecord()
        {
            ReadKind(QifParser.TokenKind.EndOfRecord);
            Assert.Null(parser.Field);
        }
    }
}
