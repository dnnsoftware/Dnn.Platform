// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvp
{
    using System;
    using System.Globalization;

    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins.Controls;
    using WebFormsMvp.Web;

    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public abstract class ModuleViewBase : ModuleUserControlBase, IModuleViewBase
    {
        protected ModuleViewBase()
        {
            this.AutoDataBind = true;
        }

        public event EventHandler Initialize;

        public new event EventHandler Load;

        public bool ThrowExceptionIfNoPresenterBound
        {
            get { return true; }
        }

        public bool AutoDataBind { get; set; }

        public void ProcessModuleLoadException(Exception ex)
        {
            Exceptions.ProcessModuleLoadException(this, ex);
        }

        public void ShowMessage(string messageHeader, string message, ModuleMessage.ModuleMessageType messageType)
        {
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, messageHeader, message, messageType);
        }

        protected T DataItem<T>()
            where T : class, new()
        {
            var _T = this.Page.GetDataItem() as T ?? new T();
            return _T;
        }

        protected T DataValue<T>()
        {
            return (T)this.Page.GetDataItem();
        }

        protected string DataValue<T>(string format)
        {
            return string.Format(CultureInfo.CurrentCulture, format, this.DataValue<T>());
        }

        protected override void OnInit(EventArgs e)
        {
            PageViewHost.Register(this, this.Context, false);

            base.OnInit(e);

            this.Page.InitComplete += this.PageInitComplete;
            this.Page.PreRenderComplete += this.PagePreRenderComplete;
            this.Page.Load += this.PageLoad;
        }

        private void PageInitComplete(object sender, EventArgs e)
        {
            if (this.Initialize != null)
            {
                this.Initialize(this, EventArgs.Empty);
            }
        }

        private void PageLoad(object sender, EventArgs e)
        {
            if (this.Load != null)
            {
                this.Load(this, e);
            }
        }

        private void PagePreRenderComplete(object sender, EventArgs e)
        {
            // This event is raised after any async page tasks have completed, so it
            // is safe to data-bind
            if (this.AutoDataBind)
            {
                this.DataBind();
            }
        }
    }
}
