// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Globalization;

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

    internal static string GetDateString(this DateTime @this, Configuration config)
    {
        string? result = null;

        if (config.WriteDateFormatMode == WriteDateFormatMode.Default)
        {
            result = @this.ToString("d"); // Short date string
        }
        else
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(config.CustomWriteDateFormat));
            result = @this.ToString(config.CustomWriteDateFormat);
        }

        return result;
    }

    internal static string GetDecimalString(this decimal @this, Configuration config)
    {
        string? result = null;

        if (config.WriteDecimalFormatMode == WriteDecimalFormatMode.Default)
        {
            result = @this.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(config.CustomWriteDecimalFormat));
            result = @this.ToString(config.CustomWriteDecimalFormat);
        }

        return result;
    }

    internal static DateTime ParseDateString(this string @this, Configuration config)
    {
        // Prepare the return value
        DateTime result;
        bool success = false;

        if (config.ReadDateFormatMode == ReadDateFormatMode.Default)
        {
            success = DateTime.TryParse(GetRealDateString(@this), config.CustomReadCultureInfo ?? CultureInfo.CurrentCulture, config.ParseDateTimeStyles, out result);
        }
        else
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(config.CustomReadDateFormat));
            success = DateTime.TryParseExact(GetRealDateString(@this), config.CustomReadDateFormat, config.CustomReadCultureInfo ?? CultureInfo.CurrentCulture, config.ParseDateTimeStyles, out result);
        }

        // If parsing failed
        if (!success)
        {
            // Identify that the value couldn't be formatted
            throw new InvalidCastException(Strings.InvalidDateFormat);
        }

        // Return the date value
        return result;
    }

    internal static decimal ParseDecimalString(this string @this, Configuration config)
    {
        decimal result = 0m;

        // If parsing failed
        if (!decimal.TryParse(@this, config.ParseNumberStyles, config.CustomReadCultureInfo ?? CultureInfo.CurrentCulture, out result))
        {
            throw new InvalidCastException(Strings.InvalidDecimalFormat);
        }

        return result;
    }

    private static string GetRealDateString(string qifDateString)
    {
        // Find the apostrophe
        int i = qifDateString.IndexOf("'", StringComparison.Ordinal);

        // Prepare the return string
        string result = string.Empty;

        // If the apostrophe is present
        if (i != -1)
        {
            // Extract everything but the apostrophe
            result = qifDateString.Substring(0, i) + "/" + qifDateString.Substring(i + 1);

            // Replace spaces with zeros
            result = result.Replace(" ", "0");

            // Return the new string
            return result;
        }
        else
        {
            // Otherwise, just return the raw value
            return qifDateString;
        }
    }
}
