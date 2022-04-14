// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class ClassTests : TestBase
{
    public ClassTests(ITestOutputHelper logger)
        : base(logger)
    {
    }

    [Fact]
    public void Load_Simple()
    {
        const string qifSource = @"NBonus
^";
        using QifReader reader = new(new StringReader(qifSource));
        Class clazz = Class.Load(reader);
        Assert.Equal("Bonus", clazz.Name);
        Assert.Null(clazz.Description);
    }

    [Fact]
    public void Load_Exhaustive()
    {
        const string qifSource = @"NBonus
DA bonus
^";
        using QifReader reader = new(new StringReader(qifSource));
        Class clazz = Class.Load(reader);
        Assert.Equal("Bonus", clazz.Name);
        Assert.Equal("A bonus", clazz.Description);
    }
}
