// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Qif;

/// <summary>
/// A price point for some <see cref="Security"/> on a particular date.
/// </summary>
/// <param name="Symbol">The symbol for this security.</param>
/// <param name="Value">The price of this security.</param>
/// <param name="Date">The date when of this price point.</param>
public record Price(string Symbol, decimal Value, DateTime Date);
