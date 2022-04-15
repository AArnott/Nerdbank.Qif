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
    }

    [Fact]
    public void WriteField_WithReadOnlySpanOfChar()
    {
        this.writer.WriteField("N", "Checking".AsSpan());
        this.AssertWritten("NChecking\n");
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
    }

    [Fact]
    public void WriteField_WithDecimal()
    {
        this.writer.WriteField("D", 35.5m);
        this.AssertWritten("D35.5\n");
    }

    [Fact]
    public void WriteField_WithClearedState()
    {
        this.writer.WriteField("D", ClearedState.Cleared);
        this.AssertWritten("DC\n");
    }

    private void AssertWritten(string qif) => Assert.Equal(qif, this.stringWriter.ToString());
}
