// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social.Data;

using System.Data;

using DotNetNuke.ComponentModel;
using DotNetNuke.Data;

internal class DataService : ComponentBase<IDataService, DataService>, IDataService
{
    private readonly DataProvider provider = DataProvider.Instance();

    /// <inheritdoc/>
    public IDataReader GetAllRelationshipTypes()
    {
        return this.provider.ExecuteReader("GetAllRelationshipTypes");
    }

    /// <inheritdoc/>
    public IDataReader GetRelationshipType(int relationshipTypeId)
    {
        return this.provider.ExecuteReader("GetRelationshipType", relationshipTypeId);
    }

    /// <inheritdoc/>
    public void DeleteRelationshipType(int relationshipTypeId)
    {
        this.provider.ExecuteNonQuery("DeleteRelationshipType", relationshipTypeId);
    }

    /// <inheritdoc/>
    public int SaveRelationshipType(RelationshipType relationshipType, int createUpdateUserId)
    {
        return this.provider.ExecuteScalar<int>("SaveRelationshipType", relationshipType.RelationshipTypeId, relationshipType.Direction, relationshipType.Name, relationshipType.Description, createUpdateUserId);
    }

    /// <inheritdoc/>
    public void DeleteRelationship(int relationshipId)
    {
        this.provider.ExecuteNonQuery("DeleteRelationship", relationshipId);
    }

    /// <inheritdoc/>
    public IDataReader GetRelationship(int relationshipId)
    {
        return this.provider.ExecuteReader("GetRelationship", relationshipId);
    }

    /// <inheritdoc/>
    public IDataReader GetRelationshipsByUserId(int userId)
    {
        return this.provider.ExecuteReader("GetRelationshipsByUserID", userId);
    }

    /// <inheritdoc/>
    public IDataReader GetRelationshipsByPortalId(int portalId)
    {
        return this.provider.ExecuteReader("GetRelationshipsByPortalID", portalId);
    }

    /// <inheritdoc/>
    public int SaveRelationship(Relationship relationship, int createUpdateUserId)
    {
        return this.provider.ExecuteScalar<int>("SaveRelationship", relationship.RelationshipId, relationship.RelationshipTypeId, relationship.Name, relationship.Description, this.provider.GetNull(relationship.UserId), this.provider.GetNull(relationship.PortalId), relationship.DefaultResponse, createUpdateUserId);
    }

    /// <inheritdoc/>
    public void DeleteUserRelationship(int userRelationshipId)
    {
        this.provider.ExecuteNonQuery("DeleteUserRelationship", userRelationshipId);
    }

    /// <inheritdoc/>
    public IDataReader GetUserRelationship(int userRelationshipId)
    {
        return this.provider.ExecuteReader("GetUserRelationship", userRelationshipId);
    }

    /// <inheritdoc/>
    public IDataReader GetUserRelationship(int userId, int relatedUserId, int relationshipId, RelationshipDirection relationshipDirection)
    {
        return this.provider.ExecuteReader("GetUserRelationshipsByMultipleIDs", userId, relatedUserId, relationshipId, relationshipDirection);
    }

    /// <inheritdoc/>
    public IDataReader GetUserRelationships(int userId)
    {
        return this.provider.ExecuteReader("GetUserRelationships", userId);
    }

    /// <inheritdoc/>
    public IDataReader GetUserRelationshipsByRelationshipId(int relationshipId)
    {
        return this.provider.ExecuteReader("GetUserRelationshipsByRelationshipID", relationshipId);
    }

    /// <inheritdoc/>
    public int SaveUserRelationship(UserRelationship userRelationship, int createUpdateUserId)
    {
        return this.provider.ExecuteScalar<int>("SaveUserRelationship", userRelationship.UserRelationshipId, userRelationship.UserId, userRelationship.RelatedUserId, userRelationship.RelationshipId, userRelationship.Status, createUpdateUserId);
    }

    /// <inheritdoc/>
    public IDataReader GetUserRelationshipPreferenceById(int preferenceId)
    {
        return this.provider.ExecuteReader("GetUserRelationshipPreferenceByID", preferenceId);
    }

    /// <inheritdoc/>
    public IDataReader GetUserRelationshipPreference(int userId, int relationshipId)
    {
        return this.provider.ExecuteReader("GetUserRelationshipPreference", userId, relationshipId);
    }

    /// <inheritdoc/>
    public void DeleteUserRelationshipPreference(int preferenceId)
    {
        this.provider.ExecuteNonQuery("DeleteUserRelationshipPreference", preferenceId);
    }

    /// <inheritdoc/>
    public int SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference, int createUpdateUserId)
    {
        return this.provider.ExecuteScalar<int>("SaveUserRelationshipPreference", userRelationshipPreference.PreferenceId, userRelationshipPreference.UserId, userRelationshipPreference.RelationshipId, userRelationshipPreference.DefaultResponse, createUpdateUserId);
    }
}
