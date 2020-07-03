// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    public interface IDataService
    {
        void DeleteRelationshipType(int relationshipTypeId);

        IDataReader GetAllRelationshipTypes();

        IDataReader GetRelationshipType(int relationshipTypeId);

        int SaveRelationshipType(RelationshipType relationshipType, int createUpdateUserId);

        void DeleteRelationship(int relationshipId);

        IDataReader GetRelationship(int relationshipId);

        IDataReader GetRelationshipsByUserId(int userId);

        IDataReader GetRelationshipsByPortalId(int portalId);

        int SaveRelationship(Relationship relationship, int createUpdateUserId);

        void DeleteUserRelationship(int userRelationshipId);

        IDataReader GetUserRelationship(int userRelationshipId);

        IDataReader GetUserRelationship(int userId, int relatedUserId, int relationshipId, RelationshipDirection relationshipDirection);

        IDataReader GetUserRelationships(int userId);

        IDataReader GetUserRelationshipsByRelationshipId(int relationshipId);

        int SaveUserRelationship(UserRelationship userRelationship, int createUpdateUserId);

        void DeleteUserRelationshipPreference(int preferenceId);

        IDataReader GetUserRelationshipPreferenceById(int preferenceId);

        IDataReader GetUserRelationshipPreference(int userId, int relationshipId);

        int SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference, int createUpdateUserId);
    }
}
