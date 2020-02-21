// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;

#endregion

namespace DotNetNuke.Services.Personalization
{
    [Serializable]
    public class PersonalizationInfo
    {
        #region Private Members

        #endregion

        #region Public Properties

        public int UserId { get; set; }

        public int PortalId { get; set; }

        public bool IsModified { get; set; }

        public Hashtable Profile { get; set; }

        #endregion
    }
}
