// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class QifReaderTests : TestBase
{
    public QifReaderTests(ITestOutputHelper logger)
        : base(logger)
    {
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
        using QifReader reader = new(new StreamReader(GetSampleDataStream(ReaderInputsDataFile)));

        ReadType("Tag");
        ReadField("N", "Market adjustment");
        Assert.True(reader.TryReadEndOfRecord());
        ReadField("N", "Reimbursable");
        Assert.True(reader.TryReadEndOfRecord());
        ReadType("Bank");
        ReadField("D", "1/1/2008");
        ReadField("T", "1500");
        ReadField("N", "123");
        ReadField("P", "Paycheck");
        ReadField("L", "Income.Salary");
        Assert.True(reader.TryReadEndOfRecord());
        Assert.True(reader.TryReadEndOfFile());
        Assert.True(reader.TryReadEndOfFile());

        void ReadType(string expectedType)
        {
            Assert.True(reader.TryReadType(out ReadOnlyMemory<char> type));
            AssertEqual(expectedType, type);
        }

        void ReadField(string expectedHeader, string expectedValue)
        {
            Assert.True(reader.TryReadField(out ReadOnlyMemory<char> name, out ReadOnlyMemory<char> value));
            Assert.Equal(expectedHeader, name.ToString());
            Assert.Equal(expectedValue, value.ToString());
        }
    }

    [Fact]
    public void TryRead_GuessWrong()
    {
        using QifReader reader = new(new StreamReader(GetSampleDataStream(ReaderInputsDataFile)));

        ReadType("Tag");
        Assert.False(reader.TryReadType(out _));
        ReadField("N", "Market adjustment");
        Assert.True(reader.TryReadEndOfRecord());
        ReadField("N", "Reimbursable");
        Assert.True(reader.TryReadEndOfRecord());
        ReadType("Bank");
        ReadField("D", "1/1/2008");
        ReadField("T", "1500");
        ReadField("N", "123");
        ReadField("P", "Paycheck");
        ReadField("L", "Income.Salary");
        Assert.True(reader.TryReadEndOfRecord());
        Assert.True(reader.TryReadEndOfFile());
        Assert.True(reader.TryReadEndOfFile());

        void ReadType(string expectedType)
        {
            Assert.False(reader.TryReadField(out _, out _));
            Assert.False(reader.TryReadEndOfRecord());
            Assert.False(reader.TryReadEndOfFile());
            Assert.True(reader.TryReadType(out ReadOnlyMemory<char> type));
            AssertEqual(expectedType, type);
        }

        void ReadField(string expectedHeader, string expectedValue)
        {
            Assert.False(reader.TryReadType(out _));
            Assert.False(reader.TryReadEndOfRecord());
            Assert.False(reader.TryReadEndOfFile());
            Assert.True(reader.TryReadField(out ReadOnlyMemory<char> name, out ReadOnlyMemory<char> value));
            Assert.Equal(expectedHeader, name.ToString());
            Assert.Equal(expectedValue, value.ToString());
        }
    }

    [Fact]
    public void TryRead_RealWorldLoop()
    {
        using QifReader reader = new(new StreamReader(GetSampleDataStream(ReaderInputsDataFile)));

        while (reader.TryReadType(out ReadOnlyMemory<char> type))
        {
            this.Logger.WriteLine($"{type} records:");
            while (true)
            {
                while (reader.TryReadField(out ReadOnlyMemory<char> name, out ReadOnlyMemory<char> value))
                {
                    this.Logger.WriteLine($"\t{name} = {value}");
                }

                if (reader.TryReadEndOfRecord())
                {
                    this.Logger.WriteLine(string.Empty);
                }
                else
                {
                    break;
                }
            }
        }

        Assert.True(reader.TryReadEndOfFile());
    }

    [Fact]
    public void TrySkipToToken()
    {
        using QifReader reader = new(new StreamReader(GetSampleDataStream(ReaderInputsDataFile)));

        Assert.True(reader.TrySkipToToken(QifParser.TokenKind.Type));
        Assert.True(reader.TryReadType(out _));

        Assert.True(reader.TrySkipToToken(QifParser.TokenKind.Type));
        Assert.True(reader.TryReadType(out _));

        Assert.False(reader.TrySkipToToken(QifParser.TokenKind.Type));
        Assert.True(reader.TryReadEndOfFile());
    }

    [Fact]
    public void ReadFieldAsString()
    {
        using QifReader reader = new(new StreamReader(GetSampleDataStream(ReaderInputsDataFile)));

        Assert.Throws<InvalidOperationException>(() => reader.ReadFieldAsString());
        reader.TryReadType(out _);
        Assert.Throws<InvalidOperationException>(() => reader.ReadFieldAsString());
        reader.TryReadField(out _, out _);
        Assert.Equal("Market adjustment", reader.ReadFieldAsString());
    }

    [Fact]
    public void ReadFieldAsDate()
    {
        using QifReader reader = new(new StreamReader(GetSampleDataStream(ReaderInputsDataFile)));

        Assert.Throws<InvalidOperationException>(() => reader.ReadFieldAsDate());
        Assert.True(TrySkipToType(reader, "Bank"));
        Assert.True(TrySkipToField(reader, "D"));
        Assert.Equal(new DateTime(2008, 1, 1), reader.ReadFieldAsDate());
    }

    [Fact]
    public void ReadFieldAsInteger()
    {
        using QifReader reader = new(new StreamReader(GetSampleDataStream(ReaderInputsDataFile)));

        Assert.Throws<InvalidOperationException>(() => reader.ReadFieldAsInt64());
        Assert.True(TrySkipToType(reader, "Bank"));
        Assert.True(TrySkipToField(reader, "N"));
        Assert.Equal(123, reader.ReadFieldAsInt64());
    }

    [Fact]
    public void ReadFieldAsDecimal()
    {
        using QifReader reader = new(new StreamReader(GetSampleDataStream(ReaderInputsDataFile)));

        Assert.Throws<InvalidOperationException>(() => reader.ReadFieldAsDecimal());
        Assert.True(TrySkipToType(reader, "Bank"));
        Assert.True(TrySkipToField(reader, "T"));
        Assert.Equal(1500m, reader.ReadFieldAsDecimal());
    }

    private static bool TrySkipToType(QifReader reader, string type)
    {
        while (reader.TrySkipToToken(QifParser.TokenKind.Type))
        {
            Assert.True(reader.TryReadType(out ReadOnlyMemory<char> actualType));
            if (QifUtilities.Equals(type, actualType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TrySkipToField(QifReader reader, string fieldName)
    {
        while (reader.TrySkipToToken(QifParser.TokenKind.Field))
        {
            Assert.True(reader.TryReadField(out ReadOnlyMemory<char> actualName, out _));
            if (QifUtilities.Equals(fieldName, actualName))
            {
                return true;
            }
        }

        return false;
    }
}
