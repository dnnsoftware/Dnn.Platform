// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Common
{
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Content.Taxonomy;

    /// <summary>
    /// Utility Methods for Content.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Gets the data service.
        /// </summary>
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

        /// <summary>
        /// Gets the content controller.
        /// </summary>
        /// <returns>ContentController from ComponentFactory.</returns>
        public static IContentController GetContentController()
        {
            var ctl = ComponentFactory.GetComponent<IContentController>();

            if (ctl == null)
            {
                ctl = new ContentController();
                ComponentFactory.RegisterComponentInstance<IContentController>(ctl);
            }

            return ctl;
        }

        /// <summary>
        /// Gets the scope type controller.
        /// </summary>
        /// <returns>ScopeTypeController from ComponentFactory.</returns>
        public static IScopeTypeController GetScopeTypeController()
        {
            var ctl = ComponentFactory.GetComponent<IScopeTypeController>();

            if (ctl == null)
            {
                ctl = new ScopeTypeController();
                ComponentFactory.RegisterComponentInstance<IScopeTypeController>(ctl);
            }

            return ctl;
        }

        /// <summary>
        /// Gets the term controller.
        /// </summary>
        /// <returns>TermController from ComponentFactory.</returns>
        public static ITermController GetTermController()
        {
            var ctl = ComponentFactory.GetComponent<ITermController>();

            if (ctl == null)
            {
                ctl = new TermController();
                ComponentFactory.RegisterComponentInstance<ITermController>(ctl);
            }

            return ctl;
        }

        /// <summary>
        /// Gets the vocabulary controller.
        /// </summary>
        /// <returns>VocabularyController from ComponentFactory.</returns>
        public static IVocabularyController GetVocabularyController()
        {
            var ctl = ComponentFactory.GetComponent<IVocabularyController>();

            if (ctl == null)
            {
                ctl = new VocabularyController();
                ComponentFactory.RegisterComponentInstance<IVocabularyController>(ctl);
            }

            return ctl;
        }
    }
}
