/*
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2013
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
*/

using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Content;
using DotNetNuke.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetNuke.Tests.Services.Content
{
    /// <summary>
    /// Summary description for RatingTests
    /// </summary>
    [TestClass]
    public class RatingServiceTests
    {
        #region Private Constants

        private const int ARG_InValidContentItemId = -1;
        private const int ARG_ValidContentItemId = 1;

        private const int ARG_InValidUserId = -1;
        private const int ARG_ValidUserId = 1;

        private const float ARG_ValidRating = 0.5f;
        private const float ARG_RatingLessThanZero = -1.0f;
        private const float ARG_RatingGreaterThanOne = 1.5f;

        #endregion

        #region AddRating

        [TestMethod]
        [Description("The AddRating method should throw if the Rating is null.")]
        public void RatingService_AddRating_Throws_On_Null_Rating()
        {
            AutoTester.ArgumentNull<Rating>(marker => RatingService.AddRating(marker));
        }

        #endregion

        #region CreateRating

        [TestMethod]
        [Description("The CreateRating method should throw on invalid userId.")]
        public void RatingService_CreateRating_Throws_On_Invalid_UserId()
        {
            //userId should be >=0
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => RatingService.CreateRating(ARG_InValidUserId, ARG_ValidContentItemId, ARG_ValidRating));
        }

        [TestMethod]
        [Description("The CreateRating method should throw if there are invalid contentItemId.")]
        public void RatingService_CreateRating_Throws_On_Invalid_ContentItemId()
        {
            //contentItemIdId should be >=0
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => RatingService.CreateRating(ARG_ValidUserId, ARG_InValidContentItemId, ARG_ValidRating));
        }

        [TestMethod]
        [Description("The CreateRating method should throw if there are invalid rating.")]
        public void RatingService_CreateRating_Throws_On_Invalid_Rating()
        {
            //Rating should be >=0
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => RatingService.CreateRating(ARG_ValidUserId, ARG_ValidContentItemId, ARG_RatingLessThanZero));

            //Rating should be <=1
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => RatingService.CreateRating(ARG_ValidUserId, ARG_ValidContentItemId, ARG_RatingGreaterThanOne));
        }

        #endregion

        #region DeleteRating

        [TestMethod]
        [Description("The DeleteRating method should throw if the Rating is null.")]
        public void RatingService_DeleteRating_Throws_On_Null_Rating()
        {
            AutoTester.ArgumentNull<Rating>(marker => RatingService.DeleteRating(marker));
        }

        #endregion

        #region GetRating

        [TestMethod]
        [Description("The GetRating method should throw if the RatingId is invalid.")]
        public void RatingService_GetRating_Throws_On_Invalid_RatingId()
        {
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => RatingService.GetRating(Null.NullInteger));
        }

        #endregion

        #region GetRatings

        [TestMethod]
        [Description("The GetRatings method should throw if the ContentItemId is invalid.")]
        public void RatingService_GetRatings_Throws_On_Invalid_ContentItemId()
        {
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => RatingService.GetRatings(Null.NullInteger));
        }

        #endregion

        #region UpdateRating

        [TestMethod]
        [Description("The UpdateRating method should throw if the Rating is null.")]
        public void RatingService_UpdateRating_Throws_On_Null_Rating()
        {
            AutoTester.ArgumentNull<Rating>(marker => RatingService.UpdateRating(marker));
        }

        #endregion

    }
}
