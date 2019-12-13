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
