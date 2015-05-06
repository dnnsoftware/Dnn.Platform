// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;

namespace Dnn.DynamicContent
{
    public interface IContentTemplateManager
    {
        /// <summary>
        /// Adds a new content template for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="contentTemplate">The content template to add.</param>
        /// <returns>content template id.</returns>
        /// <exception cref="System.ArgumentNullException">content template is null.</exception>
        /// <exception cref="System.ArgumentException">contentTemplate.Name is empty.</exception>
        int AddContentTemplate(ContentTemplate contentTemplate);

        /// <summary>
        /// Deletes the content template for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="contentTemplate">The content template to delete.</param>
        /// <exception cref="System.ArgumentNullException">content template is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content template id is less than 0.</exception>
        /// <exception cref="System.InvalidOperationException">contentTemplate is in use.</exception>
        void DeleteContentTemplate(ContentTemplate contentTemplate);

        /// <summary>
        /// Gets the content templates.
        /// </summary>
        /// <param name="contentTypeId">The Id of the content type which this template is for</param>
        /// <returns>content template collection.</returns>
        IQueryable<ContentTemplate> GetContentTemplates(int contentTypeId);

        /// <summary>
        /// Updates the content template.
        /// </summary>
        /// <param name="contentTemplate">The content template.</param>
        /// <exception cref="System.ArgumentNullException">content template is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content template id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">contentTemplate.Name is empty.</exception>
        /// <exception cref="System.InvalidOperationException">contentTemplate is in use.</exception>
        void UpdateContentTemplate(ContentTemplate contentTemplate);
    }
}
