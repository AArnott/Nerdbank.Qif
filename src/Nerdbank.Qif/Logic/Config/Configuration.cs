// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace Nerdbank.Qif;

/// <summary>
/// The configuration class to use during QIF file processing.
/// </summary>
public class Configuration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Configuration"/> class.
    /// </summary>
    public Configuration()
    {
        // Explicitly set the write format modes.
        this.WriteDateFormatMode = WriteDateFormatMode.Default;
        this.WriteDecimalFormatMode = WriteDecimalFormatMode.Default;
        this.ReadDateFormatMode = ReadDateFormatMode.Default;
        this.ParseNumberStyles = NumberStyles.Any;
        this.ParseDateTimeStyles = DateTimeStyles.None;
    }

    /// <summary>
    /// Gets or sets the date format mode to use when writing dates. See WriteDateFormatMode for more information.
    /// </summary>
    /// <value>The date format mode to use when writing dates.</value>
    public WriteDateFormatMode WriteDateFormatMode { get; set; }

    /// <summary>
    /// Gets or sets the custom date format when the WriteDateFormatMode is set to <b>Custom</b>.
    /// </summary>
    /// <value>The custom date format when the WriteDateFormatMode is set to <b>Custom</b>.</value>
    public string? CustomWriteDateFormat { get; set; }

    /// <summary>
    /// Gets or sets the decimal format mode to use when writing decimals. See WriteDecimalFormatMode for more information.
    /// </summary>
    /// <value>The decimal format mode to use when writing decimals.</value>
    public WriteDecimalFormatMode WriteDecimalFormatMode { get; set; }

    /// <summary>
    /// Gets or sets the custom decimal format when the WriteDecimalFormatMode is set to <b>Custom</b>.
    /// </summary>
    /// <value>The custom decimal format when the WriteDecimalFormatMode is set to <b>Custom</b>.</value>
    public string? CustomWriteDecimalFormat { get; set; }

    /// <summary>
    /// Gets or sets the date format mode to use when parsing dates. See ReadDateFormatMode for more information.
    /// </summary>
    /// <value>The date format mode to use when parsing dates.</value>
    public ReadDateFormatMode ReadDateFormatMode { get; set; }

    /// <summary>
    /// Gets or sets the custom date format when the ReadDateFormatMode is set to <b>Custom</b>.
    /// </summary>
    /// <value>The custom date format when the ReadDateFormatMode is set to <b>Custom</b>.</value>
    public string? CustomReadDateFormat { get; set; }

    /// <summary>
    /// Gets or sets the custom <see cref="CultureInfo"/> to use when reading data.
    /// </summary>
    /// <remarks>
    /// Overrides the current <see cref="CultureInfo"/> while importing.
    /// </remarks>
    public CultureInfo? CustomReadCultureInfo { get; set; }

    /// <summary>
    /// Gets or sets the date time styles to use while parsing dates.
    /// </summary>
    /// <value>The date time styles to use while parsing dates.</value>
    public DateTimeStyles ParseDateTimeStyles { get; set; }

    /// <summary>
    /// Gets or sets the custom decimal format when parsing decimals.
    /// </summary>
    /// <value>The custom decimal format when parsing decimals.</value>
    public NumberStyles ParseNumberStyles { get; set; }
}
