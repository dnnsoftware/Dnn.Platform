// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services;

using System.Net;
using System.Net.Http;
using System.Web.Http;

using Dnn.Modules.ResourceManager.Components;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;

/// <summary>Provides localization web services.</summary>
[DnnAuthorize]
[DnnExceptionFilter]
public class LocalizationController : DnnApiController
{
    private readonly ILocalizationController localizationController;

    /// <summary>Initializes a new instance of the <see cref="LocalizationController"/> class.</summary>
    public LocalizationController()
    {
        this.localizationController = Components.LocalizationController.Instance;
    }

    /// <summary>Gets a dictionary of the resource manager localization values.</summary>
    /// <returns>
    /// A dictionary of the keys and values where the key is the localization key
    /// and the value is the localized text for the requested culture.
    /// </returns>
    [HttpGet]
    [AllowAnonymous]
    [DnnPageEditor]
    public HttpResponseMessage GetResources()
    {
        var culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
        return this.Request.CreateResponse(
            HttpStatusCode.OK,
            this.localizationController.GetLocalizedDictionary(Constants.ViewResourceFileName, culture, Constants.ResourceManagerLocalization));
    }
}
