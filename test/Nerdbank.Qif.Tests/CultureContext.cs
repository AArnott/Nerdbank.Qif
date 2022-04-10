// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

/// <summary>
/// A culture swapping context. When instantiated, the <see cref="CultureInfo.CurrentCulture"/>
/// is saved, then replaced by the specified culture.
/// </summary>
internal class CultureContext : IDisposable
{
    private readonly CultureInfo previousCulture;

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureContext"/> class.
    /// </summary>
    /// <param name="nameOfCultureToUse">The language tag of the culture to use (e.g., en-US, es-MX, ar-SA, etc).</param>
    public CultureContext(string nameOfCultureToUse)
        : this(new CultureInfo(nameOfCultureToUse))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureContext"/> class.
    /// </summary>
    /// <param name="cultureToUse">The <see cref="CultureInfo"/> to replace <see cref="CultureInfo.CurrentCulture"/>.</param>
    public CultureContext(CultureInfo cultureToUse)
    {
        this.previousCulture = CultureInfo.CurrentCulture;

        CultureInfo.CurrentCulture = cultureToUse;
    }

    /// <summary>
    /// This method restores the <see cref="CultureInfo.CurrentCulture"/> instance that had been replaced
    /// when this instance was created.
    /// </summary>
    public void Dispose()
    {
        CultureInfo.CurrentCulture = this.previousCulture;
    }
}
