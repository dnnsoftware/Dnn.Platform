// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

using Microsoft.Extensions.DependencyInjection;

/// <summary>The default <see cref="IJournalController"/> implementation.</summary>
internal class JournalControllerImpl : IJournalController
{
    private const string AllowResizePhotosSetting = "Journal_AllowResizePhotos";
    private const string AllowPhotosSetting = "Journal_AllowPhotos";
    private const string EditorEnabledSetting = "Journal_EditorEnabled";

    private static readonly string[] InvalidSecuritySetsWithoutId = ["R", "U", "F", "P"];
    private static readonly char[] ValidSecurityDescriptors = ['E', 'C', 'R', 'U', 'F', 'P'];
    private readonly IJournalDataService dataService;
    private readonly IHostSettings hostSettings;
    private readonly IRoleController roleController;
    private readonly ISearchHelper searchHelper;
    private readonly DataProvider dataProvider;
    private readonly IFolderManager folderManager;
    private readonly IFileManager fileManager;
    private readonly IFileContentTypeManager fileContentTypeManager;
    private readonly PortalSecurity portalSecurity;

    /// <summary>Initializes a new instance of the <see cref="JournalControllerImpl"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.1.1. Please use overload with IJournalDataService. Scheduled removal in v12.0.0.")]
    public JournalControllerImpl()
        : this(null, null, null, null, null, null, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="JournalControllerImpl"/> class.</summary>
    /// <param name="journalDataService">The journal data service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="roleController">The role controller.</param>
    /// <param name="searchHelper">The search helpers.</param>
    /// <param name="dataProvider">The data provider.</param>
    /// <param name="folderManager">The folder manager.</param>
    /// <param name="fileManager">The file manager.</param>
    /// <param name="fileContentTypeManager">The file content type manager.</param>
    /// <param name="portalSecurity">The portal security instance.</param>
    public JournalControllerImpl(IJournalDataService journalDataService, IHostSettings hostSettings, IRoleController roleController, ISearchHelper searchHelper, DataProvider dataProvider, IFolderManager folderManager, IFileManager fileManager, IFileContentTypeManager fileContentTypeManager, PortalSecurity portalSecurity)
    {
        this.dataService = journalDataService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJournalDataService>();
        this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        this.roleController = roleController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>();
        this.searchHelper = searchHelper ?? Globals.GetCurrentServiceProvider().GetRequiredService<ISearchHelper>();
        this.dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
        this.folderManager = folderManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFolderManager>();
        this.fileManager = fileManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileManager>();
        this.fileContentTypeManager = fileContentTypeManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileContentTypeManager>();
        this.portalSecurity = portalSecurity ?? Globals.GetCurrentServiceProvider().GetRequiredService<PortalSecurity>();
    }

    /// <summary>Save the journal object into database.</summary>
    /// <param name="journalItem">Journal object.</param>
    /// <param name="tabId">The ID of the tab of the journal item context.</param>
    /// <param name="moduleId">The ID of the module of the journal item context.</param>
    public void SaveJournalItem(JournalItem journalItem, int tabId, int moduleId)
    {
        if (journalItem.UserId < 1)
        {
            throw new ArgumentException("journalItem.UserId must be for a real user");
        }

        UserInfo currentUser = UserController.GetUserById(journalItem.PortalId, journalItem.UserId);
        if (currentUser == null)
        {
            throw new UserDoesNotExistException("Unable to locate the current user");
        }

        string xml = null;
        if (!string.IsNullOrEmpty(journalItem.Title))
        {
            journalItem.Title = this.portalSecurity.InputFilter(journalItem.Title, PortalSecurity.FilterFlag.NoMarkup);
        }

        if (!string.IsNullOrEmpty(journalItem.Summary))
        {
            journalItem.Summary = this.portalSecurity.InputFilter(journalItem.Summary, PortalSecurity.FilterFlag.NoScripting);
        }

        if (!string.IsNullOrEmpty(journalItem.Body))
        {
            journalItem.Body = this.portalSecurity.InputFilter(journalItem.Body, PortalSecurity.FilterFlag.NoScripting);
        }

        if (!string.IsNullOrEmpty(journalItem.Body))
        {
            var xDoc = new XmlDocument { XmlResolver = null };
            var itemsNode = xDoc.CreateElement("items");
            var itemNode = xDoc.CreateElement("item");
            itemNode.AppendChild(CreateElement(xDoc, "id", "-1"));
            itemNode.AppendChild(CreateCDataElement(xDoc, "body", journalItem.Body));
            itemsNode.AppendChild(itemNode);
            xDoc.AppendChild(itemsNode);
            var xDec = xDoc.CreateXmlDeclaration("1.0", null, null);
            xDec.Encoding = "UTF-16";
            xDec.Standalone = "yes";
            var root = xDoc.DocumentElement;
            xDoc.InsertBefore(xDec, root);
            journalItem.JournalXML = xDoc;
            xml = journalItem.JournalXML.OuterXml;
        }

        if (journalItem.ItemData != null)
        {
            if (!string.IsNullOrEmpty(journalItem.ItemData.Title))
            {
                journalItem.ItemData.Title = this.portalSecurity.InputFilter(journalItem.ItemData.Title, PortalSecurity.FilterFlag.NoMarkup);
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.Description))
            {
                journalItem.ItemData.Description = this.portalSecurity.InputFilter(journalItem.ItemData.Description, PortalSecurity.FilterFlag.NoScripting);
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.Url))
            {
                journalItem.ItemData.Url = this.portalSecurity.InputFilter(journalItem.ItemData.Url, PortalSecurity.FilterFlag.NoScripting);
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.ImageUrl))
            {
                journalItem.ItemData.ImageUrl = this.portalSecurity.InputFilter(journalItem.ItemData.ImageUrl, PortalSecurity.FilterFlag.NoScripting);
            }
        }

        var journalData = journalItem.ItemData.ToJson();
        if (journalData == "null")
        {
            journalData = null;
        }

        this.PrepareSecuritySet(journalItem);

        journalItem.JournalId = this.dataService.Journal_Save(
            journalItem.PortalId,
            journalItem.UserId,
            journalItem.ProfileId,
            journalItem.SocialGroupId,
            journalItem.JournalId,
            journalItem.JournalTypeId,
            journalItem.Title,
            journalItem.Summary,
            journalItem.Body,
            journalData,
            xml,
            journalItem.ObjectKey,
            journalItem.AccessKey,
            journalItem.SecuritySet,
            journalItem.CommentsDisabled,
            journalItem.CommentsHidden);

        var updatedJournalItem = this.GetJournalItem(journalItem.PortalId, journalItem.UserId, journalItem.JournalId);
        journalItem.DateCreated = updatedJournalItem.DateCreated;
        journalItem.DateUpdated = updatedJournalItem.DateUpdated;
        var cnt = new Content();

        if (journalItem.ContentItemId > 0)
        {
            cnt.UpdateContentItem(journalItem, tabId, moduleId);
            this.dataService.Journal_UpdateContentItemId(journalItem.JournalId, journalItem.ContentItemId);
        }
        else
        {
            var ci = cnt.CreateContentItem(journalItem, tabId, moduleId);
            this.dataService.Journal_UpdateContentItemId(journalItem.JournalId, ci.ContentItemId);
            journalItem.ContentItemId = ci.ContentItemId;
        }

        if (journalItem.SocialGroupId > 0)
        {
            try
            {
                this.UpdateGroupStats(journalItem.PortalId, journalItem.SocialGroupId);
            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.LogException(exc);
            }
        }
    }

    /// <summary>Update the journal info in database.</summary>
    /// <param name="journalItem">Journal object.</param>
    /// <param name="tabId">The ID of the tab of the journal item context.</param>
    /// <param name="moduleId">The ID of the module of the journal item context.</param>
    public void UpdateJournalItem(JournalItem journalItem, int tabId, int moduleId)
    {
        if (journalItem.UserId < 1)
        {
            throw new ArgumentException("journalItem.UserId must be for a real user");
        }

        UserInfo currentUser = UserController.GetUserById(journalItem.PortalId, journalItem.UserId);
        if (currentUser == null)
        {
            throw new UserDoesNotExistException("Unable to locate the current user");
        }

        string xml = null;
        if (!string.IsNullOrEmpty(journalItem.Title))
        {
            journalItem.Title = this.portalSecurity.InputFilter(journalItem.Title, PortalSecurity.FilterFlag.NoMarkup);
        }

        if (!string.IsNullOrEmpty(journalItem.Summary))
        {
            journalItem.Summary = this.portalSecurity.InputFilter(journalItem.Summary, PortalSecurity.FilterFlag.NoScripting);
        }

        if (!string.IsNullOrEmpty(journalItem.Body))
        {
            journalItem.Body = this.portalSecurity.InputFilter(journalItem.Body, PortalSecurity.FilterFlag.NoScripting);
        }

        if (!string.IsNullOrEmpty(journalItem.Body))
        {
            var xDoc = new XmlDocument { XmlResolver = null };
            var itemsNode = xDoc.CreateElement("items");
            var itemNode = xDoc.CreateElement("item");
            itemNode.AppendChild(CreateElement(xDoc, "id", "-1"));
            itemNode.AppendChild(CreateCDataElement(xDoc, "body", journalItem.Body));
            itemsNode.AppendChild(itemNode);
            xDoc.AppendChild(itemsNode);
            var xDec = xDoc.CreateXmlDeclaration("1.0", null, null);
            xDec.Encoding = "UTF-16";
            xDec.Standalone = "yes";
            var root = xDoc.DocumentElement;
            xDoc.InsertBefore(xDec, root);
            journalItem.JournalXML = xDoc;
            xml = journalItem.JournalXML.OuterXml;
        }

        if (journalItem.ItemData != null)
        {
            if (!string.IsNullOrEmpty(journalItem.ItemData.Title))
            {
                journalItem.ItemData.Title = this.portalSecurity.InputFilter(journalItem.ItemData.Title, PortalSecurity.FilterFlag.NoMarkup);
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.Description))
            {
                journalItem.ItemData.Description =
                    this.portalSecurity.InputFilter(journalItem.ItemData.Description, PortalSecurity.FilterFlag.NoScripting);
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.Url))
            {
                journalItem.ItemData.Url = this.portalSecurity.InputFilter(journalItem.ItemData.Url, PortalSecurity.FilterFlag.NoScripting);
            }

            if (!string.IsNullOrEmpty(journalItem.ItemData.ImageUrl))
            {
                journalItem.ItemData.ImageUrl = this.portalSecurity.InputFilter(journalItem.ItemData.ImageUrl, PortalSecurity.FilterFlag.NoScripting);
            }
        }

        var journalData = journalItem.ItemData.ToJson();
        if (journalData == "null")
        {
            journalData = null;
        }

        this.PrepareSecuritySet(journalItem);

        journalItem.JournalId = this.dataService.Journal_Update(
            journalItem.PortalId,
            journalItem.UserId,
            journalItem.ProfileId,
            journalItem.SocialGroupId,
            journalItem.JournalId,
            journalItem.JournalTypeId,
            journalItem.Title,
            journalItem.Summary,
            journalItem.Body,
            journalData,
            xml,
            journalItem.ObjectKey,
            journalItem.AccessKey,
            journalItem.SecuritySet,
            journalItem.CommentsDisabled,
            journalItem.CommentsHidden);

        var updatedJournalItem = this.GetJournalItem(journalItem.PortalId, journalItem.UserId, journalItem.JournalId);
        journalItem.DateCreated = updatedJournalItem.DateCreated;
        journalItem.DateUpdated = updatedJournalItem.DateUpdated;

        var cnt = new Content();
        if (journalItem.ContentItemId > 0)
        {
            cnt.UpdateContentItem(journalItem, tabId, moduleId);
            this.dataService.Journal_UpdateContentItemId(journalItem.JournalId, journalItem.ContentItemId);
        }
        else
        {
            var ci = cnt.CreateContentItem(journalItem, tabId, moduleId);
            this.dataService.Journal_UpdateContentItemId(journalItem.JournalId, ci.ContentItemId);
            journalItem.ContentItemId = ci.ContentItemId;
        }

        if (journalItem.SocialGroupId > 0)
        {
            try
            {
                this.UpdateGroupStats(journalItem.PortalId, journalItem.SocialGroupId);
            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.LogException(exc);
            }
        }
    }

    /// <inheritdoc/>
    public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId)
    {
        return this.GetJournalItem(portalId, currentUserId, journalId, false, false);
    }

    /// <inheritdoc/>
    public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId, bool includeAllItems)
    {
        return this.GetJournalItem(portalId, currentUserId, journalId, includeAllItems, false);
    }

    /// <inheritdoc/>
    public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId, bool includeAllItems, bool isDeleted)
    {
        return this.GetJournalItem(portalId, currentUserId, journalId, includeAllItems, isDeleted, false);
    }

    /// <inheritdoc/>
    public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId, bool includeAllItems, bool isDeleted, bool securityCheck)
    {
        return CBO.FillObject<JournalItem>(this.dataService.Journal_Get(portalId, currentUserId, journalId, includeAllItems, isDeleted, securityCheck));
    }

    /// <inheritdoc/>
    public JournalItem GetJournalItemByKey(int portalId, string objectKey)
    {
        return this.GetJournalItemByKey(portalId, objectKey, false, false);
    }

    /// <inheritdoc/>
    public JournalItem GetJournalItemByKey(int portalId, string objectKey, bool includeAllItems)
    {
        return this.GetJournalItemByKey(portalId, objectKey, includeAllItems, false);
    }

    /// <inheritdoc/>
    public JournalItem GetJournalItemByKey(int portalId, string objectKey, bool includeAllItems, bool isDeleted)
    {
        if (string.IsNullOrEmpty(objectKey))
        {
            return null;
        }

        return CBO.FillObject<JournalItem>(this.dataService.Journal_GetByKey(portalId, objectKey, includeAllItems, isDeleted));
    }

    /// <inheritdoc/>
    public IFileInfo SaveJourmalFile(ModuleInfo module, UserInfo userInfo, string fileName, Stream fileContent)
    {
        var userFolder = this.folderManager.GetUserFolder(userInfo);

        Stream fileContentStream = null;
        try
        {
            if (IsImageFile(fileName) && IsResizePhotosEnabled(module))
            {
                fileContentStream = GetJournalImageContent(fileContent);
            }

            return this.fileManager.AddFile(userFolder, fileName, fileContentStream ?? fileContent, true, true, this.fileContentTypeManager.GetContentType(Path.GetExtension(fileName)));
        }
        finally
        {
            fileContentStream?.Dispose();
        }
    }

    /// <inheritdoc/>
    public void SaveJournalItem(JournalItem journalItem, ModuleInfo module)
    {
        var tabId = module?.TabID ?? Null.NullInteger;
        var tabModuleId = module?.TabModuleID ?? Null.NullInteger;

        this.SaveJournalItem(journalItem, tabId, tabModuleId);
    }

    /// <inheritdoc/>
    public void UpdateJournalItem(JournalItem journalItem, ModuleInfo module)
    {
        var tabId = module?.TabID ?? Null.NullInteger;
        var tabModuleId = module?.TabModuleID ?? Null.NullInteger;

        this.UpdateJournalItem(journalItem, tabId, tabModuleId);
    }

    /// <inheritdoc/>
    public void DisableComments(int portalId, int journalId)
    {
        this.dataService.Journal_Comments_ToggleDisable(portalId, journalId, true);
    }

    /// <inheritdoc/>
    public void EnableComments(int portalId, int journalId)
    {
        this.dataService.Journal_Comments_ToggleDisable(portalId, journalId, false);
    }

    /// <inheritdoc/>
    public void HideComments(int portalId, int journalId)
    {
        this.dataService.Journal_Comments_ToggleHidden(portalId, journalId, true);
    }

    /// <inheritdoc/>
    public void ShowComments(int portalId, int journalId)
    {
        this.dataService.Journal_Comments_ToggleHidden(portalId, journalId, false);
    }

    // Delete Journal Items

    /// <summary>HARD deletes journal items.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="currentUserId">The user ID.</param>
    /// <param name="journalId">The journal ID.</param>
    public void DeleteJournalItem(int portalId, int currentUserId, int journalId)
    {
        this.DeleteJournalItem(portalId, currentUserId, journalId, false);
    }

    /// <summary>HARD deletes journal items based on item key.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="objectKey">The key.</param>
    public void DeleteJournalItemByKey(int portalId, string objectKey)
    {
        this.dataService.Journal_DeleteByKey(portalId, objectKey);
    }

    /// <summary>HARD deletes journal items based on group ID.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="groupId">The group ID.</param>
    public void DeleteJournalItemByGroupId(int portalId, int groupId)
    {
        this.dataService.Journal_DeleteByGroupId(portalId, groupId);
    }

    /// <summary>SOFT deletes journal items.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="currentUserId">The user ID.</param>
    /// <param name="journalId">The journal ID.</param>
    public void SoftDeleteJournalItem(int portalId, int currentUserId, int journalId)
    {
        this.DeleteJournalItem(portalId, currentUserId, journalId, true);
    }

    /// <summary>SOFT deletes journal items based on item key.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="objectKey">The key.</param>
    public void SoftDeleteJournalItemByKey(int portalId, string objectKey)
    {
        this.dataService.Journal_SoftDeleteByKey(portalId, objectKey);
    }

    /// <summary>SOFT deletes journal items based on group ID.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="groupId">The group ID.</param>
    public void SoftDeleteJournalItemByGroupId(int portalId, int groupId)
    {
        this.dataService.Journal_SoftDeleteByGroupId(portalId, groupId);
    }

    /// <inheritdoc/>
    public IList<CommentInfo> GetCommentsByJournalIds(List<int> journalIdList)
    {
        var journalIds = journalIdList.Aggregate(string.Empty, (current, journalId) => current + journalId + ";");
        return CBO.FillCollection<CommentInfo>(this.dataService.Journal_Comment_ListByJournalIds(journalIds));
    }

    /// <inheritdoc/>
    public void LikeJournalItem(int journalId, int userId, string displayName)
    {
        this.dataService.Journal_Like(journalId, userId, displayName);
    }

    /// <inheritdoc/>
    public void SaveComment(CommentInfo comment)
    {
        if (!string.IsNullOrEmpty(comment.Comment))
        {
            comment.Comment =
                this.portalSecurity.InputFilter(comment.Comment, PortalSecurity.FilterFlag.NoScripting);
        }

        // TODO: enable once the profanity filter is working properly.
        // objCommentInfo.Comment = portalSecurity.Remove(objCommentInfo.Comment, DotNetNuke.Security.PortalSecurity.ConfigType.ListController, "ProfanityFilter", DotNetNuke.Security.PortalSecurity.FilterScope.PortalList);
        string xml = null;
        if (comment.CommentXML != null)
        {
            xml = comment.CommentXML.OuterXml;
        }

        comment.CommentId = this.dataService.Journal_Comment_Save(comment.JournalId, comment.CommentId, comment.UserId, comment.Comment, xml, Null.NullDate);

        var newComment = this.GetComment(comment.CommentId);
        comment.DateCreated = newComment.DateCreated;
        comment.DateUpdated = newComment.DateUpdated;
    }

    /// <inheritdoc/>
    public CommentInfo GetComment(int commentId)
    {
        return CBO.FillObject<CommentInfo>(this.dataService.Journal_Comment_Get(commentId));
    }

    /// <inheritdoc/>
    public void DeleteComment(int journalId, int commentId)
    {
        this.dataService.Journal_Comment_Delete(journalId, commentId);

        // UNDONE: update the parent journal item and content item so this comment gets removed from search index
    }

    /// <inheritdoc/>
    public void LikeComment(int journalId, int commentId, int userId, string displayName)
    {
        this.dataService.Journal_Comment_Like(journalId, commentId, userId, displayName);
    }

    /// <inheritdoc/>
    public JournalTypeInfo GetJournalType(string journalType)
    {
        return CBO.FillObject<JournalTypeInfo>(this.dataService.Journal_Types_Get(journalType));
    }

    /// <inheritdoc/>
    public JournalTypeInfo GetJournalTypeById(int journalTypeId)
    {
        return CBO.FillObject<JournalTypeInfo>(this.dataService.Journal_Types_GetById(journalTypeId));
    }

    /// <inheritdoc/>
    public IEnumerable<JournalTypeInfo> GetJournalTypes(int portalId)
    {
        return CBO.GetCachedObject<IEnumerable<JournalTypeInfo>>(
            this.hostSettings,
            new CacheItemArgs(
                string.Format(CultureInfo.InvariantCulture, DataCache.JournalTypesCacheKey, portalId),
                DataCache.JournalTypesTimeOut,
                DataCache.JournalTypesCachePriority,
                portalId),
            _ => CBO.FillCollection<JournalTypeInfo>(this.dataService.Journal_Types_List(portalId)));
    }

    private static XmlElement CreateElement(XmlDocument xDoc, string name, string value)
    {
        var element = xDoc.CreateElement(name);
        var xtext = xDoc.CreateTextNode(value);
        element.AppendChild(xtext);
        return element;
    }

    private static XmlElement CreateCDataElement(XmlDocument xDoc, string name, string value)
    {
        var element = xDoc.CreateElement(name);
        var xdata = xDoc.CreateCDataSection(value);
        element.AppendChild(xdata);
        return element;
    }

    private static MemoryStream GetJournalImageContent(Stream fileContent)
    {
        Image image = new Bitmap(fileContent);
        var thumbnailWidth = 400;
        var thumbnailHeight = 400;
        GetThumbnailSize(image.Width, image.Height, ref thumbnailWidth, ref thumbnailHeight);
        var thumbnail = image.GetThumbnailImage(thumbnailWidth, thumbnailHeight, () => true, IntPtr.Zero);
        var result = new MemoryStream();
        thumbnail.Save(result, image.RawFormat);
        return result;
    }

    private static void GetThumbnailSize(int imageWidth, int imageHeight, ref int thumbnailWidth, ref int thumbnailHeight)
    {
        if (imageWidth >= imageHeight)
        {
            thumbnailWidth = Math.Min(imageWidth, thumbnailWidth);
            thumbnailHeight = GetMinorSize(imageHeight, imageWidth, thumbnailWidth);
        }
        else
        {
            thumbnailHeight = Math.Min(imageHeight, thumbnailHeight);
            thumbnailWidth = GetMinorSize(imageWidth, imageHeight, thumbnailHeight);
        }
    }

    private static int GetMinorSize(int imageMinorSize, int imageMajorSize, int thumbnailMajorSize)
    {
        if (imageMajorSize == thumbnailMajorSize)
        {
            return imageMinorSize;
        }

        double calculated = (Convert.ToDouble(imageMinorSize) * Convert.ToDouble(thumbnailMajorSize)) / Convert.ToDouble(imageMajorSize);
        return Convert.ToInt32(Math.Round(calculated));
    }

    private static bool IsImageFile(string fileName)
    {
        return (Globals.ImageFileTypes + ",").IndexOf(Path.GetExtension(fileName).Replace(".", string.Empty) + ",", StringComparison.InvariantCultureIgnoreCase) > -1;
    }

    private static bool IsResizePhotosEnabled(ModuleInfo module)
    {
        return GetBooleanSetting(AllowResizePhotosSetting, false, module) &&
               GetBooleanSetting(AllowPhotosSetting, true, module) &&
               GetBooleanSetting(EditorEnabledSetting, true, module);
    }

    private static bool GetBooleanSetting(string settingName, bool defaultValue, ModuleInfo module)
    {
        if (module.ModuleSettings.Contains(settingName))
        {
            return Convert.ToBoolean(module.ModuleSettings[settingName].ToString());
        }

        return defaultValue;
    }

    // none of the parameters should be null; checked before calling this method
    private void PrepareSecuritySet(JournalItem journalItem)
    {
        var originalSecuritySet =
            journalItem.SecuritySet = (journalItem.SecuritySet ?? string.Empty).ToUpperInvariant();

        if (string.IsNullOrEmpty(journalItem.SecuritySet))
        {
            journalItem.SecuritySet = "E,";
        }
        else if (!journalItem.SecuritySet.EndsWith(",", StringComparison.Ordinal))
        {
            journalItem.SecuritySet += ",";
            originalSecuritySet = journalItem.SecuritySet;
        }

        if (journalItem.SecuritySet == "F,")
        {
            journalItem.SecuritySet = "F" + journalItem.UserId + ",";
            if (journalItem.ProfileId > 0)
            {
                journalItem.SecuritySet += "P" + journalItem.ProfileId + ",";
            }
        }
        else if (journalItem.SecuritySet == "U,")
        {
            journalItem.SecuritySet += "U" + journalItem.UserId + ",";
        }
        else if (journalItem.SecuritySet == "R,")
        {
            if (journalItem.SocialGroupId > 0)
            {
                journalItem.SecuritySet += "R" + journalItem.SocialGroupId + ",";
            }
        }

        if (journalItem.ProfileId > 0 && journalItem.UserId != journalItem.ProfileId)
        {
            if (!journalItem.SecuritySet.Contains("P" + journalItem.ProfileId + ","))
            {
                journalItem.SecuritySet += "P" + journalItem.ProfileId + ",";
            }

            if (!journalItem.SecuritySet.Contains("U" + journalItem.UserId + ","))
            {
                journalItem.SecuritySet += "U" + journalItem.UserId + ",";
            }
        }

        if (!journalItem.SecuritySet.Contains("U" + journalItem.UserId + ","))
        {
            journalItem.SecuritySet += "U" + journalItem.UserId + ",";
        }

        // if the post is marked as private, we shouldn't make it visible to the group.
        if (journalItem.SocialGroupId > 0 && originalSecuritySet.Contains("U,"))
        {
            var item = journalItem;
            var role = this.roleController.GetRole(
                journalItem.PortalId,
                r => r.SecurityMode != SecurityMode.SecurityRole && r.RoleID == item.SocialGroupId);

            if (role != null && !role.IsPublic)
            {
                journalItem.SecuritySet = journalItem.SecuritySet.Replace("E,", string.Empty).Replace("C,", string.Empty);
            }
        }

        // clean up and remove duplicates
        var parts = journalItem.SecuritySet
            .Replace(" ", string.Empty)
            .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            .Distinct()
            .Except(InvalidSecuritySetsWithoutId)
            .Where(p => p.IndexOfAny(ValidSecurityDescriptors) >= 0);

        // TODO: validate existence and visibility/accessability of all Roles added to the set (if any)
        journalItem.SecuritySet = string.Join(",", parts);
    }

    private void UpdateGroupStats(int portalId, int groupId)
    {
        var role = this.roleController.GetRole(portalId, r => r.RoleID == groupId);
        if (role is null)
        {
            return;
        }

        for (var i = 0; i < role.Settings.Keys.Count; i++)
        {
            var key = role.Settings.Keys.ElementAt(i);
            if (key.StartsWith("stat_", StringComparison.OrdinalIgnoreCase))
            {
                role.Settings[key] = "0";
            }
        }

        using (var dr = this.dataService.Journal_GetStatsForGroup(portalId, groupId))
        {
            while (dr.Read())
            {
                var settingName = "stat_" + dr["JournalType"];
                if (role.Settings.ContainsKey(settingName))
                {
                    role.Settings[settingName] = dr["JournalTypeCount"].ToString();
                }
                else
                {
                    role.Settings.Add(settingName, dr["JournalTypeCount"].ToString());
                }
            }

            dr.Close();
        }

        this.roleController.UpdateRoleSettings(role, true);
    }

    private void DeleteJournalItem(int portalId, int currentUserId, int journalId, bool softDelete)
    {
        var ji = this.GetJournalItem(portalId, currentUserId, journalId, !softDelete);
        if (ji == null)
        {
            return;
        }

        var groupId = ji.SocialGroupId;

        if (softDelete)
        {
            this.dataService.Journal_SoftDelete(journalId);
        }
        else
        {
            this.dataService.Journal_Delete(journalId);
        }

        if (groupId > 0)
        {
            this.UpdateGroupStats(portalId, groupId);
        }

        // queue remove journal from search index
        var document = new SearchDocumentToDelete
        {
            PortalId = portalId,
            AuthorUserId = currentUserId,
            UniqueKey = ji.ContentItemId.ToString("D", CultureInfo.InvariantCulture),

            // QueryString = "journalid=" + journalId,
            SearchTypeId = this.searchHelper.GetSearchTypeByName("module").SearchTypeId,
        };

        if (groupId > 0)
        {
            document.RoleId = groupId;
        }

        this.dataProvider.AddSearchDeletedItems(document);
    }
}
