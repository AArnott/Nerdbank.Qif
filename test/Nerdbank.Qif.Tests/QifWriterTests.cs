// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

public class QifWriterTests : TestBase
{
    private QifWriter writer;
    private StringWriter stringWriter;

    public QifWriterTests(ITestOutputHelper logger)
        : base(logger)
    {
        this.stringWriter = new() { NewLine = "\n" };
        this.writer = new(this.stringWriter) { FormatProvider = CultureInfo.InvariantCulture };
    }

    [Fact]
    public void Ctor_ValidatesArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new QifWriter(null!));
    }

    [Fact]
    public void WriteHeader_NoValue()
    {
        this.writer.WriteHeader("Account");
        this.AssertWritten("!Account\n");
    }

    [Fact]
    public void WriteHeader_WithValue()
    {
        this.writer.WriteHeader("Type", "Bank");
        this.AssertWritten("!Type:Bank\n");
    }

    [Fact]
    public void WriteField_NoValue()
    {
        this.writer.WriteField("I");
        this.AssertWritten("I\n");
    }

    [Fact]
    public void WriteField_WithString()
    {
        this.writer.WriteField("N", "Checking");
        this.AssertWritten("NChecking\n");
        this.writer.WriteField("N", (string?)null);
        this.AssertWritten(string.Empty);
    }

    [Fact]
    public void WriteField_WithReadOnlySpanOfChar()
    {
        this.writer.WriteField("N", "Checking".AsSpan());
        this.AssertWritten("NChecking\n");
        this.writer.WriteField("D", (DateTime?)null);
        this.AssertWritten(string.Empty);
    }

    [Fact]
    public void WriteField_WithDate()
    {
        this.writer.WriteField("D", new DateTime(2013, 2, 4));
        this.AssertWritten("D02/04/2013\n");
    }

    [Fact]
    public void WriteField_WithInt64()
    {
        this.writer.WriteField("D", 35L);
        this.AssertWritten("D35\n");
        this.writer.WriteField("D", (long?)null);
        this.AssertWritten(string.Empty);
    }

    [Fact]
    public void WriteField_WithDecimal()
    {
        this.writer.WriteField("D", 35.5m);
        this.AssertWritten("D35.5\n");
        this.writer.WriteField("D", (decimal?)null);
        this.AssertWritten(string.Empty);
    }

    [Fact]
    public void WriteField_WithClearedState()
    {
        this.writer.WriteField("D", ClearedState.Cleared);
        this.AssertWritten("DC\n");
        this.writer.WriteField("D", ClearedState.Reconciled);
        this.AssertWritten("DR\n");
        this.writer.WriteField("D", ClearedState.None);
        this.AssertWritten(string.Empty);
    }

    [Fact]
    public void WriteCommaDelimitedValue_WithString()
    {
        this.writer.WriteCommaDelimitedValue("Str1");
        this.AssertWritten("\"Str1\"", clear: false);
        this.writer.WriteCommaDelimitedValue("Str2");
        this.AssertWritten("\"Str1\",\"Str2\"", clear: false);
        this.writer.WriteEndOfRecord();
        this.AssertWritten("\"Str1\",\"Str2\"\n^\n", clear: false);
    }

    [Fact]
    public void WriteCommaDelimitedValue_WithDate()
    {
        this.writer.WriteCommaDelimitedValue(new DateTime(2011, 2, 3));
        this.AssertWritten("\"02/03/2011\"");
    }

    [Fact]
    public void WriteCommaDelimitedValue_WithDecimal()
    {
        this.writer.WriteCommaDelimitedValue(1.2m);
        this.AssertWritten("1.2");
    }

    [Fact]
    public void WriteCommaDelimitedValue_MultiValueRecord()
    {
        this.writer.WriteCommaDelimitedValue("Str1");
        this.AssertWritten("\"Str1\"", clear: false);
        this.writer.WriteCommaDelimitedValue(1.2m);
        this.AssertWritten("\"Str1\",1.2", clear: false);
        this.writer.WriteEndOfRecord();
        this.AssertWritten("\"Str1\",1.2\n^\n");
    }

    [Fact]
    public void WriteCommaDelimitedValue_TwoRecords()
    {
        for (int i = 0; i < 2; i++)
        {
            this.WriteCommaDelimitedValue_MultiValueRecord();
        }
    }

    private void AssertWritten(string qif, bool clear = true)
    {
        Assert.Equal(qif, this.stringWriter.ToString());
        if (clear)
        {
            this.stringWriter.GetStringBuilder().Clear();
        }
    }
}
