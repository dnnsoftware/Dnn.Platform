// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Authentication
{
    using DotNetNuke.Authentication.LiveConnect.Components;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Authentication.OAuth;
    using NUnit.Framework;

    [TestFixture]
    public class LiveUserDataTests
    {
        private const string SampleUserJson = @"{
  ""id"" : ""contact.c1678ab4000000000000000000000000"",
  ""first_name"" : ""Frederick"",
  ""last_name"" : ""Franklin"",
  ""name"" : ""Fred the Dinosaur"",
  ""gender"" : ""male"",
  ""locale"" : ""en_US""
} ";

        [Test]
        public void LiveUserData_Populates_Inherited_Name_Properties_When_Deserialized()
        {
            // Act
            UserData dukesUser = Json.Deserialize<LiveUserData>(SampleUserJson);

            // Assert
            Assert.AreEqual("Frederick", dukesUser.FirstName, "Should correctly pull first name from first_name field, not by parsing name");
            Assert.AreEqual("Franklin", dukesUser.LastName, "Should correctly pull first name from last_name field, not by parsing name");
        }
    }
}
