// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Communications
{
    using System;

    public class ModuleCommunicationEventArgs : EventArgs
    {
        public ModuleCommunicationEventArgs()
        {
        }

        public ModuleCommunicationEventArgs(string text)
        {
            this.Text = text;
        }

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
}
