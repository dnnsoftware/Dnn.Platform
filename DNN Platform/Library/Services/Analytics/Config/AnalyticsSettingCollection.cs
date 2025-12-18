// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics.Config;

using System;

using DotNetNuke.Collections;

[Serializable]
public class AnalyticsSettingCollection : GenericCollectionBase<AnalyticsSetting>
{
    public new void Add(AnalyticsSetting r) => base.Add(r);
}
