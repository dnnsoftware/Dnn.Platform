// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.UI.Controllers
{
    using System.Web.Mvc;

    public class PersonaBarContainerModel
    {
        public string PersonaBarSettings { get; internal set; }

        public string BuildNumber { get; internal set; }

        public string AppPath { get; internal set; }

        public bool Visible { get; internal set; }
    }
}
