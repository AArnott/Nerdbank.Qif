# Nerdbank.Qif

[![NuGet package](https://img.shields.io/nuget/v/Nerdbank.Qif.svg)](https://nuget.org/packages/Nerdbank.Qif)
[![Build Status](https://dev.azure.com/andrewarnott/OSS/_apis/build/status/Nerdbank.Qif/Nerdbank.Qif?branchName=main)](https://dev.azure.com/andrewarnott/OSS/_build/latest?definitionId=66&branchName=main)

This is a library for creating, reading, modifying and writing Quicken Interchange Format (QIF) files.

The following data types are supported in QIF files:

* Bank transactions
* Investment transactions
* Memorized transactions
* Classes
* Categories
* Tags
* Price history
* Securities
* Accounts

Most use cases will leverage `QifDocument` for its ease in representing an entire QIF file for import/export.
In some cases you may find the lower-level `QifReader` and `QifWriter` (or even the `QifParser`) useful.

Multi-culture support is built-in. Some methods are virtual, allowing you to tweak the exact QIF syntax where necessary.

## Sample usage

```cs
using Nerdbank.Qif;

var document = QifDocument.Load("quicken.qif");

// create or modify transactions...

// This writes the QifDom to a file (overwrites if file is already present).
document.Save("quicken.qif");
```

All transactions present in the DOM are written according to the QIF file format specification. Dates and numbers should be written according to globalization standards.
