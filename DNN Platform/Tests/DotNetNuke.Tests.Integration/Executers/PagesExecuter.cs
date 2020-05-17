﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
