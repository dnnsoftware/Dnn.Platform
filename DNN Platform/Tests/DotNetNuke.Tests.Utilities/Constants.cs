// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace DotNetNuke.Tests.Utilities
{
    using System.Web.Caching;

    using DotNetNuke.Entities.Content.Taxonomy;

    public class Constants
    {
        public const string CACHEING_InValidKey = "InValidKey";
        public const string CACHEING_ParamCacheKey = "CacheKey";
        public const string CACHEING_ValidKey = "ValidKey";
        public const string CACHEING_ValidValue = "ValidValue";
        public const int USER_Null = -1;
        public const int USER_ValidId = 200;
        public const int USER_InValidId = 42;
        public const int USER_AnonymousUserId = -1;
        public const int USER_TenId = 10;
        public const string USER_TenName = "user10";
        public const int USER_ElevenId = 11;
        public const string USER_ElevenName = "user11";
        public const int UserID_Host = 1;
        public const int UserID_Admin = 2;
        public const int UserID_User12 = 12;
        public const int UserID_FirstSocialGroupOwner = 13;
        public const string UserName_Admin = "admin";
        public const string UserName_Host = "host";
        public const string UserName_User12 = "user12";
        public const string UserDisplayName_Host = "SuperUser Account";
        public const string UserDisplayName_Admin = "Administrator Account";
        public const string UserDisplayName_User12 = "User 12";
        public const string UserDisplayName_FirstSocialGroupOwner = "First Social Group Owner";
        public const string RuFirstName = "TestUser";
        public const string RuLastName = "Automation";
        public const string DefaultPassword = "dnnhost";
        public const int RoleID_Administrators = 0;
        public const int RoleID_RegisteredUsers = 1;
        public const int RoleID_Subscribers = 2;
        public const int RoleID_Translator_EN_US = 3;
        public const int RoleID_FirstSocialGroup = 4;

        public const string RoleName_Administrators = "Administrators";
        public const string RoleName_RegisteredUsers = "RegisteredUsers";
        public const string RoleName_Subscribers = "Subscribers";
        public const string RoleName_Translator_EN_US = "translator_EN_US";
        public const string RoleName_FirstSocialGroup = "First Social Group";
        public const int PORTAL_Zero = 0;
        public const int PORTAL_One = 1;
        public const int PORTAL_Null = -1;
        public const int PORTAL_InValidPortalId = -1;
        public const int PORTAL_ValidPortalId = 1;

        /// <summary>The setting name that stores whether attachments are allowed.</summary>
        public const string PORTALSETTING_MessagingAllowAttachments_Name = "MessagingAllowAttachments";

        /// <summary>The setting name that stores whether outgoing emails will include an attachment.</summary>
        public const string PORTALSETTING_MessagingIncludeAttachments_Name = "MessagingIncludeAttachments";

        public const string PORTALSETTING_MessagingAllowAttachments_Value_YES = "YES";
        public const string PORTALSETTING_MessagingAllowAttachments_Value_NO = "NO";
        public const string CULTURE_EN_US = "en-US";
        public const int PORTALGROUP_ValidPortalGroupId = 1;
        public const int PORTALGROUP_AddPortalGroupId = 2;
        public const int PORTALGROUP_DeletePortalGroupId = 3;
        public const int PORTALGROUP_InValidPortalGroupId = 999;

        public const string PORTALGROUP_ValidName = "PortalGroupName";
        public const string PORTALGROUP_ValidDescription = "PortalGroupDescription";
        public const int PORTALGROUP_UpdatePortalGroupId = 4;

        public const string PORTALGROUP_UpdateName = "UpdateName";
        public const string PORTALGROUP_UpdateDescription = "UpdateDescription";

        public const int PORTALGROUP_ValidPortalGroupCount = 5;
        public const string PORTALGROUP_ValidNameFormat = "PortalGroupName {0}";
        public const string PORTALGROUP_ValidDescriptionFormat = "PortalGroupDescription {0}";

        // Valid Content values
        public const int CONTENT_ValidContentItemId = 1;
        public const string CONTENT_ValidContent = "Content";
        public const string CONTENT_ValidContentKey = "ContentKey";
        public const string CONTENT_ValidTitle = "ContentTitle";
        public const string CONTENT_ValidTitle2 = "ContentTitle2";
        public const int CONTENT_ValidModuleId = 30;
        public const int CONTENT_ValidPortalId = 20;
        public const int CONTENT_ValidTabId = 10;

        public const int CONTENT_ValidContentItemCount = 5;
        public const string CONTENT_ValidContentFormat = "Content {0}";
        public const string CONTENT_ValidContentKeyFormat = "ContentKey {0}";
        public const int CONTENT_ValidStartTabId = 10;
        public const int CONTENT_ValidStartModuleId = 100;

        // InValid Content values
        public const int CONTENT_InValidContentItemId = 999;
        public const string CONTENT_InValidContent = "";
        public const int CONTENT_InValidModuleId = 888;
        public const int CONTENT_InValidPortalId = 777;
        public const int CONTENT_InValidTabId = 99;

        public const int CONTENT_IndexedTrueItemCount = 2;
        public const int CONTENT_TaggedItemCount = 4;
        public const int CONTENT_IndexedFalseItemCount = 3;
        public const bool CONTENT_IndexedFalse = false;
        public const bool CONTENT_IndexedTrue = true;

        public const int CONTENT_AddContentItemId = 2;
        public const int CONTENT_DeleteContentItemId = 3;
        public const int CONTENT_UpdateContentItemId = 4;

        public const string CONTENT_UpdateContent = "Update";
        public const string CONTENT_UpdateContentKey = "UpdateKey";

        public const string CONTENT_ValidMetaDataName = "Creator";
        public const string CONTENT_ValidMetaDataValue = "John Smith";
        public const string CONTENT_NewMetaDataName = "Abstract";
        public const string CONTENT_NewMetaDataValue = "My abstract";
        public const string CONTENT_InValidMetaDataName = "InvalidName";
        public const string CONTENT_InValidMetaDataValue = "InvalidValue";
        public const int CONTENT_MetaDataCount = 4;
        public const int CONTENTTYPE_ValidContentTypeId = 1;
        public const string CONTENTTYPE_ValidContentType = "ContentType Name";

        public const int CONTENTTYPE_ValidContentTypeCount = 5;
        public const string CONTENTTYPE_ValidContentTypeFormat = "ContentType Name {0}";

        public const int CONTENTTYPE_InValidContentTypeId = -1;
        public const string CONTENTTYPE_InValidContentType = "Invalid ContentType";

        public const int CONTENTTYPE_AddContentTypeId = 2;
        public const int CONTENTTYPE_DeleteContentTypeId = 3;
        public const int CONTENTTYPE_UpdateContentTypeId = 4;
        public const int CONTENTTYPE_GetByNameContentTypeId = 5;
        public const string CONTENTTYPE_GetByNameContentType = "TestGetByName";
        public const string CONTENTTYPE_OriginalUpdateContentType = "TestUpdate";

        public const string CONTENTTYPE_UpdateContentType = "Update Name";

        public const int CONTENTTYPE_AddDataTypeId = 4;
        public const int CONTENTTYPE_ValidDataTypeId = 3;
        public const int CONTENTTYPE_InValidDataTypeId = -1;
        public const int CONTENTTYPE_UpdateDataTypeId = 2;
        public const int CONTENTTYPE_ValidDataTypeCount = 10;

        public const int CONTENTTYPE_AddFieldDefinitionId = 7;
        public const int CONTENTTYPE_ValidFieldDefinitionId = 3;
        public const int CONTENTTYPE_InValidFieldDefinitionId = -1;
        public const int CONTENTTYPE_UpdateFieldDefinitionId = 2;
        public const int CONTENTTYPE_ValidFieldDefinitionCount = 10;

        public const int CONTENTTYPE_InValidValidatorTypeId = -1;
        public const int CONTENTTYPE_ValidValidationRuleId = 3;
        public const int CONTENTTYPE_AddValidationRuleId = 2;
        public const int CONTENTTYPE_ValidValidationRuleCount = 25;

        public const int CONTENTTYPE_AddValidatorTypeId = 2;
        public const int CONTENTTYPE_ValidValidatorTypeCount = 23;
        public const int CONTENTTYPE_ValidValidatorTypeId = 3;
        public const int CONTENTTYPE_UpdateValidatorTypeId = 4;
        public const string CONTENTTYPE_ValidValidatorTypeName = "Name";
        public const string CONTENTTYPE_ValidValidatorClassName = "Class";
        public const string CONTENTTYPE_ValidValidatorErrorKey = "Key";
        public const string CONTENTTYPE_ValidValidatorErrorMessage = "Message";

        public const int CONTENTTYPE_AddContentTemplateId = 5;
        public const int CONTENTTYPE_ValidContentTemplateId = 2;
        public const int CONTENTTYPE_ValidContentTemplateCount = 7;
        public const int CONTENTTYPE_InValidContentTemplateId = -1;
        public const int CONTENTTYPE_UpdateContentTemplateId = 4;
        public const int SCOPETYPE_ValidScopeTypeId = 1;
        public const string SCOPETYPE_ValidScopeType = "ScopeType Name";

        public const int SCOPETYPE_ValidScopeTypeCount = 5;
        public const string SCOPETYPE_ValidScopeTypeFormat = "ScopeType Name {0}";

        public const int SCOPETYPE_InValidScopeTypeId = 999;
        public const string SCOPETYPE_InValidScopeType = "Invalid ScopeType";

        public const int SCOPETYPE_AddScopeTypeId = 2;
        public const int SCOPETYPE_DeleteScopeTypeId = 3;
        public const int SCOPETYPE_UpdateScopeTypeId = 4;
        public const int SCOPETYPE_GetByNameScopeTypeId = 5;
        public const string SCOPETYPE_GetByNameScopeType = "TestGetByName";
        public const string SCOPETYPE_OriginalUpdateScopeType = "TestUpdate";

        public const string SCOPETYPE_UpdateScopeType = "Update Name";
        public const int TAG_DuplicateContentItemId = 1;
        public const int TAG_DuplicateTermId = 6;
        public const int TAG_NoContentContentId = 99;
        public const int TAG_ValidContentId = 1;
        public const int TAG_ValidContentCount = 2;
        public const string TERM_CacheKey = "DNN_Terms_{0}";

        public const int TERM_ValidTermId = 1;
        public const int TERM_InValidTermId = 999;
        public const int TERM_AddTermId = 2;
        public const int TERM_DeleteTermId = 3;
        public const int TERM_UpdateTermId = 4;
        public const int TERM_ValidParentTermId = 2;
        public const int TERM_InValidParentTermId = 888;

        public const string TERM_ValidName = "Term Name";
        public const string TERM_InValidName = "";
        public const string TERM_UnusedName = "Unused";
        public const string TERM_UpdateName = "Update Name";
        public const string TERM_OriginalUpdateName = "LCD";
        public const int TERM_ValidVocabularyId = 2;
        public const int TERM_ValidGetTermsByVocabularyCount = 9;

        public const int TERM_InsertChildBeforeParentId = 2;

        public const int TERM_ValidTermStartId = 1;
        public const int TERM_ValidCount = 5;
        public const string TERM_ValidNameFormat = "Term Name {0}";
        public const int TERM_ValidCountForVocabulary1 = 2;
        public const int TERM_ValidVocabulary1 = 1;
        public const int TERM_ValidVocabulary2 = 2;
        public const int TERM_ValidWeight = 0;
        public const int TERM_UpdateWeight = 5;

        public const int TERM_ValidCountForContent1 = 2;
        public const int TERM_ValidContent1 = 1;
        public const int TERM_ValidContent2 = 2;
        public const string VOCABULARY_CacheKey = "DNN_Vocabularies";

        public const int VOCABULARY_ValidVocabularyId = 1;
        public const int VOCABULARY_HierarchyVocabularyId = 2;
        public const int VOCABULARY_InValidVocabularyId = 999;
        public const int VOCABULARY_AddVocabularyId = 2;
        public const int VOCABULARY_DeleteVocabularyId = 3;
        public const int VOCABULARY_UpdateVocabularyId = 4;

        public const string VOCABULARY_ValidName = "Vocabulary Name";
        public const string VOCABULARY_InValidName = "";
        public const string VOCABULARY_UpdateName = "Update Name";

        public const VocabularyType VOCABULARY_ValidType = VocabularyType.Simple;
        public const int VOCABULARY_SimpleTypeId = 1;
        public const int VOCABULARY_HierarchyTypeId = 2;

        public const int VOCABULARY_ValidScopeTypeId = 2;
        public const int VOCABULARY_InValidScopeTypeId = 888;
        public const int VOCABULARY_UpdateScopeTypeId = 3;

        public const int VOCABULARY_ValidScopeId = 1;
        public const int VOCABULARY_InValidScopeId = 3;
        public const int VOCABULARY_UpdateScopeId = 2;
        public const string VOCABULARY_OriginalUpdateName = "TestUpdate";

        public const int VOCABULARY_ValidWeight = 0;
        public const int VOCABULARY_UpdateWeight = 5;

        public const int VOCABULARY_ValidCount = 5;
        public const string VOCABULARY_ValidNameFormat = "Vocabulary Name {0}";
        public const int VOCABULARY_ValidCountForScope1 = 2;
        public const int VOCABULARY_ValidScope1 = 1;
        public const int VOCABULARY_ValidScope2 = 2;
        public const int FOLDER_ValidFileId = 1;
        public const int FOLDER_InvalidFileId = -1;
        public const int FOLDER_ValidFileSize = 16;
        public const int FOLDER_ValidFolderId = 3;
        public const int FOLDER_OtherValidFolderId = 7;
        public const int FOLDER_ValidFolderMappingID = 5;
        public const string FOLDER_ValidFileName = "file.txt";
        public const string FOLDER_ValidSvgFileName = "file.svg";
        public const string FOLDER_ValidFilePath = "C:\\folder\\file.txt";
        public const string FOLDER_ValidFolderName = "folder";
        public const string FOLDER_ValidFolderPath = "C:\\folder";
        public const string FOLDER_ValidFolderRelativePath = "folder/";
        public const string FOLDER_ValidFolderProviderType = "ValidFolderProvider";
        public const string FOLDER_ValidHostPath = "C:\\inetpub\\wwwroot\\dotnetnuke\\portals\\_default";
        public const string FOLDER_ValidRootFolderMapPath = "C:\\inetpub\\wwwroot\\dotnetnuke\\portals\\20";
        public const string FOLDER_ValidSecureFilePath = "C:\\folder\\file.txt.resources";
        public const string FOLDER_ValidSubFolderName = "subfolder";
        public const string FOLDER_ValidSubFolderPath = "C:\\folder\\subfolder";
        public const string FOLDER_ValidSubFolderRelativePath = "folder/subfolder/";
        public const string FOLDER_InvalidSubFolderRelativePath = "folder/subfolder/../";
        public const string FOLDER_ValidUNCFolderPath = @"\\SERVER\folder";
        public const string FOLDER_ValidUNCSubFolderPath = @"\\SERVER\folder\subfolder";
        public const string FOLDER_ValidZipFileName = "file.zip";
        public const string FOLDER_ValidZipFilePath = "C:\\folder\\file.zip";
        public const string FOLDER_OtherValidFileName = "otherfile.txt";
        public const string FOLDER_OtherInvalidFileNameExtension = "otherfile.asp";
        public const string FOLDER_OtherValidFilePath = "C:\\folder\\otherfile.txt";
        public const string FOLDER_OtherValidFolderName = "otherfolder";
        public const string FOLDER_OtherValidFolderPath = "C:\\otherfolder";
        public const string FOLDER_OtherValidFolderRelativePath = "otherfolder/";
        public const string FOLDER_OtherValidSecureFilePath = "C:\\folder\\otherfile.txt.resources";
        public const string FOLDER_OtherValidSubFolderPath = "C:\\folder\\othersubfolder";
        public const string FOLDER_OtherValidSubFolderRelativePath = "folder/othersubfolder/";
        public const string FOLDER_ModifiedFileHash = "0123456789X";
        public const string FOLDER_UnmodifiedFileHash = "0123456789";
        public const string FOLDER_FileStartDate = "2010-01-01T00:00:00";
        public const int SOCIAL_InValidRelationshipType = 999;
        public const int SOCIAL_InValidRelationship = 999;
        public const int SOCIAL_InValidUserRelationship = 999;
        public const int SOCIAL_FriendRelationshipTypeID = 1;
        public const int SOCIAL_FollowerRelationshipTypeID = 2;
        public const int SOCIAL_FriendRelationshipID = 1;
        public const int SOCIAL_FollowerRelationshipID = 2;
        public const int SOCIAL_UserRelationshipIDUser10User11 = 3;
        public const int SOCIAL_UserRelationshipIDUser12User13 = 4;
        public const int SOCIAL_PrefereceIDForUser11 = 1;

        public const string SOCIAL_RelationshipTypeName = "TestType";
        public const string SOCIAL_RelationshipName = "TestName";

        public const string LOCALIZATION_RelationshipType_Deleted_Key = "RelationshipType_Deleted";
        public const string LOCALIZATION_RelationshipType_Deleted = "Deleted RelationshipType {0} : ID {1}";
        public const string LOCALIZATION_RelationshipType_Added_Key = "RelationshipType_Added";
        public const string LOCALIZATION_RelationshipType_Added = "Added RelationshipType {0}";
        public const string LOCALIZATION_RelationshipType_Updated_Key = "RelationshipType_Updated";
        public const string LOCALIZATION_RelationshipType_Updated = "Updated RelationshipType {0}";

        public const string LOCALIZATION_Relationship_Deleted_Key = "Relationship_Deleted";
        public const string LOCALIZATION_Relationship_Deleted = "Deleted Relationship {0} : ID {1}";
        public const string LOCALIZATION_Relationship_Added_Key = "Relationship_Added";
        public const string LOCALIZATION_Relationship_Added = "Added Relationship {0}";
        public const string LOCALIZATION_Relationship_Updated_Key = "Relationship_Updated";
        public const string LOCALIZATION_Relationship_Updated = "Updated Relationship {0}";

        public const string LOCALIZATION_UserRelationship_Deleted_Key = "UserRelationship_Deleted";
        public const string LOCALIZATION_UserRelationship_Deleted = "Deleted UserRelationship ID {0}, UserID {1}, RelatedUserID {2}";
        public const string LOCALIZATION_UserRelationship_Added_Key = "UserRelationship_Added";
        public const string LOCALIZATION_UserRelationship_Added = "Added UserRelationship ID {0}, UserID {1}, RelatedUserID {2}";
        public const string LOCALIZATION_UserRelationship_Updated_Key = "UserRelationship_Updated";
        public const string LOCALIZATION_UserRelationship_Updated = "Updated UserRelationship ID {0}, UserID {1}, RelatedUserID {2}";

        public const string LOCALIZATION_UserRelationshipPreference_Deleted_Key = "UserRelationshipPreference_Deleted";
        public const string LOCALIZATION_UserRelationshipPreference_Deleted = "Deleted UserRelationshipPreference ID {0} for User ID {1} and Relationship ID {2}";
        public const string LOCALIZATION_UserRelationshipPreference_Added_Key = "UserRelationshipPreference_Added";
        public const string LOCALIZATION_UserRelationshipPreference_Added = "Added UserRelationshipPreference ID {0} for User ID {1} and Relationship ID {2}";
        public const string LOCALIZATION_UserRelationshipPreference_Updated_Key = "UserRelationshipPreference_Updated";
        public const string LOCALIZATION_UserRelationshipPreference_Updated = "UpdatedUserRelationshipPreference ID {0} for User ID {1} and Relationship ID {2}";
        public const bool Messaging_ReadMessage = true;
        public const bool Messaging_UnReadMessage = false;
        public const bool Messaging_ArchivedMessage = true;
        public const bool Messaging_UnArchivedMessage = false;
        public const int Messaging_RecipientId_1 = 1;
        public const int Messaging_RecipientId_2 = 2;
        public const int Messaging_MessageId_1 = 1;
        public const int Messaging_NotificationTypeId = 1;
        public const string Messaging_NotificationTypeName = "AcceptFriend";
        public const string Messaging_NotificationTypeDescription = "Accept Friend Notification";
        public const int Messaging_NotificationTypeTTL = 1440; // This is one day in minutes
        public const int Messaging_NotificationTypeDesktopModuleId = 3;
        public const int Messaging_NotificationTypeActionId = 6;
        public const string Messaging_NotificationTypeActionNameResourceKey = "Accept";
        public const string Messaging_NotificationTypeActionDescriptionResourceKey = "Accept a friend request";
        public const string Messaging_NotificationTypeActionConfirmResourceKey = "Are you sure you want to accept this friend?";
        public const string Messaging_NotificationTypeActionAPICall = "~/API/ModuleName/ModuleService.ashx/Accept";
        public const int Messaging_NotificationActionId = 4;
        public const string Messaging_NotificationActionKey = "{F:1}{U:2}";
        public const string Messaging_NotificationSubject = "Friend Request Received";
        public const string Messaging_NotificationBody = "You've received a new friend request from {0}";
        public const bool Messaging_IncludeDismissAction = true;
        public const string Messaging_NotificationContext = "context";
        public const string COLUMNNAME_Name = "Name";
        public const string COLUMNNAME_PersonName = "PersonName";

        public const string PETAPOCO_DatabaseName = "Test.sdf";
        public const string PETAPOCO_DogTableName = "Dogs";
        public const string PETAPOCO_CreateDogTableSql = "CREATE TABLE Dogs (ID int IDENTITY(1,1) NOT NULL, Name nvarchar(100) NOT NULL, Age int NULL)";
        public const string PETAPOCO_InsertDogRow = "INSERT INTO Dogs (Name, Age) VALUES ('{0}', {1})";
        public const string PETAPOCO_DogNames = "Spot,Buster,Buddy,Vienna,Gizmo";
        public const string PETAPOCO_DogAges = "1,5,3,4,6";
        public const int PETAPOCO_RecordCount = 5;

        public const string PETAPOCO_InsertDogName = "Milo";
        public const int PETAPOCO_InsertDogAge = 3;

        public const int PETAPOCO_DeleteDogId = 2;
        public const string PETAPOCO_DeleteDogName = "Buster";
        public const int PETAPOCO_DeleteDogAge = 5;

        public const int PETAPOCO_ValidDogId = 3;
        public const string PETAPOCO_ValidDogName = "Buddy";
        public const int PETAPOCO_ValidDogAge = 3;

        public const int PETAPOCO_InvalidDogId = 999;

        public const string PETAPOCO_UpdateDogName = "Milo";
        public const int PETAPOCO_UpdateDogAge = 6;
        public const int PETAPOCO_UpdateDogId = 3;

        public const int PETAPOCO_ValidCatId = 3;

        public const string CACHE_DogsKey = "Dogs";
        public const string CACHE_CatsKey = "Cats";
        public const string CACHE_ScopeAll = "";
        public const string CACHE_ScopeModule = "ModuleId";
        public const int CACHE_TimeOut = 10;
        public const CacheItemPriority CACHE_Priority = CacheItemPriority.High;

        public const string TABLENAME_Dog = "Dogs";
        public const string TABLENAME_Key = "ID";
        public const string TABLENAME_Prefix = "dnn_";
        public const string TABLENAME_Person = "People";
        public const string TABLENAME_Person_Key = "PersonID";
        public const int PAGE_First = 0;
        public const int PAGE_Second = 1;
        public const int PAGE_Last = 4;
        public const int PAGE_RecordCount = 5;
        public const int PAGE_TotalCount = 22;

        public const int PAGE_NegativeIndex = -1;
        public const int PAGE_OutOfRange = 5;
        public const int TAB_ValidId = 10;
        public const int TAB_InValidId = -1;
        public const int MODULE_ValidId = 100;
        public const int MODULE_InValidId = -1;
    }
}
