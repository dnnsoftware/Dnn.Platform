#region Usings

using System;
using System.Collections.Generic;
using System.Data;

#endregion

namespace DotNetNuke.Entities.Users.Social.Data
{
    public interface IDataService
    {
        #region RelationshipType CRUD

        void DeleteRelationshipType(int relationshipTypeId);
        IDataReader GetAllRelationshipTypes();
        IDataReader GetRelationshipType(int relationshipTypeId);
        int SaveRelationshipType(RelationshipType relationshipType, int createUpdateUserId);

        #endregion

        #region Relationship CRUD

        void DeleteRelationship(int relationshipId);
        IDataReader GetRelationship(int relationshipId);
        IDataReader GetRelationshipsByUserId(int userId);
        IDataReader GetRelationshipsByPortalId(int portalId);
        int SaveRelationship(Relationship relationship, int createUpdateUserId);

        #endregion

        #region UserRelationship CRUD

        void DeleteUserRelationship(int userRelationshipId);
        IDataReader GetUserRelationship(int userRelationshipId);
        IDataReader GetUserRelationship(int userId, int relatedUserId, int relationshipId, RelationshipDirection relationshipDirection);
        IDataReader GetUserRelationships(int userId);
        IDataReader GetUserRelationshipsByRelationshipId(int relationshipId);
        int SaveUserRelationship(UserRelationship userRelationship, int createUpdateUserId);

        #endregion

        #region UserRelationshipPreference CRUD

        void DeleteUserRelationshipPreference(int preferenceId);
        IDataReader GetUserRelationshipPreferenceById(int preferenceId);
        IDataReader GetUserRelationshipPreference(int userId, int relationshipId);
        int SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference, int createUpdateUserId);

        #endregion
    }
}
