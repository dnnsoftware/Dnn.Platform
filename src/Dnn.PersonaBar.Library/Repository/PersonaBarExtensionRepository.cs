#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
