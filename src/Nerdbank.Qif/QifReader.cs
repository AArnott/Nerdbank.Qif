// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Nerdbank.Qif;

/// <summary>
/// A QIF file reader that is slightly higher level than <see cref="QifParser"/>.
/// </summary>
public class QifReader : IDisposable
{
    private readonly QifParser parser;

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
    /// Gets the value of the header at the current reader position.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Kind"/> is not <see cref="QifParser.TokenKind.Header"/>.</exception>
    public (ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) Header
    {
        get
        {
            this.ThrowIfNotAt(QifParser.TokenKind.Header);
            return this.parser.CurrentHeader;
        }
    }

    /// <summary>
    /// Gets the value of the field at the current reader position.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Kind"/> is not <see cref="QifParser.TokenKind.Field"/>.</exception>
    public (ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) Field
    {
        get
        {
            this.ThrowIfNotAt(QifParser.TokenKind.Field);
            return this.parser.Field;
        }
    }

    /// <summary>
    /// Parses the field value as a <see cref="string"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Kind"/> is not <see cref="QifParser.TokenKind.Field"/>.</exception>
    public string ReadFieldAsString() => this.Field.Value.ToString();

    /// <summary>
    /// Parses the field value as a <see cref="DateTime"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Kind"/> is not <see cref="QifParser.TokenKind.Field"/>.</exception>
    public virtual DateTime ReadFieldAsDate()
    {
        if (this.Field.Value.Span.Length > 50)
        {
            throw new InvalidTransactionException("Date value too long.");
        }

        Span<char> mutableDate = stackalloc char[this.Field.Value.Span.Length];
        this.Field.Value.Span.CopyTo(mutableDate);

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
    /// Parses the field value as a <see cref="long"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Kind"/> is not <see cref="QifParser.TokenKind.Field"/>.</exception>
    public virtual long ReadFieldAsInt64() => long.Parse(this.Field.Value.Span, NumberStyles.Any, this.FormatProvider);

    /// <summary>
    /// Parses the field value as a <see cref="decimal"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Kind"/> is not <see cref="QifParser.TokenKind.Field"/>.</exception>
    public virtual decimal ReadFieldAsDecimal() => decimal.Parse(this.Field.Value.Span, NumberStyles.Any, this.FormatProvider);

    /// <summary>
    /// Parses the field value as a <see cref="ClearedState"/>.
    /// </summary>
    /// <returns>The parsed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Kind"/> is not <see cref="QifParser.TokenKind.Field"/>.</exception>
    public virtual ClearedState ReadFieldAsClearedState()
    {
        return this.Field.Value.Length == 0 ? ClearedState.None : char.ToUpperInvariant(this.Field.Value.Span[0]) switch
        {
            '*' or 'C' => ClearedState.Cleared,
            'R' or 'X' => ClearedState.Reconciled,
            _ => throw new InvalidTransactionException("Unrecognized reconciled status."),
        };
    }

    /// <summary>
    /// Moves the reader to the next line.
    /// </summary>
    public void MoveNext() => this.parser.Read();

    /// <summary>
    /// Advances the reader to the next token of a given kind.
    /// </summary>
    /// <param name="kind">The kind of token to skip to.</param>
    /// <returns><see langword="true" /> if a token of the required kind was found; <see langword="false" /> if the end of file was reached first.</returns>
    public bool MoveToNext(QifParser.TokenKind kind)
    {
        do
        {
            this.MoveNext();
        }
        while (this.parser.Kind != kind && this.parser.Kind != QifParser.TokenKind.EOF);
        return this.parser.Kind == kind;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.parser.Dispose();
    }

    /// <summary>
    /// Reads beyond the current <see cref="QifParser.TokenKind.EndOfRecord"/> token.
    /// </summary>
    public void ReadEndOfRecord() => this.MovePast(QifParser.TokenKind.EndOfRecord);

    /// <summary>
    /// Reads beyond the current token.
    /// </summary>
    /// <param name="kind">The type of token the caller believes is the current one.</param>
    /// <exception cref="InvalidOperationException">Thrown if the current token is not of type <paramref name="kind"/>.</exception>
    public void MovePast(QifParser.TokenKind kind)
    {
        this.ThrowIfNotAt(kind);
        this.MoveNext();
    }

    /// <summary>
    /// Loops over the fields in the current set, skipping the current token if it is a <see cref="QifParser.TokenKind.Header"/>.
    /// </summary>
    /// <returns>A sequence of fields.</returns>
    public IEnumerable<(ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value)> ReadTheseFields()
    {
        if (this.Kind == QifParser.TokenKind.Header)
        {
            this.MoveNext();
        }

        while (this.Kind == QifParser.TokenKind.Field)
        {
            yield return this.Field;
            this.MoveNext();
        }

        this.ReadEndOfRecord();
    }

    private void ThrowIfNotAt(QifParser.TokenKind kind) => Verify.Operation(this.parser.Kind == kind, "Reader is not at a {0}.", kind);
}
