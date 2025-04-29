namespace DotNetNuke.Tests.Core.Framework;

using System.Text;

using DotNetNuke.Framework;
using NUnit.Framework;

public class ReflectionTests
{
    [Test]
    public void CreateInstance_WithArgs_WorksCorrectly()
    {
        // Arrange
        var typeToCreate = typeof(StringBuilder);
        var argToPass = new object[] { "one" };

        // Act
        var result = (StringBuilder)Reflection.CreateInstance(typeToCreate, argToPass);

        // Assert
        Assert.That(result.ToString(), Is.EqualTo("one"));
    }

    [Test]
    public void CreateInstance_WithoutArgs_WorksCorrectly()
    {
        // Arrange
        var typeToCreate = typeof(StringBuilder);

        // Act
        var result = (StringBuilder)Reflection.CreateInstance(typeToCreate);

        // Assert
        Assert.That(result.ToString(), Is.EqualTo(string.Empty));
    }
}
