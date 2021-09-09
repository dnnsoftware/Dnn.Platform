namespace DotNetNuke.Tests.Core.Framework
{
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
            var argToPass = new object[] { 1 };

            // Act
            var result = Reflection.CreateInstance(typeToCreate, argToPass) as StringBuilder;

            // Assert
            Assert.AreEqual(1, result.Capacity);
        }

        [Test]
        public void CreateInstance_WithoutArgs_WorksCorrectly()
        {
            // Arrange
            var typeToCreate = typeof(StringBuilder);

            // Act
            var result = Reflection.CreateInstance(typeToCreate) as StringBuilder;

            // Assert
            Assert.AreEqual(16, result.Capacity);
        }
    }
}
