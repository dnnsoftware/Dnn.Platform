// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Authentication
{
    using System;

    using DotNetNuke.Authentication.Facebook.Components;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Authentication.OAuth;
    using NUnit.Framework;

    [TestFixture]
    public class FacebookUserDataTests
    {
        private const string SampleUserJson = @"{
    ""id"": ""220439"",
    ""name"": ""Bret the Taylor"",
    ""first_name"": ""Bret"",
    ""last_name"": ""Taylor"",
    ""link"": ""https://www.facebook.com/btaylor"",
    ""username"": ""btaylor"",
    ""gender"": ""male"",
    ""locale"": ""en_US""
}";

        [Test]
        public void FacebookUserData_Populates_Inherited_Name_Properties_When_Deserialized()
        {
            // Act
            UserData sampleUser = Json.Deserialize<FacebookUserData>(SampleUserJson);

            // Assert
            Assert.AreEqual("Bret", sampleUser.FirstName, "Should correctly pull first name from first_name field, not by parsing name");
            Assert.AreEqual("Taylor", sampleUser.LastName, "Should correctly pull first name from first_name field, not by parsing name");
        }

        [Test]
        public void FacebookUserData_Populates_Link_Property_When_Deserialized()
        {
            // Act
            var sampleUser = Json.Deserialize<FacebookUserData>(SampleUserJson);

            // Assert
            Assert.AreEqual(new Uri("https://www.facebook.com/btaylor", UriKind.Absolute), sampleUser.Link);
        }
    }
}
