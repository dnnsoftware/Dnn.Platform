// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.EventListeners
{
    public class SkinEventListener
    {
        public SkinEventListener(SkinEventType type, SkinEventHandler e)
        {
            this.EventType = type;
            this.SkinEvent = e;
        }

        public SkinEventType EventType { get; private set; }

        public SkinEventHandler SkinEvent { get; private set; }
    }
}
