// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;

    /// <summary>
    /// Find and replace text in files.
    /// </summary>
    internal class ReplaceTextInFileStep : StepBase, IReplaceTextInFileStep
    {
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly IFileSystemProvider fileSystemProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceTextInFileStep"/> class.
        /// </summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        /// <param name="fileSystemProvider">An instance of <see cref="IFileSystemProvider"/>.</param>
        public ReplaceTextInFileStep(
            ILoggerSource loggerSource,
            IApplicationStatusInfo applicationStatusInfo,
            IFileSystemProvider fileSystemProvider)
            : base(loggerSource)
        {
            this.applicationStatusInfo = applicationStatusInfo ??
                throw new ArgumentNullException(nameof(applicationStatusInfo));

            this.fileSystemProvider = fileSystemProvider ??
                throw new ArgumentNullException(nameof(fileSystemProvider));
        }

        /// <inheritdoc/>
        [Required]
        public string RelativeFilePath { get; set; }

        /// <inheritdoc/>
        [Required]
        public string SearchPattern { get; set; }

        /// <inheritdoc/>
        public string Replacement { get; set; }

        /// <inheritdoc/>
        public int MatchCount { get; protected set; }

        /// <inheritdoc/>
        protected override void ExecuteInternal()
        {
            this.Success = true;

            var root = this.applicationStatusInfo.ApplicationMapPath + "\\";
            var combined = Path.Combine(root, this.RelativeFilePath);
            var fullPath = Path.GetFullPath(combined);
            var content = this.ReadFile(fullPath);
            var matches = Regex.Matches(content, this.SearchPattern, RegexOptions.Singleline);
            this.MatchCount = matches.Count;

            if (this.MatchCount > 0)
            {
                this.Notes = $"Found {this.MatchCount} matches.";

                var replaced = Regex.Replace(content, this.SearchPattern, this.Replacement, RegexOptions.Singleline);
                this.WriteFile(fullPath, replaced);
            }
            else
            {
                this.Notes = "No matches found.";
            }
        }

        private string ReadFile(string path)
        {
            using (var stream = this.fileSystemProvider.CreateFileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private void WriteFile(string path, string content)
        {
            using (var stream = this.fileSystemProvider.CreateFileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content);
            }
        }
    }
}
