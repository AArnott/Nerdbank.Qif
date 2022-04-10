// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif.Logic;

/// <summary>
/// The class list fields used in transactions.
/// </summary>
internal static class ClassListFields
{
    /// <summary>
    /// Class name.
    /// </summary>
    internal const string ClassName = "N";

    /// <summary>
    /// Description.
    /// </summary>
    internal const string Description = "D";

    /// <summary>
    /// End of entry.
    /// </summary>
    internal const string EndOfEntry = "^";
}
