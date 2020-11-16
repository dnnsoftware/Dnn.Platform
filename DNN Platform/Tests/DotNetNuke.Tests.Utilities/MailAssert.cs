// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using NUnit.Framework;

    public static class MailAssert
    {
        public static void Base64EncodedContentLineContains(string expectedValue, string toAddress, string findByText)
        {
            var contentLine = ConvertEmailContentFromBase64ToUnicode(GetEmailFileName(toAddress, findByText));

            Assert.IsTrue(contentLine.Contains(expectedValue));
        }

        public static void ContentLineContains(string expectedValue, string toAddress, string findByText)
        {
            var contentLine = FindContentUsingRegex(expectedValue, GetEmailFileName(toAddress, findByText));

            Assert.IsFalse(string.IsNullOrEmpty(contentLine));
        }

        public static void FromLineContains(string expectedValue, string toAddress, string findByText)
        {
            string fromLine = FindEmailLineUsingRegex("From", GetEmailFileName(toAddress, findByText));

            Assert.IsTrue(fromLine.Contains(expectedValue));
        }

        public static void MessageSent(string expectedValue, string toAddress, string message)
        {
            GetEmailFileName(toAddress, expectedValue);
        }

        public static void ReplyToLineContains(string expectedValue, string toAddress, string findByText)
        {
            string replyToLine = FindEmailLineUsingRegex("Reply-To", GetEmailFileName(toAddress, findByText));

            Assert.IsTrue(replyToLine.Contains(expectedValue));
        }

        public static void SubjectLineContains(string expectedValue, string toAddress, string findByText)
        {
            string subjectLine = FindEmailLineUsingRegex("Subject", GetEmailFileName(toAddress, findByText));

            Assert.IsTrue(subjectLine.Contains(expectedValue));
        }

        public static void ToLineContains(string expectedValue, string toAddress, string findByText)
        {
            string toLine = FindEmailLineUsingRegex("To", GetEmailFileName(toAddress, findByText));

            Assert.IsTrue(toLine.Contains(expectedValue));
        }

        private static string ConvertEmailContentFromBase64ToUnicode(string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);

            // Separate the content from the EmailAddress information
            const string contentLineRegex = "(base64)(.*(\n)+)+";
            Match match = Regex.Match(@emailContent, contentLineRegex, RegexOptions.Singleline);
            Assert.IsTrue(match.Success, "Could not find content Line! Looking in file: " + emailFileName);

            // Convert the EmailAddress content
            emailContent = match.Groups[2].Value.Replace("\r\n", string.Empty);

            byte[] b = Convert.FromBase64String(emailContent);
            string decodedEmailContent = Encoding.ASCII.GetString(b);
            return decodedEmailContent;
        }

        private static string FindContentUsingRegex(string content, string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);
            string contentLineRegex = @content + ".*";
            Match match = Regex.Match(@emailContent, @contentLineRegex);
            Assert.IsTrue(match.Success, "Could not find content Line! Looking in file: " + emailFileName);
            string contentLine = match.Value;
            return contentLine;
        }

        private static string FindEmailLineUsingRegex(string linePrefix, string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);
            string lineRegex = @"^(" + linePrefix + @"): (.+)";
            Match match = Regex.Match(@emailContent, @lineRegex, RegexOptions.Multiline);
            Assert.IsTrue(match.Success, "Could not find " + linePrefix + " Line! Looking in file: " + emailFileName);
            string line = match.Value;
            return line;
        }

        private static string FindLineUsingRegex(string linePrefix, string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);
            string lineRegex = linePrefix + @": .*";
            Match match = Regex.Match(@emailContent, @lineRegex);
            Assert.IsTrue(match.Success, "Could not find " + linePrefix + " Line! Looking in file: " + emailFileName);
            string line = match.Value;
            return line;
        }

        private static string GetEmailFileName(string toAddress, string findByText)
        {
            return GetEmailFileName(toAddress, findByText, "The newsletter sent to " + toAddress + " could not be found");
        }

        private static string GetEmailFileName(string toAddress, string findByText, string message)
        {
            var physicalPath = Directory.GetCurrentDirectory();
            var packagePath = physicalPath.Replace("\\Fixtures", "\\Packages");
            var emailPath = packagePath + "\\TestEmails";
            var emailFileName = string.Empty;

            foreach (var email in Directory.GetFiles(emailPath))
            {
                string emailContent = File.ReadAllText(email);
                if (emailContent.Contains(findByText) && emailContent.Contains("To: " + toAddress))
                {
                    emailFileName = email;
                    break;
                }
            }

            Assert.IsTrue(emailFileName != null, message + " The test was searching in: " + emailPath);

            return emailFileName;
        }
    }
}
