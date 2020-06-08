// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Collections.Generic;
using Dnn.PersonaBar.Library.Model;

namespace Dnn.PersonaBar.Library.Controllers
{
    public interface IExtensionController
    {
        string GetPath(PersonaBarExtension extension);

        bool Visible(PersonaBarExtension extension);

        IDictionary<string, object> GetSettings(PersonaBarExtension extension);
    }
}
