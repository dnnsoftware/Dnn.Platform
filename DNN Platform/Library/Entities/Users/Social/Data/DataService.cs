﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.ComponentModel;
using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Entities.Users.Social.Data
{
    internal class DataService : ComponentBase<IDataService, DataService>, IDataService        
    {
        private readonly DataProvider _provider = DataProvider.Instance();

        #region RelationshipType CRUD

        public IDataReader GetAllRelationshipTypes()
        {
            return _provider.ExecuteReader("GetAllRelationshipTypes");
        }

        public IDataReader GetRelationshipType(int relationshipTypeId)
        {
            return _provider.ExecuteReader("GetRelationshipType", relationshipTypeId);
        }

        public void DeleteRelationshipType(int relationshipTypeId)
        {            
            _provider.ExecuteNonQuery("DeleteRelationshipType", relationshipTypeId);
        }

        public int SaveRelationshipType(RelationshipType relationshipType, int createUpdateUserId)
        {
            return _provider.ExecuteScalar<int>("SaveRelationshipType", relationshipType.RelationshipTypeId, relationshipType.Direction, relationshipType.Name, relationshipType.Description, createUpdateUserId);
        }

        #endregion

        #region Relationship CRUD

        public void DeleteRelationship(int relationshipId)
        {
            _provider.ExecuteNonQuery("DeleteRelationship", relationshipId);
        }

        public IDataReader GetRelationship(int relationshipId)
        {
            return _provider.ExecuteReader("GetRelationship", relationshipId);
        }

        public IDataReader GetRelationshipsByUserId(int userId)
        {
            return _provider.ExecuteReader("GetRelationshipsByUserID", userId);
        }

        public IDataReader GetRelationshipsByPortalId(int portalId)
        {
            return _provider.ExecuteReader("GetRelationshipsByPortalID", portalId);
        }

        public int SaveRelationship(Relationship relationship, int createUpdateUserId)
        {
            return _provider.ExecuteScalar<int>("SaveRelationship", relationship.RelationshipId, relationship.RelationshipTypeId, relationship.Name, relationship.Description, _provider.GetNull(relationship.UserId), _provider.GetNull(relationship.PortalId), relationship.DefaultResponse, createUpdateUserId);
        }

        #endregion

        #region UserRelationship CRUD

        public void DeleteUserRelationship(int userRelationshipId)
        {
            _provider.ExecuteNonQuery("DeleteUserRelationship", userRelationshipId);
        }

        public IDataReader GetUserRelationship(int userRelationshipId)
        {
            return _provider.ExecuteReader("GetUserRelationship", userRelationshipId);
        }

        public IDataReader GetUserRelationship(int userId, int relatedUserId, int relationshipId, RelationshipDirection relationshipDirection)
        {
            return _provider.ExecuteReader("GetUserRelationshipsByMultipleIDs", userId, relatedUserId, relationshipId, relationshipDirection);
        }

        public IDataReader GetUserRelationships(int userId)
        {
            return _provider.ExecuteReader("GetUserRelationships", userId);
        }

        public IDataReader GetUserRelationshipsByRelationshipId(int relationshipId)
        {
            return _provider.ExecuteReader("GetUserRelationshipsByRelationshipID", relationshipId);
        }

        public int SaveUserRelationship(UserRelationship userRelationship, int createUpdateUserId)
        {
            return _provider.ExecuteScalar<int>("SaveUserRelationship", userRelationship.UserRelationshipId, userRelationship.UserId, userRelationship.RelatedUserId, userRelationship.RelationshipId, userRelationship.Status, createUpdateUserId);
        }

        #endregion

        #region UserRelationshipPreference CRUD

        public IDataReader GetUserRelationshipPreferenceById(int preferenceId)
        {
            return _provider.ExecuteReader("GetUserRelationshipPreferenceByID", preferenceId);
        }

        public IDataReader GetUserRelationshipPreference(int userId, int relationshipId)
        {
            return _provider.ExecuteReader("GetUserRelationshipPreference", userId, relationshipId);
        }

        public void DeleteUserRelationshipPreference(int preferenceId)
        {
            _provider.ExecuteNonQuery("DeleteUserRelationshipPreference", preferenceId);
        }

        public int SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference, int createUpdateUserId)
        {
            return _provider.ExecuteScalar<int>("SaveUserRelationshipPreference", userRelationshipPreference.PreferenceId, userRelationshipPreference.UserId, userRelationshipPreference.RelationshipId, userRelationshipPreference.DefaultResponse, createUpdateUserId);
        }

        #endregion
    }
}
