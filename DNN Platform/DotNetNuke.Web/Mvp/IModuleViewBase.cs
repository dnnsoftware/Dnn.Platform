﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvp
{
    using System;

    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.UI.Skins.Controls;
    using WebFormsMvp;

    /// <summary>Represents a class that is a view for a module in a Web Forms Model-View-Presenter application.</summary>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public partial interface IModuleViewBase : IView
    {
        event EventHandler Initialize;

        bool AutoDataBind { get; set; }

        void ProcessModuleLoadException(Exception ex);

        void ShowMessage(string messageHeader, string message, ModuleMessage.ModuleMessageType messageType);
    }
}
