// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using Cake.Common.Diagnostics;
    using Cake.Frosting;

    using Dnn.CakeUtils;

    /// <summary>A cake task to generate a <c>checksums.md</c> file with the artifact checksums.</summary>
    [IsDependentOn(typeof(CleanArtifacts))]
    [IsDependentOn(typeof(UpdateDnnManifests))]
    [IsDependentOn(typeof(CreateInstall))]
    [IsDependentOn(typeof(CreateUpgrade))]
    [IsDependentOn(typeof(CreateDeploy))]
    [IsDependentOn(typeof(CreateSymbols))]
    public sealed class GeneratePackagesChecksums : FrostingTask<Context>
    {
        private static readonly string[] ZipFiles = ["*.zip",];

        /// <inheritdoc/>
        public override void Run(Context context)
        {
            context.Information("Computing packages checksums...");

            var sb = new StringBuilder();
            sb.AppendLine($"## MD5 Checksums")
                .AppendLine($"| File       | Checksum |")
                .AppendLine($"|------------|----------|");

            var files = context.GetFilesByPatterns(context.ArtifactsFolder, ZipFiles);
            foreach (var file in files)
            {
                string hash;
                var fileName = file.GetFilename();
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(file.FullPath))
                    {
                        var hashBytes = md5.ComputeHash(stream);
                        hash = BitConverter.ToString(hashBytes)
                            .Replace("-", string.Empty)
                            .ToLowerInvariant();
                    }
                }

                sb.AppendLine($"| {fileName} | {hash}   |");
            }

            sb.AppendLine();
            var filePath = Path.Combine(context.ArtifactsFolder, "checksums.md");
            File.WriteAllText(filePath, sb.ToString());

            context.Information($"Saved checksums to {filePath}");
        }
    }
}
