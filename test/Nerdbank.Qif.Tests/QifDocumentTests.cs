// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class QifDocumentTests
{
    [Fact]
    public void Load_ValidatesArguments()
    {
        Assert.Throws<ArgumentNullException>("reader", () => QifDocument.Load((TextReader)null!));
        Assert.Throws<ArgumentNullException>("fileName", () => QifDocument.Load((string)null!));
    }
}
