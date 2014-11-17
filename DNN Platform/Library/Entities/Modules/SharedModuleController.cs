#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Runtime.InteropServices;
using DotNetNuke.Entities.Content;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Modules
{
    /// <summary>
    /// This class ...
    /// </summary>
    public class SharedModuleController : ServiceLocator<ISharedModuleController, SharedModuleController>, ISharedModuleController
    {
        #region Constants
        #endregion

        #region Members
        private readonly IContentController _contentController;
        #endregion

        #region Constructors

        public SharedModuleController()
        {
            _contentController = new ContentController();
        }
        public SharedModuleController( IContentController contentController)
        {
            _contentController = contentController;
        }
        #endregion

        #region Public Methods

        public bool IsSharedModule(ModuleInfo module)
        {
            var content = _contentController.GetContentItem(module.ContentItemId);
            return module.TabID != content.TabID;
        }

        public Exception GetModuleDoesNotBelongToPagException()
        {
            return new InvalidOperationException(Localization.GetExceptionMessage("ModuleDoesNotBelongToPage",
                "This module does not belong to the page. Please, move to its master page to change the module"));
        }
        #endregion

        #region Private Methods
        #endregion

        #region Service Locator
        protected override Func<ISharedModuleController> GetFactory()
        {
            return () => new SharedModuleController();
        }
        #endregion
    }
}
