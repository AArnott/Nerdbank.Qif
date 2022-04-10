# Nerdbank.Qif

![NuGet package](https://img.shields.io/badge/Nerdbank.Qif.svg)

[![Build Status](https://dev.azure.com/andrewarnott/OSS/_apis/build/status/Nerdbank.Qif/Nerdbank.Qif?branchName=main)](https://dev.azure.com/andrewarnott/OSS/_build/latest?definitionId=66&branchName=main)

This is a library for creating, reading, modifying and writing Quicken Interchange Format (QIF) files.

## Sample usage

```cs
// This returns a QifDom object (defined in the QifImport namespace). The QifDom represents all transactions found in the QIF file.
QifDom qifDom = QifDom.Import.ImportFile(@"c:\quicken.qif");
// --or--
QifDom qd = new QifDom();
qd.Import(@"c:\quicken.qif"); // NOTE: This will replace existing transactions in the QifDom instance.

... /* create or modify transactions */ ...

// This writes the QifDom to a file (overwrites if file is already present).
QifDom.ExportFile(qifDom, @"c:\quicken.qif");
// --or--
qd.Export(@"c:\quicken.qif");
```

All transactions present in the DOM are written according to the QIF file format specification. Dates and numbers should be written according to globalization standards.
