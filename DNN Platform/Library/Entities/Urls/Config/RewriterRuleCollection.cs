// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls.Config;

using System;

using DotNetNuke.Collections;

[Serializable]
public class RewriterRuleCollection : GenericCollectionBase<RewriterRule>
{
    public new void Add(RewriterRule r) => base.Add(r);
}
