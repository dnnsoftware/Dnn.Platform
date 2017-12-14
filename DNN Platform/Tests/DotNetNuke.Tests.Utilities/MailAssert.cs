#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace DotNetNuke.Tests.Utilities
{
    public static class MailAssert
    {
        #region Private Methods

        private static string ConvertEmailContentFromBase64ToUnicode(string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);

            //Separate the content from the EmailAddress information
            const string contentLineRegex = "(base64)(.*(\n)+)+";
            Match match = Regex.Match(@emailContent, contentLineRegex, RegexOptions.Singleline);
            Assert.IsTrue(match.Success, "Could not find content Line! Looking in file: " + emailFileName);

            //Convert the EmailAddress content
            emailContent = match.Groups[2].Value.Replace("\r\n", "");

            byte[] b = Convert.FromBase64String(emailContent);
            String decodedEmailContent = Encoding.ASCII.GetString(b);
            return decodedEmailContent;
        }

        private static string FindContentUsingRegex(string content, string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);
            string contentLineRegex = @content + ".*";
            Match match = Regex.Match(@emailContent, @contentLineRegex);
            Assert.IsTrue(match.Success, "Could not find content Line! Looking in file: " + emailFileName);
            String contentLine = match.Value;
            return contentLine;
        }

        private static string FindEmailLineUsingRegex(string linePrefix, string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);
            string lineRegex = @"^(" + linePrefix + @"): (.+)";
            Match match = Regex.Match(@emailContent, @lineRegex, RegexOptions.Multiline);
            Assert.IsTrue(match.Success, "Could not find " + linePrefix + " Line! Looking in file: " + emailFileName);
            String line = match.Value;
            return line;
        }

        private static string FindLineUsingRegex(string linePrefix, string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);
            string lineRegex = linePrefix + @": .*";
            Match match = Regex.Match(@emailContent, @lineRegex);
            Assert.IsTrue(match.Success, "Could not find " + linePrefix + " Line! Looking in file: " + emailFileName);
            String line = match.Value;
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
            var emailFileName = String.Empty;

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

        #endregion

        #region Public Methods

        public static void Base64EncodedContentLineContains(string expectedValue, string toAddress, string findByText)
        {
            var contentLine = ConvertEmailContentFromBase64ToUnicode(GetEmailFileName(toAddress, findByText));

            Assert.IsTrue(contentLine.Contains(expectedValue));
        }

        public static void ContentLineContains(string expectedValue, string toAddress, string findByText)
        {
            var contentLine = FindContentUsingRegex(expectedValue, GetEmailFileName(toAddress, findByText));

            Assert.IsFalse(String.IsNullOrEmpty(contentLine));
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

        #endregion
    }
}
