// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.Tokens
{
    using System;
    using System.Globalization;

    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

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
            // Arrange
            var ticksPropertyAccess = new TicksPropertyAccess();
            var accessingUser = new UserInfo();

            // Act
            bool propertyNotFound = false;
            string propertyValue = ticksPropertyAccess.GetProperty(propertyName, string.Empty, CultureInfo.InvariantCulture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
            Assert.AreEqual(expected, propertyNotFound);
        }

        [Test]
        [TestCase("today")]
        [TestCase("ticksperday")]
        public void TicksPropertyAcccess_GetProperty_Returns_Correct_String(string propertyName)
        {
            // Arrange
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

            // Act
            bool propertyNotFound = false;
            string propertyValue = ticksPropertyAccess.GetProperty(propertyName, string.Empty, CultureInfo.InvariantCulture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
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
            // Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var accessingUser = new UserInfo { Profile = new UserProfile { PreferredTimeZone = TimeZoneInfo.Local } };

            // Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, string.Empty, CultureInfo.InvariantCulture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
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
            // Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var accessingUser = new UserInfo { Profile = new UserProfile { PreferredTimeZone = TimeZoneInfo.Local } };
            var culture = new CultureInfo(cultureName);

            string expected = string.Empty;

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

            // Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, string.Empty, culture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
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
            // Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var accessingUser = new UserInfo { Profile = new UserProfile { PreferredTimeZone = TimeZoneInfo.Local } };
            var culture = new CultureInfo(cultureName);

            string expected = string.Empty;

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

            // Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, format, culture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
            Assert.AreEqual(expected, propertyValue);
        }

        [Test]
        [TestCase("current", "Tokyo Standard Time")]
        [TestCase("now", "Azores Standard Time")]
        public void DateTimePropertyAcccess_GetProperty_Adjusts_For_TimeZone(string propertyName, string timeZoneId)
        {
            // Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timeZoneProfileProperty = new ProfilePropertyDefinition(Constants.PORTAL_Zero)
            {
                PropertyName = "PreferredTimeZone",
                PropertyValue = timeZoneId,
            };
            var userProfile = new UserProfile();
            userProfile.ProfileProperties.Add(timeZoneProfileProperty);

            var accessingUser = new UserInfo { Profile = userProfile };
            var culture = CultureInfo.InvariantCulture;

            string expected = string.Empty;

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

            // Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, string.Empty, culture,
                                                                   accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
            Assert.AreEqual(expected, propertyValue);
        }
    }
}
