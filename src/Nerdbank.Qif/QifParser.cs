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
public partial class QifParser : IDisposable
{
    private static readonly char[] Whitespace = new[] { '\t', ' ' };
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
    /// Gets the kind of token that the reader is positioned at.
    /// </summary>
    public QifToken Kind { get; private set; } = QifToken.BOF;

    /// <summary>
    /// Gets the line number containing the current token.
    /// </summary>
    public int LineNumber { get; private set; }

    /// <summary>
    /// Gets the most recently read <see cref="QifToken.Header" /> token.
    /// </summary>
    public (ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) CurrentHeader { get; private set; }

    /// <summary>
    /// Gets the details of the field at the current reader position.
    /// </summary>
    public (ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) Field { get; private set; }

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
    public QifToken Read()
    {
        string? line = this.reader.ReadLine();
        if (line is null)
        {
            return this.Kind = QifToken.EOF;
        }

        this.LineNumber++;
        ThrowIfNot(line.Length > 0, this.LineNumber, "Unexpected empty line in data file.");

        this.Field = default;

        switch (line[0])
        {
            case '!':
                ThrowIfNot(line.Length > 1, this.LineNumber, "Line too short.");
                this.Kind = QifToken.Header;
                int colonIndex = line.IndexOf(':');
                this.CurrentHeader = colonIndex < 0
                    ? (line.AsMemory(1), default)
                    : (line.AsMemory(1, colonIndex - 1), TrimEnd(line.AsMemory(colonIndex + 1)));
                break;
            case '^':
                ThrowIfNot(line.Length == 1, this.LineNumber, "End of record line too long");
                this.Kind = QifToken.EndOfRecord;
                break;
            case 'X':
                ThrowIfNot(line.Length >= 2, this.LineNumber, "Line too short.");
                this.Kind = QifToken.Field;
                this.Field = (line.AsMemory(0, 2), TrimEnd(line.AsMemory(2)));
                break;
            default:
                this.Kind = QifToken.Field;
                this.Field = (line.AsMemory(0, 1), TrimEnd(line.AsMemory(1)));
                break;
        }

        static ReadOnlyMemory<char> TrimEnd(ReadOnlyMemory<char> value)
        {
            int trailingWhitespaceCharacterCount = 0;
            ReadOnlySpan<char> span = value.Span;
            for (int i = span.Length - 1; i >= 0; i--)
            {
                bool wasWhitespaceDetected = false;
                for (int j = 0; j < Whitespace.Length; j++)
                {
                    if (span[i] == Whitespace[j])
                    {
                        wasWhitespaceDetected = true;
                        break;
                    }
                }

                if (wasWhitespaceDetected)
                {
                    trailingWhitespaceCharacterCount++;
                }
                else
                {
                    break;
                }
            }

            return value.Slice(0, value.Length - trailingWhitespaceCharacterCount);
        }

        return this.Kind;
    }

    private static void ThrowIfNot(bool condition, int lineNumber, string message)
    {
        if (!condition)
        {
            throw new InvalidTransactionException($"Line {lineNumber}: {message}");
        }
    }
}
