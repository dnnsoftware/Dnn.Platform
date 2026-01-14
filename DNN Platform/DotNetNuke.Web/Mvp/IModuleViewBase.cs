// Licensed to the .NET Foundation under one or more agreements.
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
        /// <summary>An event which triggers when the module view initializes.</summary>
        event EventHandler Initialize;

        /// <summary>Gets or sets a value indicating whether to automatically data-bind the view.</summary>
        bool AutoDataBind { get; set; }

        /// <summary>Process an exception that occurs during module load.</summary>
        /// <param name="ex">The exception to process.</param>
        void ProcessModuleLoadException(Exception ex);

        /// <summary>Show a module message.</summary>
        /// <param name="messageHeader">The message header.</param>
        /// <param name="message">The message text.</param>
        /// <param name="messageType">The message type.</param>
        void ShowMessage(string messageHeader, string message, ModuleMessage.ModuleMessageType messageType);
    }
}
