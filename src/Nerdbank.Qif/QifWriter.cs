// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    /// Emits a field such as <c>NSome Name</c>.
    /// </summary>
    /// <param name="name">The name of the field (e.g. "N").</param>
    /// <param name="value">The value of the field (e.g. "Checking").</param>
    public void WriteField(string name, ReadOnlySpan<char> value)
    {
        this.writer.Write(name);
        this.writer.WriteLine(value);
    }
}
