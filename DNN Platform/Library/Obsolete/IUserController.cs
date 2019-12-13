using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DotNetNuke.Entities.Users.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use version in DotNetNuke.Entities.Users instead. Scheduled removal in v10.0.0.")]
    public interface IUserController
    {
        UserInfo GetUserByDisplayname(int portalId, string displayName);

        UserInfo GetUserById(int portalId, int userId);

        IList<UserInfo> GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int filterRoleId, int relationTypeId,
                                                    bool isAdmin, int pageIndex, int pageSize, string sortColumn,
                                                    bool sortAscending, string propertyNames, string propertyValues);

        IList<UserInfo> GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn,
                                                bool sortAscending, string propertyName, string propertyValue);
    }
}
