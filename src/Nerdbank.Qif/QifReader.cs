// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Nerdbank.Qif;

/// <summary>
/// A QIF file reader that is slightly higher level than <see cref="QifParser"/>.
/// </summary>
public class QifReader : IDisposable
{
    private readonly QifParser parser;
    private bool lastReadConsumed = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="QifReader"/> class.
    /// </summary>
    /// <param name="parser">The parser to read. This will be disposed of when the reader is disposed.</param>
    public QifReader(QifParser parser)
    {
        Requires.NotNull(parser, nameof(parser));
        this.parser = parser;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QifReader"/> class.
    /// </summary>
    /// <param name="reader">A <see cref="TextReader"/> to use. This will be disposed of when this <see cref="QifReader"/> is disposed.</param>
    public QifReader(TextReader reader)
        : this(new QifParser(reader))
    {
    }

    /// <summary>
    /// Gets or sets the format provider that is used to parse non-string values.
    /// </summary>
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.CurrentCulture;

    /// <inheritdoc cref="QifParser.Kind"/>
    public QifParser.TokenKind Kind => this.parser.Kind;

    /// <summary>
    /// Gets the value of the field at the current reader position.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the last read operation was not a successful call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>.</exception>
    public ReadOnlyMemory<char> Field
    {
        get
        {
            this.ThrowIfNotAtField();
            return this.parser.Field.Value;
        }
    }

    /// <summary>
    /// Reads the next token, if it is a <c>!</c> header (e.g. <c>!Type:</c> or <c>!Account</c>).
    /// </summary>
    /// <param name="name">Receives the header name (usually <c>Type</c> or <c>Account</c>).</param>
    /// <param name="value">Receives the header value. May be empty (e.g. for <c>Account</c> headers).</param>
    /// <returns><see langword="true" /> if the next token is a <see cref="QifParser.TokenKind.Header"/>; otherwise <see langword="false" />.</returns>
    public bool TryReadHeader(out ReadOnlyMemory<char> name, out ReadOnlyMemory<char> value)
    {
        if (this.TryGetNextTokenKind(QifParser.TokenKind.Header) &&
            this.parser.CurrentHeader is (ReadOnlyMemory<char> headerName, ReadOnlyMemory<char> headerValue))
        {
            name = headerName;
            value = headerValue;
            return true;
        }

        name = default;
        value = default;
        return false;
    }

    /// <summary>
    /// Reads the next token, if it is a record detail line.
    /// </summary>
    /// <param name="name">Receives the field name (usually one character, or two if the first is an <c>X</c>).</param>
    /// <param name="value">Receives the field value.</param>
    /// <returns><see langword="true" /> if the next token is a <see cref="QifParser.TokenKind.Field"/>; otherwise <see langword="false" />.</returns>
    public bool TryReadField(out ReadOnlyMemory<char> name, out ReadOnlyMemory<char> value)
    {
        if (this.TryGetNextTokenKind(QifParser.TokenKind.Field) &&
            this.parser.Field is (ReadOnlyMemory<char> fieldName, ReadOnlyMemory<char> fieldValue))
        {
            name = fieldName;
            value = fieldValue;
            return true;
        }

        name = default;
        value = default;
        return false;
    }

    /// <summary>
    /// Moves past the current end of record token, if that is the current reading position.
    /// </summary>
    /// <returns><see langword="true" /> if the next token is a <see cref="QifParser.TokenKind.EndOfRecord"/>; otherwise <see langword="false" />.</returns>
    public bool TryReadEndOfRecord()
    {
        return this.TryGetNextTokenKind(QifParser.TokenKind.EndOfRecord);
    }

    /// <summary>
    /// Returns a value indicating whether the reader has reached the end of the file.
    /// </summary>
    /// <returns><see langword="true" /> if the next token is a <see cref="QifParser.TokenKind.EOF"/>; otherwise <see langword="false" />.</returns>
    public bool TryReadEndOfFile() => this.TryGetNextTokenKind(QifParser.TokenKind.EOF);

    /// <summary>
    /// Advances the reader to the next token of a given kind.
    /// </summary>
    /// <param name="kind">The kind of token to skip to.</param>
    /// <returns><see langword="true" /> if a token of the required kind was found; <see langword="false" /> if the end of file was reached first.</returns>
    public bool TrySkipToToken(QifParser.TokenKind kind)
    {
        do
        {
            if (this.parser.Read() == QifParser.TokenKind.EOF)
            {
                return false;
            }
        }
        while (this.parser.Kind != kind);

        this.lastReadConsumed = false;
        return true;
    }

    /// <summary>
    /// Parses the field value read during the last call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>
    /// as a <see cref="string"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the last read operation was not a successful call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>.</exception>
    public string ReadFieldAsString() => this.Field.ToString();

    /// <summary>
    /// Parses the field value read during the last call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>
    /// as a <see cref="DateTime"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the last read operation was not a successful call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>.</exception>
    public virtual DateTime ReadFieldAsDate()
    {
        if (this.Field.Span.Length > 50)
        {
            throw new InvalidTransactionException("Date value too long.");
        }

        Span<char> mutableDate = stackalloc char[this.Field.Span.Length];
        this.Field.Span.CopyTo(mutableDate);

        // Replace ' with /, and space with 0.
        for (int i = 0; i < mutableDate.Length; i++)
        {
            if (mutableDate[i] == '\'')
            {
                mutableDate[i] = '/';
            }
            else if (mutableDate[i] == ' ')
            {
                mutableDate[i] = '0';
            }
        }

        return DateTime.Parse(mutableDate, this.FormatProvider);
    }

    /// <summary>
    /// Parses the field value read during the last call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>
    /// as a <see cref="long"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the last read operation was not a successful call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>.</exception>
    public virtual long ReadFieldAsInt64() => long.Parse(this.Field.Span, NumberStyles.Any, this.FormatProvider);

    /// <summary>
    /// Parses the field value read during the last call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>
    /// as a <see cref="decimal"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the last read operation was not a successful call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>.</exception>
    public virtual decimal ReadFieldAsDecimal() => decimal.Parse(this.Field.Span, NumberStyles.Any, this.FormatProvider);

    /// <summary>
    /// Parses the field value read during the last call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>
    /// as a <see cref="ClearedState"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the last read operation was not a successful call to <see cref="TryReadField(out ReadOnlyMemory{char}, out ReadOnlyMemory{char})"/>.</exception>
    public virtual ClearedState ReadFieldAsClearedState()
    {
        return this.Field.Length == 0 ? ClearedState.None : char.ToUpperInvariant(this.Field.Span[0]) switch
        {
            '*' or 'C' => ClearedState.Cleared,
            'R' or 'X' => ClearedState.Reconciled,
            _ => throw new InvalidTransactionException("Unrecognized reconciled status."),
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.parser.Dispose();
    }

    /// <summary>
    /// Reads the <see cref="QifParser.TokenKind.EndOfRecord"/> token at the current position.
    /// </summary>
    /// <exception cref="InvalidTransactionException">Thrown if the current token is not the expected token.</exception>
    internal void ReadEndOfRecord()
    {
        if (!this.TryReadEndOfRecord())
        {
            throw new InvalidTransactionException("Missing expected end of record token.");
        }
    }

    private bool TryGetNextTokenKind(QifParser.TokenKind expectedKind)
    {
        if (this.lastReadConsumed)
        {
            this.parser.Read();
        }

        this.lastReadConsumed = this.parser.Kind == expectedKind;
        return this.lastReadConsumed;
    }

    private void ThrowIfNotAtField() => Verify.Operation(this.parser.Kind == QifParser.TokenKind.Field, "Reader has not just read a field.");
}
