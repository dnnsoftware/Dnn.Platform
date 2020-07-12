// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvp
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.Web.Validators;
    using WebFormsMvp;

    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public abstract class ModulePresenterBase<TView> : Presenter<TView>
        where TView : class, IModuleViewBase
    {
        protected ModulePresenterBase(TView view)
            : base(view)
        {
            // Try and cast view to Control to get common control properties
            var control = view as Control;
            if (control != null && control.Page != null)
            {
                this.IsPostBack = control.Page.IsPostBack;
            }

            // Try and cast view to IModuleControl to get the Context
            var moduleControl = view as IModuleControl;
            if (moduleControl != null)
            {
                this.LocalResourceFile = moduleControl.LocalResourceFile;
                this.ModuleContext = moduleControl.ModuleContext;
            }

            this.Validator = new Validator(new DataAnnotationsObjectValidator());

            view.Initialize += this.InitializeInternal;
            view.Load += this.LoadInternal;
        }

        public bool AutoDataBind { get; set; }

        public ModuleInfo ModuleInfo { get; set; }

        public bool IsEditable { get; set; }

        public bool IsPostBack { get; set; }

        public bool IsSuperUser { get; set; }

        public string LocalResourceFile { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        public int ModuleId { get; set; }

        public int PortalId { get; set; }

        public int TabId { get; set; }

        public int UserId { get; set; }

        public Dictionary<string, string> Settings { get; set; }

        public Validator Validator { get; set; }

        protected internal virtual bool AllowAnonymousAccess
        {
            get
            {
                return true;
            }
        }

        protected internal virtual bool IsUserAuthorized
        {
            get
            {
                return true;
            }
        }

        public virtual void RestoreState(StateBag stateBag)
        {
            AttributeBasedViewStateSerializer.DeSerialize(this, stateBag);
        }

        public virtual void SaveState(StateBag stateBag)
        {
            AttributeBasedViewStateSerializer.Serialize(this, stateBag);
        }

        protected internal virtual bool CheckAuthPolicy()
        {
            if (this.UserId == Null.NullInteger && !this.AllowAnonymousAccess)
            {
                this.OnNoCurrentUser();
                return false;
            }

            if (!this.IsUserAuthorized)
            {
                this.OnUnauthorizedUser();
                return false;
            }

            return true;
        }

        protected virtual void LoadFromContext()
        {
            if (this.ModuleContext != null)
            {
                this.ModuleInfo = this.ModuleContext.Configuration;
                this.IsEditable = this.ModuleContext.IsEditable;
                this.IsSuperUser = this.ModuleContext.PortalSettings.UserInfo.IsSuperUser;
                this.ModuleId = this.ModuleContext.ModuleId;
                this.PortalId = this.ModuleContext.PortalId;
                this.Settings = new Dictionary<string, string>();
                foreach (object key in this.ModuleContext.Settings.Keys)
                {
                    this.Settings[key.ToString()] = (string)this.ModuleContext.Settings[key];
                }

                this.TabId = this.ModuleContext.TabId;
                this.UserId = this.ModuleContext.PortalSettings.UserInfo.UserID;
            }
        }

        protected virtual string LocalizeString(string key)
        {
            string localizedString;
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(this.LocalResourceFile))
            {
                localizedString = Localization.GetString(key, this.LocalResourceFile);
            }
            else
            {
                localizedString = Null.NullString;
            }

            return localizedString;
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnLoad()
        {
        }

        protected virtual void OnNoCurrentUser()
        {
            this.RedirectToLogin();
        }

        protected virtual void OnUnauthorizedUser()
        {
            this.RedirectToAccessDenied();
        }

        protected void RedirectToAccessDenied()
        {
            this.Response.Redirect(TestableGlobals.Instance.AccessDeniedURL(), true);
        }

        protected void RedirectToCurrentPage()
        {
            this.Response.Redirect(TestableGlobals.Instance.NavigateURL(), true);
        }

        protected void RedirectToLogin()
        {
            this.Response.Redirect(TestableGlobals.Instance.LoginURL(this.Request.RawUrl, false), true);
        }

        protected void ProcessModuleLoadException(Exception ex)
        {
            this.View.ProcessModuleLoadException(ex);
        }

        protected void ShowMessage(string messageHeader, string message, ModuleMessage.ModuleMessageType messageType)
        {
            this.ShowMessage(messageHeader, message, messageType, true);
        }

        protected void ShowMessage(string message, ModuleMessage.ModuleMessageType messageType)
        {
            this.ShowMessage(message, messageType, true);
        }

        protected void ShowMessage(string message, ModuleMessage.ModuleMessageType messageType, bool localize)
        {
            this.ShowMessage(string.Empty, message, messageType, localize);
        }

        protected void ShowMessage(string messageHeader, string message, ModuleMessage.ModuleMessageType messageType, bool localize)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (localize)
                {
                    messageHeader = this.LocalizeString(messageHeader);
                    message = this.LocalizeString(message);
                }

                this.View.ShowMessage(messageHeader, message, messageType);
            }
        }

        private void InitializeInternal(object sender, EventArgs e)
        {
            this.LoadFromContext();
            this.OnInit();
        }

        private void LoadInternal(object sender, EventArgs e)
        {
            if (this.CheckAuthPolicy())
            {
                this.OnLoad();
            }
        }
    }
}
