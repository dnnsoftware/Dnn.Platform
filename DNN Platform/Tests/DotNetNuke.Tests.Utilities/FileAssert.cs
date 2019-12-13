using System;
using System.IO;

using NUnit.Framework;

namespace DotNetNuke.Tests.Utilities
{
    public static class FileAssert
    {
        public static void TextFilesAreEqual(string expectedFile, string actualFile)
        {
            TextFilesAreEqual(expectedFile, actualFile, String.Empty);
        }

        public static void TextFilesAreEqual(string expectedFile, string actualFile, string message)
        {
            string expectedContent = File.ReadAllText(expectedFile);
            string actualContent = File.ReadAllText(actualFile);
            Assert.AreEqual(expectedContent, actualContent, message);
        }

        public static void BinaryFilesAreEqual(string expectedFile, string actualFile)
        {
            BinaryFilesAreEqual(expectedFile, actualFile, String.Empty);
        }

        public static void BinaryFilesAreEqual(string expectedFile, string actualFile, string message)
        {
            byte[] expectedContent = File.ReadAllBytes(expectedFile);
            byte[] actualContent = File.ReadAllBytes(actualFile);
            CollectionAssert.AreEqual(expectedContent, actualContent, message);
        }
    }
}
