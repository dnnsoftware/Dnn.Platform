// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Common.Xml;
    using Cake.Frosting;
    using Cake.Json;

    using Dnn.CakeUtils;

    /// <summary>A cake task to include other 3rd party packages.</summary>
    [IsDependentOn(typeof(PackageNewtonsoft))]
    [IsDependentOn(typeof(PackageMailKit))]
    [IsDependentOn(typeof(PackageHtmlSanitizer))]
    [IsDependentOn(typeof(PackageAspNetWebApi))]
    [IsDependentOn(typeof(PackageAspNetWebPages))]
    [IsDependentOn(typeof(PackageAspNetMvc))]
    [IsDependentOn(typeof(PackageMicrosoftGlobbing))]
    [IsDependentOn(typeof(PackageWebFormsMvp))]
    [IsDependentOn(typeof(PackageSharpZipLib))]
    [IsDependentOn(typeof(PackageMicrosoftExtensionsDependencyInjection))]
    [IsDependentOn(typeof(PackageMicrosoftWebInfrastructure))]
    public sealed class OtherPackages : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            var otherPackages = context.DeserializeJsonFromFile<IEnumerable<OtherPackage>>("./Build/Tasks/thirdparty.json");
            foreach (var op in otherPackages)
            {
                PackageOtherPackage(context, op);
            }
        }

        private static void PackageOtherPackage(Context context, OtherPackage package)
        {
            var srcFolder = "./" + package.Folder;
            var files = package.Excludes.Length == 0
                            ? context.GetFiles(srcFolder + "**/*")
                            : context.GetFilesByPatterns(srcFolder, new[] { "**/*" }, package.Excludes);
            var version = "00.00.00";
            foreach (var dnn in context.GetFiles(srcFolder + "**/*.dnn"))
            {
                version = context.XmlPeek(dnn, "dotnetnuke/packages/package/@version");
            }

            context.CreateDirectory(package.Destination);

            var packageZip = $"{context.WebsiteFolder}{package.Destination}/{package.Name}_{version}_Install.{package.Extension}";
            context.Information("Packaging {0}", packageZip);
            context.Zip(srcFolder, packageZip, files);
        }

        private sealed class OtherPackage
        {
            public string Name { get; set; }

            public string Folder { get; set; }

            public string Destination { get; set; }

            public string Extension { get; set; } = "zip";

            public string[] Excludes { get; set; } = Array.Empty<string>();
        }
    }
}
