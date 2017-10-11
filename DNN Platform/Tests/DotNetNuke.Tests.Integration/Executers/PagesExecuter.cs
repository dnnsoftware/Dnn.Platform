#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System.Collections.Generic;
using DotNetNuke.Tests.Integration.Executers.Dto;

namespace DotNetNuke.Tests.Integration.Executers
{
    public class PagesExecuter : WebApiExecuter
    {
        #region Page Setting API action methods

        public PagesExecuter GetPageDetails(int pageId)
        {
            Responses.Add(Connector.GetContent(
                    "API/PersonaBar/Pages/GetPageDetails?pageId=" + pageId));

            return this;
        }

        public dynamic SavePageDetails(dynamic pageDetails)
        {
            Responses.Add(Connector.PostJson("API/PersonaBar/Pages/SavePageDetails",
                pageDetails));
            return GetLastDeserializeResponseMessage();
        }

        #endregion
    }
}
