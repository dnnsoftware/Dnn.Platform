// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <inheritdoc cref="IRemoveItemFromCollectionStep" />
    internal sealed class RemoveItemFromCollectionStep : XmlStepBase, IRemoveItemFromCollectionStep
    {
        /// <summary>Initializes a new instance of the <see cref="RemoveItemFromCollectionStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        public RemoveItemFromCollectionStep(
            ILoggerSource loggerSource,
            ILocalizer localizer,
            IApplicationStatusInfo applicationStatusInfo)
            : base(loggerSource, localizer, applicationStatusInfo)
        {
        }

        /// <inheritdoc/>
        [Required]
        public string CollectionPath { get; set; }

        /// <inheritdoc/>
        [Required]
        public string AttributeNamesToIncludeInSearch { get; set; }

        /// <inheritdoc/>
        [Required]
        public string SearchTerm { get; set; }

        /// <inheritdoc/>
        protected override void ProcessXml(XmlDocument doc)
        {
            this.Success = true;

            var collection = doc.SelectSingleNode(this.CollectionPath);
            if (collection is null)
            {
                this.Notes = this.LocalizeFormat("UninstallStepSectionNotFound", this.CollectionPath);
                return;
            }

            var matchCount = 0;

            collection.SelectNodes("add|remove", this.NamespaceManager)
                .Cast<XmlElement>()
                .Select(e => new { Item = e, Text = this.JoinAttributesToIncludeInSearch(e) })
                .Where(x => x.Text.IndexOf(this.SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(x => x.Item)
                .ToList()
                .ForEach(item =>
                {
                    collection.RemoveChild(item);
                    matchCount++;
                });

            this.Notes = matchCount > 0
                ? this.LocalizeFormat("UninstallStepCountOfMatchesFound", matchCount)
                : this.Localize("UninstallStepNoMatchesFound");
        }

        private string JoinAttributesToIncludeInSearch(XmlElement element)
        {
            var attributeValues = this.AttributeNamesToIncludeInSearch.Split(',')
                .Select(attrName => element.GetAttribute(attrName.Trim()));

            return string.Join("|", attributeValues);
        }
    }
}
