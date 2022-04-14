// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class QifWriterTests : TestBase
{
    private QifWriter writer;
    private StringWriter stringWriter;

    public QifWriterTests(ITestOutputHelper logger)
        : base(logger)
    {
        this.stringWriter = new() { NewLine = "\n" };
        this.writer = new(this.stringWriter);
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
    public void WriteField_WithValue_String()
    {
        this.writer.WriteField("N", "Checking");
        this.AssertWritten("NChecking\n");
    }

    [Fact]
    public void WriteField_WithValue_ReadOnlySpanOfChar()
    {
        this.writer.WriteField("N", "Checking".AsSpan());
        this.AssertWritten("NChecking\n");
    }

    private void AssertWritten(string qif) => Assert.Equal(qif, this.stringWriter.ToString());
}
