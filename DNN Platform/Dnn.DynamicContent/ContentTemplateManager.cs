// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Data;

namespace Dnn.DynamicContent
{
    public class ContentTemplateManager : ControllerBase<ContentTemplate, IContentTemplateManager, ContentTemplateManager>, IContentTemplateManager
    {
        internal const string ContentTemplateCacheKey = "ContentTypes_Templates";
        internal const string ContentTemplateScope = "ContentTypeId";

        protected override Func<IContentTemplateManager> GetFactory()
        {
            return () => new ContentTemplateManager();
        }

        public ContentTemplateManager() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public ContentTemplateManager(IDataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Adds a new content template for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="contentTemplate">The content template to add.</param>
        /// <returns>content template id.</returns>
        /// <exception cref="System.ArgumentNullException">content template is null.</exception>
        /// <exception cref="System.ArgumentException">contentTemplate.Name is empty.</exception>
        public int AddContentTemplate(ContentTemplate contentTemplate)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(contentTemplate, "Name");
            Requires.PropertyNotNegative(contentTemplate, "ContentTypeId");

            Add(contentTemplate);

            return contentTemplate.TemplateId;
        }

        /// <summary>
        /// Deletes the content template for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="contentTemplate">The content template to delete.</param>
        /// <exception cref="System.ArgumentNullException">content template is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content template id is less than 0.</exception>
        /// <exception cref="System.InvalidOperationException">contentTemplate is in use.</exception>
        public void DeleteContentTemplate(ContentTemplate contentTemplate)
        {
            Delete(contentTemplate);
        }

        /// <summary>
        /// Gets the content templates.
        /// </summary>
        /// <returns>content template collection.</returns>
        public IQueryable<ContentTemplate> GetContentTemplates(int contentTypeId)
        {
            return Get(contentTypeId).AsQueryable();
        }

        /// <summary>
        /// Updates the content template.
        /// </summary>
        /// <param name="contentTemplate">The content template.</param>
        /// <exception cref="System.ArgumentNullException">content template is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content template id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">contentTemplate.Name is empty.</exception>
        /// <exception cref="System.InvalidOperationException">contentTemplate is in use.</exception>
        public void UpdateContentTemplate(ContentTemplate contentTemplate)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(contentTemplate, "Name");
            Requires.PropertyNotNegative(contentTemplate, "ContentTypeId");

            Update(contentTemplate);
        }
    }
}
