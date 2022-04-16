// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public abstract class TestBase
{
    protected const string SampleDataFile = "sample.qif";
    protected const string ReaderInputsDataFile = "QifReaderTestsInput.qif";

    public TestBase(ITestOutputHelper logger)
    {
        this.Logger = logger;
    }

    protected ITestOutputHelper Logger { get; }

    protected static Stream GetSampleDataStream(string fileName)
    {
        return File.OpenRead(Path.Combine("SampleData", fileName));
    }

    protected static void AssertEqual(string expectedValue, ReadOnlyMemory<char> actualValue)
    {
        if (!QifUtilities.Equals(expectedValue, actualValue))
        {
            throw new Xunit.Sdk.EqualException(expectedValue, actualValue.ToString());
        }
    }
}
