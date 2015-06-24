// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;

namespace Dnn.DynamicContent.Localization
{
    public interface IContentTypeLocalizationManager
    {
        int AddLocalization(ContentTypeLocalization item);

        void DeleteLocalization(ContentTypeLocalization item);

        string GetLocalizedValue(string key, string cultureCode, int portalId);

        IQueryable<ContentTypeLocalization> GetLocalizations(int portalId);

        void UpdateLocalization(ContentTypeLocalization item);
    }
}
