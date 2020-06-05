﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.HttpModules.Membership;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Common;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;

namespace DotNetNuke.Web.Mvc
{
    public class DnnMvcHandler : IHttpHandler, IRequiresSessionState
    {
        public DnnMvcHandler(RequestContext requestContext)
        {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }

            this.RequestContext = requestContext;
        }

        public static readonly string MvcVersionHeaderName = "X-AspNetMvc-Version";

        private ControllerBuilder _controllerBuilder;

        internal ControllerBuilder ControllerBuilder
        {
            get
            {
                if (this._controllerBuilder == null)
                {
                    this._controllerBuilder = ControllerBuilder.Current;
                }
                return this._controllerBuilder;
            }
            set { this._controllerBuilder = value; }
        }

        protected virtual bool IsReusable
        {
            get { return false; }
        }

        public static bool DisableMvcResponseHeader { get; set; }

        bool IHttpHandler.IsReusable
        {
            get { return this.IsReusable; }
        }

        void IHttpHandler.ProcessRequest(HttpContext httpContext)
        {
            MembershipModule.AuthenticateRequest(this.RequestContext.HttpContext, allowUnknownExtensions: true);
            this.ProcessRequest(httpContext);
        }

        public RequestContext RequestContext { get; private set; }

        protected virtual void ProcessRequest(HttpContext httpContext)
        {
            HttpContextBase httpContextBase = new HttpContextWrapper(httpContext);
            this.ProcessRequest(httpContextBase);
        }

        protected internal virtual void ProcessRequest(HttpContextBase httpContext)
        {
            try
            {
                var moduleExecutionEngine = this.GetModuleExecutionEngine();
                // Check if the controller supports IDnnController
                var moduleResult =
                    moduleExecutionEngine.ExecuteModule(this.GetModuleRequestContext(httpContext));
                httpContext.SetModuleRequestResult(moduleResult);
                this.RenderModule(moduleResult);
            }
            finally
            {
            }
        }

        #region DNN Mvc Methods

        private IModuleExecutionEngine GetModuleExecutionEngine()
        {
            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            if (moduleExecutionEngine == null)
            {
                moduleExecutionEngine = new ModuleExecutionEngine();
                ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(moduleExecutionEngine);
            }
            return moduleExecutionEngine;
        }

        private ModuleRequestContext GetModuleRequestContext(HttpContextBase httpContext)
        {
            var moduleInfo = httpContext.Request.FindModuleInfo();
            var moduleContext = new ModuleInstanceContext() {Configuration = moduleInfo };
            var desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(moduleInfo.DesktopModuleID, moduleInfo.PortalID);
            var moduleRequestContext = new ModuleRequestContext
            {
                HttpContext = httpContext,
                ModuleContext = moduleContext,
                ModuleApplication = new ModuleApplication(this.RequestContext, DisableMvcResponseHeader)
                {
                    ModuleName = desktopModule.ModuleName,
                    FolderPath = desktopModule.FolderName,
                },
            };

            return moduleRequestContext;
        }

        private void RenderModule(ModuleRequestResult moduleResult)
        {
            var writer = this.RequestContext.HttpContext.Response.Output;

            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            moduleExecutionEngine.ExecuteModuleResult(moduleResult, writer);
        }

        #endregion
    }
}
