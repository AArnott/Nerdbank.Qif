// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// Extension methods to help with processing QIF documents.
/// </summary>
public static class QifUtilities
{
    /// <summary>
    /// Checks whether a string is equal to a sequence of characters.
    /// </summary>
    /// <param name="value">The string.</param>
    /// <param name="memory">The sequence of characters.</param>
    /// <returns>A value indicating whether the two arguments are equal.</returns>
    public static bool Equals(string value, ReadOnlyMemory<char> memory) => Equals(value, memory.Span);

    /// <inheritdoc cref="Equals(string, ReadOnlyMemory{char})"/>
    public static bool Equals(string value, ReadOnlySpan<char> span)
    {
        if (value.Length != span.Length)
        {
            return false;
        }

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] != value[i])
            {
                return false;
            }
        }

        return true;
    }

    internal static T ValueOrThrow<T>(T? value, string fieldName)
        where T : struct
    {
        if (value is null)
        {
            throw new InvalidTransactionException("Missing required field: " + fieldName);
        }

        return value.Value;
    }

    internal static T ValueOrThrow<T>(T? value, string fieldName)
        where T : class
    {
        if (value is null)
        {
            throw new InvalidTransactionException("Missing required field: " + fieldName);
        }

        return value;
    }
}
