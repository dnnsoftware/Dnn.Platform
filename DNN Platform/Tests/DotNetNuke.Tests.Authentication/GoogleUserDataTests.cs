#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using DotNetNuke.Authentication.Google.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Authentication.OAuth;

using NUnit.Framework;

namespace DotNetNuke.Tests.Authentication
{
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

            // Assert
            Assert.AreEqual("Frederick", dukesUser.FirstName, "Should correctly pull first name from given_name field, not by parsing name");
            Assert.AreEqual("Franklin", dukesUser.LastName, "Should correctly pull first name from family_name field, not by parsing name");
        }

        [Test]
        public void GoogleUserData_Populates_Inherited_ProfileImage_Property_When_Deserialized()
        {
            // Act
            UserData dukesUser = Json.Deserialize<GoogleUserData>(SampleUserJson);

            // Assert
            Assert.AreEqual("https://lh3.googleusercontent.com/-aii-uOqdr1M/AAAAAAAAAAI/AAAAAAAAADg/dNL75Dg7lbc/photo.jpg", dukesUser.ProfileImage);
        }
    }
}
