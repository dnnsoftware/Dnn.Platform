// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Dnn.EditBar.Library.Items;

    public interface IEditBarController
    {
        IDictionary<string, object> GetConfigurations(int portalId);

        IList<BaseMenuItem> GetMenuItems();
    }
}
