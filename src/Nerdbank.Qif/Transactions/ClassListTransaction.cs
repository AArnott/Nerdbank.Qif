// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// A class list transaction.
/// </summary>
public class ClassListTransaction
{
    /// <summary>
    /// Gets or sets the name of the class.
    /// </summary>
    /// <value>The name of the class.</value>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString() => this.ClassName;
}
