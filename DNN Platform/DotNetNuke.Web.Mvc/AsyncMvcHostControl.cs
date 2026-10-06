// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Routing;

    public class AsyncMvcHostControl : MvcHostControl, IAsyncModuleControl
    {
        public AsyncMvcHostControl()
            : base()
        {
        }

        public AsyncMvcHostControl(string controlKey)
            : base(controlKey)
        {
        }

        protected override void OnInitInternal(EventArgs e)
        {
            if (this.ExecuteModuleImmediately)
            {
                this.Page.RegisterAsyncTask(new PageAsyncTask(this.ExecuteModuleAsync));
            }
        }

        protected override void OnPreRenderInternal(EventArgs e)
        {
            // We need to defer execution to after the async task registered in OnInitInternal above, which will only get executed at the WebForms async point, just before PreRenderComplete.
            this.Page.RegisterAsyncTask(new PageAsyncTask(this.OnPreRenderAsync));
        }

        protected async Task ExecuteModuleAsync(CancellationToken cancellationToken)
        {
            try
            {
                HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

                var moduleExecutionEngine = GetModuleExecutionEngine();

                this.Result = await moduleExecutionEngine.ExecuteModuleAsync(this.GetModuleRequestContext(httpContext), cancellationToken);

                this.ModuleActions = this.LoadActions(this.Result);

                httpContext.SetModuleRequestResult(this.Result);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private Task OnPreRenderAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (this.Result == null)
                {
                    return Task.CompletedTask;
                }

                var mvcString = RenderModule(this.Result);
                if (!string.IsNullOrEmpty(Convert.ToString(mvcString, CultureInfo.InvariantCulture)))
                {
                    this.Controls.Add(new LiteralControl(Convert.ToString(mvcString, CultureInfo.InvariantCulture)));
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            return Task.CompletedTask;
        }
    }
}
