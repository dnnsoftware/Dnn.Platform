// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Framework
{
    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should be used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IMvcServicesFramework
    {
        /// <summary>
        /// Requests that anti-forgery tokens be included in the current page for AJAX calls.
        /// </summary>
        void RequestAjaxAntiForgerySupport();

        /// <summary>
        /// Requests that the services framework AJAX scripts be included in the current page.
        /// </summary>
        void RequestAjaxScriptSupport();
    }
}
