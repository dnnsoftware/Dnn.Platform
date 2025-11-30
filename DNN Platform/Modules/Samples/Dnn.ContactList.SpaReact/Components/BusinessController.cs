// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Modules;

namespace Dnn.ContactList.SpaReact.Components
{
    /// <summary>
    /// Business controller for the Contact list SPA module.
    /// This class needs to implement the interface ICustomTokenProvider in order to define custom tokens that
    /// will be managed by the Token Replace Engine.
    /// </summary>
    public class BusinessController : ICustomTokenProvider
    {
        /// <summary>
        /// Implementation of the interface ICustomTokenProvider
        /// </summary>
        /// <param name="page"></param>
        /// <param name="moduleContext"></param>
        /// <returns></returns>
        public IDictionary<string, IPropertyAccess> GetTokens(Page page, ModuleInstanceContext moduleContext)
        {
            var tokens = new Dictionary<string, IPropertyAccess>();
            tokens["context"] = new ContextTokens(moduleContext, UserController.Instance.GetCurrentUserInfo());

            return tokens;
        }
    }
}
