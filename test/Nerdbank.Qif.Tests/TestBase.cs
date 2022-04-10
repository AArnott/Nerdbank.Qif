// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public abstract class TestBase
{
    protected const string SampleDataFile = "sample.qif";

    protected static Stream GetSampleDataStream(string fileName)
    {
        return File.OpenRead(Path.Combine("SampleData", fileName));
    }
}
