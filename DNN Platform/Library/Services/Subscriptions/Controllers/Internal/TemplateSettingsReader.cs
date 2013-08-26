#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using DotNetNuke.Framework;

namespace DotNetNuke.Subscriptions.Components.Controllers.Internal
{
    public class TemplateSettingsReader : ServiceLocator<ITemplateSettingsReader, TemplateSettingsReader>
    {
        #region Implementation of ServiceLocator

        protected override Func<ITemplateSettingsReader> GetFactory()
        {
            return () => new TemplateSettingsReaderImpl();
        }

        #endregion
    }
}