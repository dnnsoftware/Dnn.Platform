// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;

namespace Dnn.DynamicContent.Common
{
    public class DateUtilitiesManager : ServiceLocator<IDateUtilitiesManager, DateUtilitiesManager>, IDateUtilitiesManager
    {
        protected override Func<IDateUtilitiesManager> GetFactory()
        {
            return () => new DateUtilitiesManager();
        }

        public DateTime GetDatabaseTime()
        {
            return DateUtils.GetDatabaseTime();
        }
    }
}

