// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
