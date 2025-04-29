// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Repository;

using System;
using System.Collections.Generic;
using System.Linq;

using Dnn.PersonaBar.Library.Data;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;

public class PersonaBarExtensionRepository : ServiceLocator<IPersonaBarExtensionRepository, PersonaBarExtensionRepository>,
    IPersonaBarExtensionRepository
{
    private const string PersonaBarExtensionsCacheKey = "PersonaBarExtensions";
    private static readonly object ThreadLocker = new object();
    private readonly IDataService dataService = new DataService();

    /// <inheritdoc/>
    public void SaveExtension(PersonaBarExtension extension)
    {
        this.dataService.SavePersonaBarExtension(
            extension.Identifier,
            extension.MenuId,
            extension.FolderName,
            extension.Controller,
            extension.Container,
            extension.Path,
            extension.Order,
            extension.Enabled,
            UserController.Instance.GetCurrentUserInfo().UserID);

        this.ClearCache();
    }

    /// <inheritdoc/>
    public void DeleteExtension(PersonaBarExtension extension)
    {
        this.DeleteExtension(extension.Identifier);
    }

    /// <inheritdoc/>
    public void DeleteExtension(string identifier)
    {
        this.dataService.DeletePersonaBarExtension(identifier);

        this.ClearCache();
    }

    /// <inheritdoc/>
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
                    extensions = CBO.FillCollection<PersonaBarExtension>(this.dataService.GetPersonaBarExtensions())
                        .OrderBy(e => e.Order).ToList();

                    DataCache.SetCache(PersonaBarExtensionsCacheKey, extensions);
                }
            }
        }

        return extensions;
    }

    /// <inheritdoc/>
    public IList<PersonaBarExtension> GetExtensions(int menuId)
    {
        return this.GetExtensions().Where(t => t.MenuId == menuId).ToList();
    }

    /// <inheritdoc/>
    protected override Func<IPersonaBarExtensionRepository> GetFactory()
    {
        return () => new PersonaBarExtensionRepository();
    }

    private void ClearCache()
    {
        DataCache.RemoveCache(PersonaBarExtensionsCacheKey);
    }
}
