﻿// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Dnn.DynamicContent.Localization;
using Dnn.Modules.DynamicContentManager.Components.Entities;
using DotNetNuke.Collections;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.DynamicContentManager.Services
{
    //TODO XML Comments
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseController : DnnApiController
    {
		/// <summary>
		/// Local Resource File Path.
		/// </summary>
        protected string LocalResourceFile = "~/DesktopModules/Dnn/DynamicContentManager/App_LocalResources/Manager.resx";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getEntity"></param>
        /// <param name="deleteEntity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        protected HttpResponseMessage DeleteEntity<TEntity>(Func<TEntity> getEntity, Action<TEntity> deleteEntity)
        {
            var entity = getEntity();

            if (entity != null)
            {
                deleteEntity(entity);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { });

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="getEntity"></param>
        /// <param name="getViewModel"></param>
        /// <returns></returns>
        protected HttpResponseMessage GetEntity<TEntity, TViewModel>(Func<TEntity> getEntity, Func<TEntity, TViewModel> getViewModel)
        {
            return Request.CreateResponse(getViewModel(getEntity()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="getEntities"></param>
        /// <param name="getViewModel"></param>
        /// <returns></returns>
        protected HttpResponseMessage GetPage<TEntity, TViewModel>(Func<IPagedList<TEntity>> getEntities, Func<TEntity,TViewModel> getViewModel )
        {
            var entityList = getEntities();
            var entities = entityList
                                .Select(entity => getViewModel(entity))
                                .ToList();

            var response = new
                            {
                                results = entities,
                                total = entityList.TotalCount
                            };

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizedFields"></param>
        /// <param name="localizations"></param>
        /// <param name="portalId"></param>
        /// <returns></returns>
        protected string ParseLocalizations(List<dynamic> localizedFields, List<ContentTypeLocalization> localizations, int portalId)
        {
            var defaultValue = String.Empty;
            foreach (var localizedName in localizedFields)
            {
                if (localizedName.code == PortalSettings.CultureCode)
                {
                    defaultValue = localizedName.value;
                }
                else
                {
                    localizations.Add(new ContentTypeLocalization
                                            {
                                                PortalId = portalId,
                                                CultureCode = localizedName.code,
                                                Value = localizedName.value
                                            });
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizations"></param>
        /// <param name="formattedKey"></param>
        /// <param name="keyId"></param>
        /// <param name="portalId"></param>
        protected void SaveContentLocalizations(List<ContentTypeLocalization> localizations, string formattedKey, int keyId, int portalId)
        {
            foreach (var localization in localizations)
            {
                if (!String.IsNullOrEmpty(localization.Value))
                {
                    localization.Key = String.Format(formattedKey, keyId);
                    var savedLocalization = ContentTypeLocalizationManager.Instance.GetLocalizations(portalId)
                                                .SingleOrDefault(l => l.CultureCode == localization.CultureCode && l.Key == localization.Key);

                    if (savedLocalization == null)
                    {
                        ContentTypeLocalizationManager.Instance.AddLocalization(localization);
                    }
                    else
                    {
                        localization.LocalizationId = savedLocalization.LocalizationId;
                        ContentTypeLocalizationManager.Instance.UpdateLocalization(localization);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="errorMessage"></param>
        /// <param name="createEntity"></param>
        /// <param name="addEntity"></param>
        /// <param name="getEntity"></param>
        /// <param name="updateEntity"></param>
        /// <param name="saveLocalizations"></param>
        /// <param name="checkEntity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        protected HttpResponseMessage SaveEntity<TEntity>(int id, Func<TEntity> checkEntity, string errorMessage, Func<TEntity> createEntity, Func<TEntity, int> addEntity, Func<TEntity> getEntity, Action<TEntity> updateEntity, Action<int> saveLocalizations)
        {
            bool isSuccess = true;
            if (id == -1)
            {
                if (checkEntity() != null)
                {
                    isSuccess = false;
                }
                else
                {
                    id = addEntity(createEntity());
                }
            }
            else
            {
                //Update
                var entity = getEntity();

                if (entity != null)
                {
                    updateEntity(entity);
                }
            }

            saveLocalizations(id);

            var response = (isSuccess) 
                                ? Request.CreateResponse(HttpStatusCode.OK, new { id })
                                : Request.CreateErrorResponse((HttpStatusCode) HttpStatusCodeAdditions.UnprocessableEntity, errorMessage);

            return response;
        }
    }
}