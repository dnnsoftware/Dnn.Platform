// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Common
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Utility Methods for Content.</summary>
    [DnnDeprecated(10, 2, 4, "Resolve types via Dependency Injection")]
    public static partial class Util
    {
        /// <summary>Gets the data service.</summary>
        /// <returns>data service instance from ComponentFactory.</returns>
        public static IDataService GetDataService()
        {
            var ds = ComponentFactory.GetComponent<IDataService>();
            if (ds == null)
            {
                ds = new DataService();
                ComponentFactory.RegisterComponentInstance<IDataService>(ds);
            }

            return ds;
        }

        /// <summary>Gets the content controller.</summary>
        /// <returns>ContentController from ComponentFactory.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial IContentController GetContentController()
            => GetContentController(null, null);

        /// <summary>Gets the content controller.</summary>
        /// <param name="dataService">The data service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>ContentController from ComponentFactory.</returns>
        public static IContentController GetContentController(IDataService dataService, IHostSettings hostSettings)
        {
            var ctl = ComponentFactory.GetComponent<IContentController>();
            if (ctl == null)
            {
                ctl = new ContentController(
                    dataService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDataService>(),
                    hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
                ComponentFactory.RegisterComponentInstance<IContentController>(ctl);
            }

            return ctl;
        }

        /// <summary>Gets the scope type controller.</summary>
        /// <returns>ScopeTypeController from ComponentFactory.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial IScopeTypeController GetScopeTypeController()
            => GetScopeTypeController(null, null);

        /// <summary>Gets the scope type controller.</summary>
        /// <param name="dataService">The data service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>ScopeTypeController from ComponentFactory.</returns>
        public static IScopeTypeController GetScopeTypeController(IDataService dataService, IHostSettings hostSettings)
        {
            var ctl = ComponentFactory.GetComponent<IScopeTypeController>();
            if (ctl == null)
            {
                ctl = new ScopeTypeController(
                    dataService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDataService>(),
                    hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
                ComponentFactory.RegisterComponentInstance<IScopeTypeController>(ctl);
            }

            return ctl;
        }

        /// <summary>Gets the term controller.</summary>
        /// <returns>TermController from ComponentFactory.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial ITermController GetTermController()
            => GetTermController(null, null);

        /// <summary>Gets the term controller.</summary>
        /// <param name="dataService">The data service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>TermController from ComponentFactory.</returns>
        public static ITermController GetTermController(IDataService dataService, IHostSettings hostSettings)
        {
            var ctl = ComponentFactory.GetComponent<ITermController>();
            if (ctl == null)
            {
                ctl = new TermController(
                    dataService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDataService>(),
                    hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
                ComponentFactory.RegisterComponentInstance<ITermController>(ctl);
            }

            return ctl;
        }

        /// <summary>Gets the vocabulary controller.</summary>
        /// <returns>VocabularyController from ComponentFactory.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial IVocabularyController GetVocabularyController()
            => GetVocabularyController(null, null);

        /// <summary>Gets the vocabulary controller.</summary>
        /// <param name="dataService">The data service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>VocabularyController from ComponentFactory.</returns>
        public static IVocabularyController GetVocabularyController(IDataService dataService, IHostSettings hostSettings)
        {
            var ctl = ComponentFactory.GetComponent<IVocabularyController>();
            if (ctl == null)
            {
                ctl = new VocabularyController(
                    dataService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDataService>(),
                    hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
                ComponentFactory.RegisterComponentInstance<IVocabularyController>(ctl);
            }

            return ctl;
        }
    }
}
