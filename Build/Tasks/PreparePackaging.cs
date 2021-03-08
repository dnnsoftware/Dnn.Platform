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

    using Newtonsoft.Json;

    /// <summary>A cake task to prepare for packaging (by building the platform and copying files).</summary>
    [Dependency(typeof(CopyWebsite))]
    [Dependency(typeof(Build))]
    [Dependency(typeof(CopyWebConfig))]
    [Dependency(typeof(CopyWebsiteBinFolder))]
    public sealed class PreparePackaging : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            context.PackagingPatterns = JsonConvert.DeserializeObject<PackagingPatterns>(Utilities.ReadFile("./Build/Tasks/packaging.json"));

            // Various fixes
            context.CopyFile(
                "./DNN Platform/Components/DataAccessBlock/bin/Microsoft.ApplicationBlocks.Data.dll",
                context.WebsiteFolder + "bin/Microsoft.ApplicationBlocks.Data.dll");
            context.CopyFiles(
                "./DNN Platform/Components/Lucene.Net.Contrib/bin/Lucene.Net.Contrib.Analyzers.*",
                context.WebsiteFolder + "bin/");
            context.CopyFile(
                "./DNN Platform/Library/bin/PetaPoco.dll",
                context.WebsiteFolder + "bin/PetaPoco.dll");
        }
    }
}
