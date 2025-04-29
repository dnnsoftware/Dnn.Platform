// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Communications;

using System;

public class ModuleCommunicationEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="ModuleCommunicationEventArgs"/> class.</summary>
    public ModuleCommunicationEventArgs()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ModuleCommunicationEventArgs"/> class.</summary>
    /// <param name="text"></param>
    public ModuleCommunicationEventArgs(string text)
    {
        this.Text = text;
    }

    /// <summary>Initializes a new instance of the <see cref="ModuleCommunicationEventArgs"/> class.</summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="sender"></param>
    /// <param name="target"></param>
    public ModuleCommunicationEventArgs(string type, object value, string sender, string target)
    {
        this.Type = type;
        this.Value = value;
        this.Sender = sender;
        this.Target = target;
    }

    public string Sender { get; set; }

    public string Target { get; set; }

    public string Text { get; set; }

    public string Type { get; set; }

    public object Value { get; set; }
}
