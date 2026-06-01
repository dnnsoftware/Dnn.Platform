// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Common.Security;
    using Cake.Core.IO;
    using Cake.FileHelpers;
    using Cake.Frosting;

    /// <summary>A cake task to generate the <c>Default.aspx</c> checksum for the Security Analyzer.</summary>
    [IsDependentOn(typeof(SetVersion))]
    public sealed class GenerateSecurityAnalyzerChecksums : FrostingTask<Context>
    {
        /// <inheritdoc />
        public override void Run(Context context)
        {
            context.Information("Generating default.aspx checksum…");
            var sourceFile = context.File("./Dnn Platform/Website/Default.aspx");
            var destFile = context.File("./Dnn.AdminExperience/Dnn.PersonaBar.Extensions/Components/Security/Resources/sums.resources");
            var hash = CalculateSha(context, sourceFile);
            var content = $"""
                           <checksums>
                             <sum name="Default.aspx" version="{context.Version.MajorMinorPatch}" type="Platform" sum="{hash}" />
                           </checksums>
                           """;
            context.FileWriteText(destFile, content);
        }

        private static string CalculateSha(Context context, FilePath file)
        {
            var hash = context.CalculateFileHash(file, HashAlgorithm.SHA256);
            return Convert.ToHexStringLower(hash.ComputedHash);
        }
    }
}
