// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Nerdbank.Qif;

/// <summary>
/// A writer for QIF tokens.
/// </summary>
public class QifWriter
{
    private readonly TextWriter writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="QifWriter"/> class.
    /// </summary>
    /// <param name="writer">The text writer to emit to.</param>
    public QifWriter(TextWriter writer)
    {
        this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <summary>
    /// Gets or sets the format provider that will be used to serialize certain data types.
    /// </summary>
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// Emits a header such as <c>!Type:Bank</c>.
    /// </summary>
    /// <param name="name">The header name (e.g. "Type").</param>
    /// <param name="value">The header value (e.g. "Bank").</param>
    public void WriteHeader(string name, string? value = null)
    {
        this.writer.Write('!');
        this.writer.Write(name);
        if (!string.IsNullOrEmpty(value))
        {
            this.writer.Write(':');
            this.writer.Write(value);
        }

        this.writer.WriteLine();
    }

    /// <summary>
    /// Emits a field such as <c>NSome Name</c>.
    /// </summary>
    /// <param name="name">The name of the field (e.g. "N").</param>
    /// <param name="value">The value of the field (e.g. "Checking").</param>
    public void WriteField(string name, string? value = null)
    {
        this.writer.Write(name);
        this.writer.WriteLine(value);
    }

    /// <summary>
    /// Writes the <see cref="QifParser.TokenKind.EndOfRecord"/> token.
    /// </summary>
    public void WriteEndOfRecord()
    {
        this.writer.WriteLine("^");
    }

    /// <summary>
    /// Emits a field such as <c>NSome Name</c> if the value is not <see langword="null" /> or empty.
    /// </summary>
    /// <param name="name">The name of the field (e.g. "N").</param>
    /// <param name="value">The value of the field (e.g. "Checking").</param>
    public void WriteFieldIfNotEmpty(string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            this.WriteField(name, value);
        }
    }

    /// <summary>
    /// Emits a field such as <c>NSome Name</c>.
    /// </summary>
    /// <param name="name">The name of the field (e.g. "N").</param>
    /// <param name="value">The value of the field (e.g. "Checking").</param>
    public void WriteField(string name, ReadOnlySpan<char> value)
    {
        this.writer.Write(name);
        this.writer.WriteLine(value);
    }

    /// <inheritdoc cref="WriteField(string, string?)"/>
    public void WriteField(string name, DateTime value)
    {
        Span<char> buffer = stackalloc char[20];
        Assumes.True(value.TryFormat(buffer, out int charsWritten, "d", this.FormatProvider));
        this.WriteField(name, buffer.Slice(0, charsWritten));
    }

    /// <inheritdoc cref="WriteField(string, string?)"/>
    public void WriteField(string name, long value)
    {
        Span<char> buffer = stackalloc char[100];
        Assumes.True(value.TryFormat(buffer, out int charsWritten, provider: this.FormatProvider));
        this.WriteField(name, buffer.Slice(0, charsWritten));
    }

    /// <inheritdoc cref="WriteField(string, string?)"/>
    public void WriteField(string name, decimal value)
    {
        Span<char> buffer = stackalloc char[100];
        Assumes.True(value.TryFormat(buffer, out int charsWritten, provider: this.FormatProvider));
        this.WriteField(name, buffer.Slice(0, charsWritten));
    }

    /// <inheritdoc cref="WriteField(string, string?)"/>
    public void WriteField(string name, ClearedState value)
    {
        string valueAsString = value switch
        {
            ClearedState.Cleared => "C",
            ClearedState.Reconciled => "R",
            _ => throw new ArgumentException("Only Cleared and Reconciled states should be written."),
        };
        this.WriteField(name, valueAsString);
    }
}
