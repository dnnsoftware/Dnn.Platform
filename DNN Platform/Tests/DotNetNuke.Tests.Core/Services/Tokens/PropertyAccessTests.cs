#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Globalization;

using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Tests.Utilities;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Services.Tokens
{
    [TestFixture]
    public class PropertyAccessTests
    {
        [Test]
        [TestCase("now", false)]
        [TestCase("today", false)]
        [TestCase("ticksperday", false)]
        [TestCase("anything", true)]
        [TestCase("", true)]
        public void TicksPropertyAcccess_GetProperty_Sets_PropertyNotFound(string propertyName, bool expected)
        {
            //Arrange
            var ticksPropertyAccess = new TicksPropertyAccess();
            var accessingUser = new UserInfo();

            //Act
            bool propertyNotFound = false;
            string propertyValue = ticksPropertyAccess.GetProperty(propertyName, "", CultureInfo.InvariantCulture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            //Assert
            Assert.AreEqual(expected, propertyNotFound);
        }
        
        [Test]
        [TestCase("today")]
        [TestCase("ticksperday")]
        public void TicksPropertyAcccess_GetProperty_Returns_Correct_String(string propertyName)
        {
            //Arrange
            var ticksPropertyAccess = new TicksPropertyAccess();
            var accessingUser = new UserInfo();

            long expected = DateTime.MinValue.Ticks;
            switch (propertyName)
            {
                case "now":
                    expected = DateTime.Now.Ticks;
                    break;
                case "today":
                    expected = DateTime.Today.Ticks;
                    break;
                case "ticksperday":
                    expected = TimeSpan.TicksPerDay;
                    break;
            }

            //Act
            bool propertyNotFound = false;
            string propertyValue = ticksPropertyAccess.GetProperty(propertyName, "", CultureInfo.InvariantCulture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            //Assert
            Assert.AreEqual(expected.ToString(CultureInfo.InvariantCulture), propertyValue);
        }

        [Test]
        [TestCase("current", false)]
        [TestCase("now", false)]
        [TestCase("system", false)]
        [TestCase("utc", false)]
        [TestCase("anything", true)]
        [TestCase("", true)]
        public void DateTimePropertyAcccess_GetProperty_Sets_PropertyNotFound(string propertyName, bool expected)
        {
            //Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var accessingUser = new UserInfo {Profile = new UserProfile {PreferredTimeZone = TimeZoneInfo.Local}};


            //Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, "", CultureInfo.InvariantCulture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            //Assert
            Assert.AreEqual(expected, propertyNotFound);
        }

        [Test]
        [TestCase("current", "")]
        [TestCase("now", "")]
        [TestCase("system", "")]
        [TestCase("utc", "")]
        [TestCase("current", "fr")]
        [TestCase("now", "de")]
        [TestCase("system", "en")]
        [TestCase("utc", "it")]
        public void DateTimePropertyAcccess_GetProperty_Returns_Correct_String_For_Culture(string propertyName, string cultureName)
        {
            //Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var accessingUser = new UserInfo { Profile = new UserProfile { PreferredTimeZone = TimeZoneInfo.Local } };
            var culture = new CultureInfo(cultureName);

            string expected = String.Empty;

            switch (propertyName)
            {
                case "current":
                    expected = DateTime.Now.ToString("D", culture);
                    break;
                case "now":
                    expected = DateTime.Now.ToString("g", culture);
                    break;
                case "system":
                    expected = DateTime.Now.ToString("g", culture);
                    break;
                case "utc":
                    expected = DateTime.Now.ToUniversalTime().ToString("g", culture);
                    break;
            }


            //Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, "", culture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            //Assert
            Assert.AreEqual(expected, propertyValue);
        }

         [Test]
         [TestCase("current", "", "D")]
         [TestCase("now", "", "g")]
         [TestCase("system", "", "g")]
         [TestCase("utc", "", "g")]
         [TestCase("current", "fr", "g")]
         [TestCase("now", "de", "mmm yyyy")]
         [TestCase("system", "en", "dd/mm/yy")]
         [TestCase("utc", "it", "mmm dd, yyyy")]
         public void DateTimePropertyAcccess_GetProperty_Returns_Correct_String_Given_Format_And_Culture(string propertyName, string cultureName, string format)
         {
             //Arrange
             var dtPropertyAccess = new DateTimePropertyAccess();
             var accessingUser = new UserInfo { Profile = new UserProfile { PreferredTimeZone = TimeZoneInfo.Local } };
             var culture = new CultureInfo(cultureName);


             string expected = String.Empty;

             switch (propertyName)
             {
                 case "current":
                     expected = DateTime.Now.ToString(format, culture);
                     break;
                 case "now":
                     expected = DateTime.Now.ToString(format, culture);
                     break;
                 case "system":
                     expected = DateTime.Now.ToString(format, culture);
                     break;
                 case "utc":
                     expected = DateTime.Now.ToUniversalTime().ToString(format, culture);
                     break;
             }


             //Act
             bool propertyNotFound = false;
             string propertyValue = dtPropertyAccess.GetProperty(propertyName, format, culture,
                                                                    accessingUser, Scope.DefaultSettings, ref propertyNotFound);

             //Assert
             Assert.AreEqual(expected, propertyValue);
         }

        [Test]
        [TestCase("current", "Tokyo Standard Time")]
        [TestCase("now", "Azores Standard Time")]
        public void DateTimePropertyAcccess_GetProperty_Adjusts_For_TimeZone(string propertyName, string timeZoneId)
        {
            //Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timeZoneProfileProperty = new ProfilePropertyDefinition(Constants.PORTAL_Zero)
                                              {
                                                  PropertyName = "PreferredTimeZone",
                                                  PropertyValue = timeZoneId
                                              };
            var userProfile = new UserProfile();
            userProfile.ProfileProperties.Add(timeZoneProfileProperty);

            var accessingUser = new UserInfo { Profile = userProfile };
            var culture = CultureInfo.InvariantCulture;

            string expected = String.Empty;

            switch (propertyName)
            {
                case "current":
                    expected = TimeZoneInfo.ConvertTime(DateTime.Now, userTimeZone).ToString("D", culture);
                    break;
                case "now":
                    expected = TimeZoneInfo.ConvertTime(DateTime.Now, userTimeZone).ToString("g", culture);
                    break;
                case "system":
                    expected = DateTime.Now.ToString("g", culture);
                    break;
                case "utc":
                    expected = DateTime.Now.ToUniversalTime().ToString("g", culture);
                    break;
            }


            //Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, "", culture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            //Assert
            Assert.AreEqual(expected, propertyValue);
        }
    }
}
