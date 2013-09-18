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

using DotNetNuke.Authentication.Twitter.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Authentication.OAuth;

using NUnit.Framework;

namespace DotNetNuke.Tests.Content
{
    [TestFixture]
    public class TwitterUserDataTests
    {
        private const string SampleUserJson = @"{
      ""name"": ""Matt Harris"",
      ""profile_sidebar_border_color"": ""C0DEED"",
      ""profile_background_tile"": false,
      ""profile_sidebar_fill_color"": ""DDEEF6"",
      ""location"": ""San Francisco"",
      ""profile_image_url"": ""http://a1.twimg.com/profile_images/554181350/matt_normal.jpg"",
      ""created_at"": ""Sat Feb 17 20:49:54 +0000 2007"",
      ""profile_link_color"": ""0084B4"",
      ""favourites_count"": 95,
      ""url"": ""http://themattharris.com"",
      ""contributors_enabled"": false,
      ""utc_offset"": -28800,
      ""id"": 777925,
      ""profile_use_background_image"": true,
      ""profile_text_color"": ""333333"",
      ""protected"": false,
      ""followers_count"": 1025,
      ""lang"": ""en"",
      ""verified"": false,
      ""profile_background_color"": ""C0DEED"",
      ""geo_enabled"": true,
      ""notifications"": false,
      ""description"": ""Developer Advocate at Twitter. Also a hacker and British expat who is married to @cindyli and lives in San Francisco."",
      ""time_zone"": ""Tijuana"",
      ""friends_count"": 294,
      ""statuses_count"": 2924,
      ""profile_background_image_url"": ""http://s.twimg.com/a/1276711174/images/themes/theme1/bg.png"",
      ""status"": {
        ""coordinates"": {
          ""coordinates"": [
            -122.40075845,
            37.78264991
          ],
          ""type"": ""Point""
        },
        ""favorited"": false,
        ""created_at"": ""Tue Jun 22 18:17:48 +0000 2010"",
        ""truncated"": false,
        ""text"": ""Going through and updating @twitterapi documentation"",
        ""contributors"": null,
        ""id"": 16789004997,
        ""geo"": {
          ""coordinates"": [
            37.78264991,
            -122.40075845
          ],
          ""type"": ""Point""
        },
        ""in_reply_to_user_id"": null,
        ""place"": null,
        ""source"": ""<a href=\""http://itunes.apple.com/app/twitter/id333903271?mt=8\"" rel=\""nofollow\"">Twitter for iPhone</a>"",
        ""in_reply_to_screen_name"": null,
        ""in_reply_to_status_id"": null
      },
      ""screen_name"": ""themattharris"",
      ""following"": false
    }";

        [Test]
        public void TwitterUserData_Populates_Inherited_DisplayName_Property_When_Deserialized()
        {
            // Act
            UserData sampleUser = Json.Deserialize<TwitterUserData>(SampleUserJson);

            // Assert
            Assert.AreEqual("themattharris", sampleUser.DisplayName);
        }

        [Test]
        public void TwitterUserData_Populates_Inherited_Locale_Property_When_Deserialized()
        {
            // Act
            UserData sampleUser = Json.Deserialize<TwitterUserData>(SampleUserJson);

            // Assert
            Assert.AreEqual("en", sampleUser.Locale);
        }

        [Test]
        public void TwitterUserData_Populates_Inherited_ProfileImage_Property_When_Deserialized()
        {
            // Act
            UserData sampleUser = Json.Deserialize<TwitterUserData>(SampleUserJson);

            // Assert
            Assert.AreEqual("http://a1.twimg.com/profile_images/554181350/matt_normal.jpg", sampleUser.ProfileImage);
        }

        [Test]
        public void TwitterUserData_Populates_Inherited_Website_Property_When_Deserialized()
        {
            // Act
            UserData sampleUser = Json.Deserialize<TwitterUserData>(SampleUserJson);

            // Assert
            Assert.AreEqual("http://themattharris.com", sampleUser.Website);
        }
    }
}
