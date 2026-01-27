// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using Cake.Common.Diagnostics;
    using Cake.Core.IO;
    using Cake.Frosting;

    using Dnn.CakeUtils;

    using Path = System.IO.Path;

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

        /// <inheritdoc />
        public override void Run(Context context)
        {
            context.Information("Computing packages checksums…");

            var checksumsMarkdown = new StringBuilder(
                """
                ## SHA256 Checksums
                | File       | Checksum |
                |------------|----------|
                """);

            var files = context.GetFilesByPatterns(context.ArtifactsFolder, ZipFiles);
            foreach (var file in files)
            {
                var fileName = file.GetFilename();
                var hash = GetFileHash(file);
                checksumsMarkdown.AppendLine(CultureInfo.InvariantCulture, $"| {fileName} | {hash}   |");
            }

            checksumsMarkdown.AppendLine();
            var filePath = Path.Combine(context.ArtifactsFolder, "checksums.md");
            File.WriteAllText(filePath, checksumsMarkdown.ToString());

            context.Information($"Saved checksums to {filePath}");
        }

        private static string GetFileHash(FilePath file)
        {
            using var hasher = SHA256.Create();
            using var stream = File.OpenRead(file.FullPath);
            var hashBytes = hasher.ComputeHash(stream);
            return Convert.ToHexStringLower(hashBytes);
        }
    }
}
