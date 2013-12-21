using System;
namespace DotNetNuke.Entities
{
    interface IBaseEntityInfo
    {
        DotNetNuke.Entities.Users.UserInfo CreatedByUser(int portalId);
        int CreatedByUserID { get; }
        DateTime CreatedOnDate { get; }
        DotNetNuke.Entities.Users.UserInfo LastModifiedByUser(int portalId);
        int LastModifiedByUserID { get; }
        DateTime LastModifiedOnDate { get; }
    }
}
