# qif
Fork of the robaker/qif utility project (itself a fork of MisterY/qif) for creating and consuming *.qif files that targets netstandard2.1.

This solution includes a driver console app that implements some transformations that I wanted to fix broken Fidelity netbenefits QIF exports that Quicken couldn't handle correctly. 

I don't warrant that this is useful to anyone but myself, nor that it won't cause terrible things to happen to your qif files. 

Original notes:

The qif project complies with documented Quicken Interchange Format (QIF) files specification. It is a completely managed, open source QIF API.

This API enables you to import or export *.qif files. It also allows you to create or modify transactions and represent them easily in code as entities. This is written exclusively in C#, targeting .NET Standard 1.3. Basically, this API can enable your application to completely handle any aspect of a QIF file. You can create transactions in an easy to understand model and export it to the versatile Quicken export file format, or you can easily import your QIF file, and have immediate access to all transactions in the file.

This project only produces a class library (*.dll). Just drop this library in place and reference it in your .NET application, and you're ready to go.

Here's a sample usage:

**C# / .NET Example**
```csharp
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

**Automation Example in VB**
```
Sub Sample()
    Dim a As New QifDomComWrapper
    
    ' Import sample QIF file
    a.Import "c:\sample_import.qif"
    
    ' Prepare a new transaction
    Dim c As New BasicTransaction
    
    ' Set the payee property
    With c
        .Payee = "Test Payee"
    End With
    
    ' Add the transaction
    a.BankTransactions.Add c
    
    ' Export the current QIF DOM
    a.Export "c:\sample_export.qif"
End Sub
```
All transactions present in the DOM are written according to the QIF file format specification. Dates and numbers should be written according to globalization standards.

**Note**: With the upcoming changes to target .NET Standard, some or all aspects of VB automation may no longer work. You can test the latest beta at [https://www.nuget.org/packages/QifApi/1.1.0-beta1](https://www.nuget.org/packages/QifApi/1.1.0-beta1). Feel free to open an issue or PR.

