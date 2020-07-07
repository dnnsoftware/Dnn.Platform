// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Executers
{
    public class PagesExecuter : WebApiExecuter
    {
        public PagesExecuter GetPageDetails(int pageId)
        {
            this.Responses.Add(this.Connector.GetContent(
                    "API/PersonaBar/Pages/GetPageDetails?pageId=" + pageId));

            return this;
        }

        public dynamic SavePageDetails(dynamic pageDetails)
        {
            this.Responses.Add(this.Connector.PostJson(
                "API/PersonaBar/Pages/SavePageDetails",
                pageDetails));
            return this.GetLastDeserializeResponseMessage();
        }
    }
}
