#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.DynamicContent
{
    public class ContentTemplateController : ControllerBase<ContentTemplate, IContentTemplateController, ContentTemplateController>, IContentTemplateController
    {
        internal const string ContentTemplateCacheKey = "ContentTypes_Templates";
        internal const string ContentTemplateScope = "ContentTypeId";

        protected override Func<IContentTemplateController> GetFactory()
        {
            return () => new ContentTemplateController();
        }

        public ContentTemplateController() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public ContentTemplateController(IDataContext dataContext) : base(dataContext) { }

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
