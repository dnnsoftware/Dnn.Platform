#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;

namespace DotNetNuke.Entities.Modules
{
    /// <summary>
    /// This interface ...
    /// </summary>
    public interface ISharedModuleController
    {
        bool IsSharedModule(ModuleInfo module);

        Exception GetModuleDoesNotBelongToPagException();

    }
}
