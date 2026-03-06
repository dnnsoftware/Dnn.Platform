// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemoval.UserControls
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Web.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A simple wrapper around <see cref="DnnButton"/> to implement <see cref="ITextControl"/>.</summary>
    public class DnnTextButton : DnnButton, ITextControl
    {
        /// <summary>Initializes a new instance of the <see cref="DnnTextButton"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnTextButton()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnTextButton"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnTextButton(IClientResourceController clientResourceController)
            : base(clientResourceController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IClientResourceController>())
        {
        }
    }
}
