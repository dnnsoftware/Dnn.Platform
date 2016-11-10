// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Data;
using Dnn.PersonaBar.Library.PersonaBar.Model;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Library.PersonaBar.Repository
{
    public class PersonaBarExtensionRepository : ServiceLocator<IPersonaBarExtensionRepository, PersonaBarExtensionRepository>,
        IPersonaBarExtensionRepository
    {
        #region Fields

        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(PersonaBarExtensionRepository));

        private IDataService _dataService = new DataService();
        private const string PersonaBarExtensionsCacheKey = "PersonaBarExtensions";
        private static object _threadLocker = new object();

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
                lock (_threadLocker)
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
