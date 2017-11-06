using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Dnn.PersonaBar.Library.Prompt.Common
{
    public class EmailValidator
    {
        private bool _invalid;
        public bool IsValid(string emailToTest)
        {
            if (string.IsNullOrEmpty(emailToTest))
                return false;

            // Use IdnMapping class to convert unicode domain names (https://msdn.microsoft.com/en-us/library/system.globalization.idnmapping(v=vs.110).aspx)
            try
            {
                emailToTest = Regex.Replace(emailToTest, "(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException ex)
            {
                return false;
            }

            if (_invalid)
                return false;

            // Return True if valid email format
            try
            {
                return Regex.IsMatch(emailToTest, "^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))" + "(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-\\w]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values
            var idn = new IdnMapping();

            var domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (Exception)
            {
                _invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}