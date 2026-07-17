// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// Marker interface for MVC controllers participating in the DNN MVC pipeline.
    /// </summary>
    public interface IMvcController : IController
    {
    }
}
