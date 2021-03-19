// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;

    using Cake.Common.IO;
    using Cake.Frosting;

    using Dnn.CakeUtils;

    /// <summary>A cake task to generate the Newtonsoft.Json package.</summary>
    public sealed class PackageNewtonsoft : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            var version = "00.00.00";
            foreach (var assy in context.GetFiles(context.WebsiteFolder + "bin/Newtonsoft.Json.dll"))
            {
                version = System.Diagnostics.FileVersionInfo.GetVersionInfo(assy.FullPath).FileVersion;
            }

            var packageZip = $"{context.WebsiteFolder}Install/Module/Newtonsoft.Json_{version}_Install.zip";
            context.Zip(
                "./DNN Platform/Components/Newtonsoft",
                packageZip,
                context.GetFiles("./DNN Platform/Components/Newtonsoft/*"));
            context.AddFilesToZip(
                packageZip,
                "Website",
                context.GetFiles(context.WebsiteFolder + "bin/Newtonsoft.Json.dll"),
                true);
        }
    }
}
