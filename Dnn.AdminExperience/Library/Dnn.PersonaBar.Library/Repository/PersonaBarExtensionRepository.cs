// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Data;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;

namespace Dnn.PersonaBar.Library.Repository
{
    public class PersonaBarExtensionRepository : ServiceLocator<IPersonaBarExtensionRepository, PersonaBarExtensionRepository>,
        IPersonaBarExtensionRepository
    {
        #region Fields
        private readonly IDataService _dataService = new DataService();
        private const string PersonaBarExtensionsCacheKey = "PersonaBarExtensions";
        private static readonly object ThreadLocker = new object();
        #endregion

        private void ClearCache()
        {
            DataCache.RemoveCache(PersonaBarExtensionsCacheKey);
        }

        protected override Func<IPersonaBarExtensionRepository> GetFactory()
        {
            return () => new PersonaBarExtensionRepository();
        }

        public void SaveExtension(PersonaBarExtension extension)
        {
            _dataService.SavePersonaBarExtension(
                extension.Identifier,
                extension.MenuId,
                extension.FolderName,
                extension.Controller,
                extension.Container,
                extension.Path,
                extension.Order,
                extension.Enabled,
                UserController.Instance.GetCurrentUserInfo().UserID
                );

            ClearCache();
        }

        public void DeleteExtension(PersonaBarExtension extension)
        {
            DeleteExtension(extension.Identifier);
        }

        public void DeleteExtension(string identifier)
        {
            _dataService.DeletePersonaBarExtension(identifier);

            ClearCache();
        }

        public IList<PersonaBarExtension> GetExtensions()
        {
            var extensions = DataCache.GetCache<IList<PersonaBarExtension>>(PersonaBarExtensionsCacheKey);
            if (extensions == null)
            {
                lock (ThreadLocker)
                {
                    extensions = DataCache.GetCache<IList<PersonaBarExtension>>(PersonaBarExtensionsCacheKey);
                    if (extensions == null)
                    {
                        extensions = CBO.FillCollection<PersonaBarExtension>(_dataService.GetPersonaBarExtensions())
                            .OrderBy(e => e.Order).ToList();

                        DataCache.SetCache(PersonaBarExtensionsCacheKey, extensions);
                    }
                }
            }

            return extensions;
        }

        public IList<PersonaBarExtension> GetExtensions(int menuId)
        {
            return GetExtensions().Where(t => t.MenuId == menuId).ToList();
        }
    }
}
