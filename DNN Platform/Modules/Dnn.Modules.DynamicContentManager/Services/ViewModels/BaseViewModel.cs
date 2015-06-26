// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Dnn.DynamicContent.Localization;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="formattedKey"></param>
        /// <param name="keyId"></param>
        /// <param name="portalId"></param>
        /// <param name="portalSettings"></param>
        /// <returns></returns>
        protected List<dynamic> GetLocalizedValues(string defaultValue, string formattedKey, int keyId, int portalId, PortalSettings portalSettings)
        {
            var localizedValues = new List<dynamic>();
            foreach (var locale in LocaleController.Instance.GetLocales(portalSettings.PortalId).Values)
            {
                dynamic language;
                if (locale.Code == portalSettings.DefaultLanguage)
                {
                    language = new { code = locale.Code, value = defaultValue };
                }
                else
                {
                    var key = String.Format(formattedKey, keyId);
                    language = new
                                {
                                    code = locale.Code,
                                    value = ContentTypeLocalizationManager.Instance.GetLocalizedValue(key, locale.Code, portalId)
                                };
                }
                localizedValues.Add(language);
            }

            return localizedValues;
        }
    }
}