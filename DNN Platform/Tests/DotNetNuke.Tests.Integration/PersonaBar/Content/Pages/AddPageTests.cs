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
using System.Collections.Generic;
using System.Linq;
using DNN.Integration.Test.Framework;
using DNN.Integration.Test.Framework.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.PersonaBar.Content.Pages
{
    [TestFixture]
    public class AddPageTests : IntegrationTestBase
    {
        private const string AddBulkPagesApi = "/API/PersonaBar/Pages/SaveBulkPages";
        private const string VerigyBulkPagesApi = "/API/PersonaBar/Pages/PreSaveBulkPagesValidate";

        [Test]
        public void Add_Multi_Pages_For_Exisitng_Shoul_Return_Results()
        {
            var rnd = new Random().Next(1000, 10000);
            var addPagesDto = new BulkPage
            {
                BulkPages = "Page_" + rnd,
                ParentId = -1,
                Keywords = "",
                Tags = "",
                IncludeInMenu = true,
                StartDate = null,
                EndDate = null
            };

            Console.WriteLine(@"Add bulk pages request = {0}", JsonConvert.SerializeObject(addPagesDto));
            var connector = WebApiTestHelper.LoginHost();
            var response = connector.PostJson(AddBulkPagesApi, addPagesDto).Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<BulkPageResponseWrapper>(response);
            Console.WriteLine(@"Add bulk pages ersponse = {0}", response);
            Assert.AreEqual(0, result.Status);
            Assert.IsNull(result.Response.Pages.First().ErrorMessage);

            var response2 = connector.PostJson(VerigyBulkPagesApi, addPagesDto).Content.ReadAsStringAsync().Result;
            var result2 = JsonConvert.DeserializeObject<BulkPageResponseWrapper>(response2);
            Console.WriteLine(@"Verify bulk pages ersponse = {0}", response2);
            Assert.AreEqual(0, int.Parse(result2.Status.ToString()));
            Assert.IsNotNullOrEmpty(result2.Response.Pages.First().ErrorMessage);
        }

        #region helper classes
        [JsonObject]
        public class BulkPage
        {
            public string BulkPages { get; set; }
            public int ParentId { get; set; }
            public string Keywords { get; set; }
            public string Tags { get; set; }
            public bool IncludeInMenu { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }

        [JsonObject]
        public class BulkPageResponseItem
        {
            public string PageName { get; set; }
            public int Status { get; set; }
            public int TabId { get; set; }
            public string ErrorMessage { get; set; }
        }

        [JsonObject]
        public class BulkPageResponse
        {
            public int OverallStatus { get; set; }
            public IEnumerable<BulkPageResponseItem> Pages { get; set; }
        }

        [JsonObject]
        public class BulkPageResponseWrapper
        {
            public int Status { get; set; }
            public BulkPageResponse Response { get; set; }
        }
        #endregion
    }
}