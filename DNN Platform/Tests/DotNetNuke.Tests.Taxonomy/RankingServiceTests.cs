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
    /// Summary description for RankingTests
    /// </summary>
    [TestClass]
    public class RankingServiceTests
    {

        #region AddRanking

        [TestMethod]
        [Description("The AddRanking method should throw if the Ranking is null.")]
        public void RankingService_AddRanking_Throws_On_Null_Ranking()
        {
            AutoTester.ArgumentNull<Ranking>(marker => RankingService.AddRanking(marker));
        }

        #endregion

        #region DeleteRanking

        [TestMethod]
        [Description("The DeleteRanking method should throw if the Ranking is null.")]
        public void RankingService_DeleteRanking_Throws_On_Null_Ranking()
        {
            AutoTester.ArgumentNull<Ranking>(marker => RankingService.DeleteRanking(marker));
        }

        #endregion

        #region GetRanking

        [TestMethod]
        [Description("The GetRanking method should throw if the RankingId is invalid.")]
        public void RankingService_GetRanking_Throws_On_Invalid_RankingId()
        {
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => RankingService.GetRanking(Null.NullInteger));
        }

        #endregion

        #region GetRankings

        [TestMethod]
        [Description("The GetRankings method should throw if the ContentItemId is invalid.")]
        public void RankingService_GetRankings_Throws_On_Invalid_ContentItemId()
        {
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => RankingService.GetRankings(Null.NullInteger));
        }

        #endregion

        #region UpdateRanking

        [TestMethod]
        [Description("The UpdateRanking method should throw if the Ranking is null.")]
        public void RankingService_UpdateRanking_Throws_On_Null_Ranking()
        {
            AutoTester.ArgumentNull<Ranking>(marker => RankingService.UpdateRanking(marker));
        }

        #endregion

    }
}
