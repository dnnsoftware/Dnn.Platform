#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Services.FileSystem
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : FolderController
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Business Class that provides access to the Database for the functions within the calling classes
    /// Instantiates the instance of the DataProvider and returns the object, if any
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class FolderController
    {
        #region StorageLocationTypes enum

        public enum StorageLocationTypes
        {
            InsecureFileSystem = 0,
            SecureFileSystem = 1,
            DatabaseSecure = 2
        }

        #endregion
    }
}
