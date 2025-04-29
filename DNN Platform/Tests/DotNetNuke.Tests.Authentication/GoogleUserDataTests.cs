// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Authentication;

using DotNetNuke.Authentication.Google.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Authentication.OAuth;
using NUnit.Framework;

[TestFixture]
public class GoogleUserDataTests
{
    private const string SampleUserJson = @"{
 ""id"": ""108453011160062628399"",
 ""name"": ""Fred the Dinosaur"",
 ""given_name"": ""Frederick"",
 ""family_name"": ""Franklin"",
 ""link"": ""https://plus.google.com/108453011160062628399"",
 ""picture"": ""https://lh3.googleusercontent.com/-aii-uOqdr1M/AAAAAAAAAAI/AAAAAAAAADg/dNL75Dg7lbc/photo.jpg"",
 ""gender"": ""male"",
 ""locale"": ""en""
}";

    [Test]
    public void GoogleUserData_Populates_Inherited_Name_Properties_When_Deserialized()
    {
        // Act
        UserData dukesUser = Json.Deserialize<GoogleUserData>(SampleUserJson);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(dukesUser.FirstName, Is.EqualTo("Frederick"), "Should correctly pull first name from given_name field, not by parsing name");
            Assert.That(dukesUser.LastName, Is.EqualTo("Franklin"), "Should correctly pull first name from family_name field, not by parsing name");
        });
    }

    [Test]
    public void GoogleUserData_Populates_Inherited_ProfileImage_Property_When_Deserialized()
    {
        // Act
        UserData dukesUser = Json.Deserialize<GoogleUserData>(SampleUserJson);

        // Assert
        Assert.That(dukesUser.ProfileImage, Is.EqualTo("https://lh3.googleusercontent.com/-aii-uOqdr1M/AAAAAAAAAAI/AAAAAAAAADg/dNL75Dg7lbc/photo.jpg"));
    }
}
