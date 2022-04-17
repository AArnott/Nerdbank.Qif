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
    /// <param name="writer">The text writer to emit to. This value will <em>not</em> be disposed of when writing is done.</param>
    public QifWriter(TextWriter writer)
    {
        this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <summary>
    /// Gets or sets the format provider that will be used to serialize certain data types.
    /// </summary>
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// Gets the last header written.
    /// </summary>
    public (ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) LastWrittenHeader { get; private set; }

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
        this.LastWrittenHeader = (name.AsMemory(), value.AsMemory());
    }

    /// <summary>
    /// Emits a field such as <c>N</c>.
    /// </summary>
    /// <param name="name">The name of the field (e.g. "N").</param>
    public void WriteField(string name)
    {
        this.writer.WriteLine(name);
    }

    /// <summary>
    /// Emits a field such as <c>NSome Name</c>.
    /// </summary>
    /// <param name="name">The name of the field (e.g. "N").</param>
    /// <param name="value">The value of the field (e.g. "Checking").</param>
    /// <remarks>Nothing is written when <paramref name="value"/> is <see langword="null"/>.</remarks>
    public void WriteField(string name, string? value)
    {
        if (value is null)
        {
            return;
        }

        this.writer.Write(name);
        this.writer.WriteLine(value);
    }

    /// <summary>
    /// Writes the <see cref="QifToken.EndOfRecord"/> token.
    /// </summary>
    public void WriteEndOfRecord()
    {
        this.writer.WriteLine("^");
    }

    /// <summary>
    /// Emits a field such as <c>T</c> if some expression is <see langword="true"/>.
    /// </summary>
    /// <param name="name">The name of the field (e.g. "N").</param>
    /// <param name="condition">The condition that determines whether this method does anything.</param>
    public void WriteFieldIf(string name, bool condition)
    {
        if (condition)
        {
            this.WriteField(name);
        }
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
    public virtual void WriteField(string name, DateTime? value)
    {
        if (value is null)
        {
            return;
        }

        Span<char> buffer = stackalloc char[20];
        Assumes.True(value.Value.TryFormat(buffer, out int charsWritten, "d", this.FormatProvider));
        this.WriteField(name, buffer.Slice(0, charsWritten));
    }

    /// <inheritdoc cref="WriteField(string, string?)"/>
    public virtual void WriteField(string name, long? value)
    {
        if (value is null)
        {
            return;
        }

        Span<char> buffer = stackalloc char[100];
        Assumes.True(value.Value.TryFormat(buffer, out int charsWritten, provider: this.FormatProvider));
        this.WriteField(name, buffer.Slice(0, charsWritten));
    }

    /// <inheritdoc cref="WriteField(string, string?)"/>
    public virtual void WriteField(string name, char value)
    {
        Span<char> buffer = stackalloc char[1];
        buffer[0] = value;
        this.WriteField(name, buffer);
    }

    /// <inheritdoc cref="WriteField(string, string?)"/>
    public virtual void WriteField(string name, decimal? value)
    {
        if (value is null)
        {
            return;
        }

        Span<char> buffer = stackalloc char[100];
        Assumes.True(value.Value.TryFormat(buffer, out int charsWritten, provider: this.FormatProvider));
        this.WriteField(name, buffer.Slice(0, charsWritten));
    }

    /// <inheritdoc cref="WriteField(string, string?)"/>
    public virtual void WriteField(string name, ClearedState value)
    {
        switch (value)
        {
            case ClearedState.None:
                // There is no encoding for this value. The lack of an encoding implies this value.
                break;
            case ClearedState.Cleared:
                this.WriteField(name, "C");
                break;
            case ClearedState.Reconciled:
                this.WriteField(name, "R");
                break;
            default:
                throw new ArgumentException();
        }
    }
}
