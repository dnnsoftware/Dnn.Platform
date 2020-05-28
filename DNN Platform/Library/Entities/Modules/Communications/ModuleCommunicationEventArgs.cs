// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

#endregion

namespace DotNetNuke.Entities.Modules.Communications
{
    public class ModuleCommunicationEventArgs : EventArgs
    {
        public ModuleCommunicationEventArgs()
        {
        }

        public ModuleCommunicationEventArgs(string text)
        {
            Text = text;
        }

        public ModuleCommunicationEventArgs(string type, object value, string sender, string target)
        {
            Type = type;
            Value = value;
            Sender = sender;
            Target = target;
        }

        public string Sender { get; set; }

        public string Target { get; set; }

        public string Text { get; set; }

        public string Type { get; set; }

        public object Value { get; set; }
    }
}
