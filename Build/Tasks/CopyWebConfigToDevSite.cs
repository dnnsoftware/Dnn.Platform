// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Common.IO;
using Cake.Common.Xml;
using Cake.FileHelpers;
using Cake.Frosting;

/// <summary>A cake task to copy the <c>web.config</c> file to the local dev site with appropriate transformations.</summary>
public sealed class CopyWebConfigToDevSite : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        if (string.IsNullOrWhiteSpace(context.Settings.DnnConnectionString))
        {
            throw new InvalidOperationException("DnnConnectionString was blank and must have a value when the CopyWebConfigToDevSite task is run");
        }

        var conf = context.FileReadText("./Website/web.config");
        var transFile = "./Build/Tasks/webconfig-transform.local.xsl";
        if (!context.FileExists(transFile))
        {
            transFile = "./Build/Tasks/webconfig-transform.xsl";
        }

        var trans = context.FileReadText(transFile);
        trans = trans.Replace("{ConnectionString}", context.Settings.DnnConnectionString)
            .Replace("{DbOwner}", context.Settings.DbOwner)
            .Replace("{ObjectQualifier}", context.Settings.ObjectQualifier);
        var res = context.XmlTransform(trans, conf);
        var webConfig = context.File(System.IO.Path.Combine(context.Settings.WebsitePath, "web.config"));
        context.FileWriteText(webConfig, res);
    }
}
