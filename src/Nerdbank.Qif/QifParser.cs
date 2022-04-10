// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Nerdbank.Qif;

/// <summary>
/// A low-level parser of QIF files.
/// </summary>
/// <remarks>
/// Use the <see cref="QifReader"/> for a more convenient API.
/// </remarks>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
public class QifParser : IDisposable
{
    private const string TypeHeader = "!Type:";
    private readonly TextReader reader;

    /// <summary>
    /// Initializes a new instance of the <see cref="QifParser"/> class.
    /// </summary>
    /// <param name="reader">The text to read. This will be disposed of when the parser is disposed.</param>
    public QifParser(TextReader reader)
    {
        Requires.NotNull(reader, nameof(reader));
        this.reader = reader;
    }

    /// <summary>
    /// The kinds of tokens that may appear in a QIF file.
    /// </summary>
    public enum TokenKind
    {
        /// <summary>
        /// The beginning of the file, before <see cref="Read"/> is called.
        /// </summary>
        BOF,

        /// <summary>
        /// The end of the file, after <see cref="Read"/> has returned <see langword="false"/>.
        /// </summary>
        EOF,

        /// <summary>
        /// The reader is positioned at a <c>!Type:</c> header.
        /// </summary>
        Type,

        /// <summary>
        /// The reader is positioned at a detail line.
        /// </summary>
        Field,

        /// <summary>
        /// The reader is positioned at the end of a list of fields for an individual record.
        /// </summary>
        EndOfRecord,
    }

    /// <summary>
    /// Gets the kind of token that the reader is positioned at.
    /// </summary>
    public TokenKind Kind { get; private set; } = TokenKind.BOF;

    /// <summary>
    /// Gets the most recently read <see cref="TokenKind.Type" /> token.
    /// </summary>
    public ReadOnlyMemory<char>? CurrentType { get; private set; }

    /// <summary>
    /// Gets the details of the field at the current reader position.
    /// </summary>
    public (ReadOnlyMemory<char> Header, ReadOnlyMemory<char> Value)? Field { get; private set; }

    private string DebuggerDisplay => $"{this.Kind}";

    /// <inheritdoc/>
    public void Dispose()
    {
        this.reader.Dispose();
    }

    /// <summary>
    /// Reads the next token from the input stream.
    /// </summary>
    /// <returns>The kind of token that was read. Get details of the token from properties on this class.</returns>
    public TokenKind Read()
    {
        string? line = this.reader.ReadLine();
        if (line is null)
        {
            return this.Kind = TokenKind.EOF;
        }

        ThrowIfNot(line.Length > 0, "Unexpected empty line in data file.");

        this.Field = null;

        switch (line[0])
        {
            case '!':
                ThrowIfNot(line.StartsWith(TypeHeader, StringComparison.Ordinal), "Invalid type header.");
                this.Kind = TokenKind.Type;
                this.CurrentType = line.AsMemory(TypeHeader.Length);
                break;
            case '^':
                ThrowIfNot(line.Length == 1, "End of record line too long");
                this.Kind = TokenKind.EndOfRecord;
                break;
            case 'X':
                ThrowIfNot(line.Length > 1, "Line too short");
                this.Kind = TokenKind.Field;
                this.Field = (line.AsMemory(0, 2), line.AsMemory(2));
                break;
            default:
                this.Kind = TokenKind.Field;
                this.Field = (line.AsMemory(0, 1), line.AsMemory(1));
                break;
        }

        return this.Kind;
    }

    private static void ThrowIfNot(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidTransactionException(message);
        }
    }
}
