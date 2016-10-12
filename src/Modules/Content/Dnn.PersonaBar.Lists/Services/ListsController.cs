#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Lists.Services.DTO;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Web.Api;
using ListEntryInfoDto = Dnn.PersonaBar.Lists.Services.DTO.ListEntryInfoDto;
using ListInfoDto = Dnn.PersonaBar.Lists.Services.DTO.ListInfoDto;

namespace Dnn.PersonaBar.Lists.Services
{
    [ServiceScope(Scope = ServiceScope.AdminHost)]
    public class ListsController : PersonaBarApiController
    {
        #region Properties

        private ListController _listController = new ListController();

        #endregion

        #region API

        #region Lists API

        [HttpGet]
        public HttpResponseMessage GetLists(int portalId)
        {
            var lists = _listController.GetListInfoCollection(string.Empty, string.Empty, portalId)
                .Cast<ListInfo>()
                .Select(ConvertListInfoToDto);

            return Request.CreateResponse(HttpStatusCode.OK, lists);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteList(DeleteListDto deleteList)
        {
            _listController.DeleteList(deleteList.Name, deleteList.ParentKey, deleteList.PortalId);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        #region List Entries API

        [HttpGet]
        public HttpResponseMessage GetListEntries(int portalId, string parentKey, string listName)
        {
            var entries = _listController.GetListEntryInfoItems(listName, parentKey, portalId)
                .Select(ConvertListEntryInfoToDto);

            return Request.CreateResponse(HttpStatusCode.OK, entries);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AddListEntry(ListEntryInfoDto list)
        {
            var entry = ConvertDtoToListEntryInfo(list);
            _listController.AddListEntry(entry);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteListEntry(DeleteListEntryDto deleteListEntry)
        {
            _listController.DeleteListEntryByID(deleteListEntry.Id, true);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        #endregion

        #region Private Methods

        private ListInfo ConvertDtoToListInfo(ListInfoDto dto)
        {
            return new ListInfo()
            {
                Name = dto.Name,
                ParentID = dto.ParentId,
                ParentKey = dto.ParentKey,
                Parent = dto.Parent,
                ParentList = dto.ParentList,
                Level = dto.Level,
                DefinitionID = dto.DefinitionId,
                PortalID = dto.PortalId,
                IsPopulated = dto.IsPopulated,
                EntryCount = dto.EntryCount,
                EnableSortOrder = dto.EnableSort
            };
        }

        private ListInfoDto ConvertListInfoToDto(ListInfo listInfo)
        {
            return new ListInfoDto()
            {
                Key = listInfo.Key,
                Name = listInfo.Name,
                DisplayName = listInfo.DisplayName,
                ParentId = listInfo.ParentID,
                ParentKey = listInfo.ParentKey,
                Parent = listInfo.Parent,
                ParentList = listInfo.ParentList,
                Level = listInfo.Level,
                DefinitionId = listInfo.DefinitionID,
                PortalId = listInfo.PortalID,
                IsPopulated = listInfo.IsPopulated,
                EntryCount = listInfo.EntryCount,
                EnableSort= listInfo.EnableSortOrder 
            };
        }

        private ListEntryInfo ConvertDtoToListEntryInfo(ListEntryInfoDto dto)
        {
            return new ListEntryInfo()
            {
                ListName = dto.ListName,
                ParentID = dto.ParentId,
                ParentKey = dto.ParentKey,
                Parent = dto.Parent,
                Level = dto.Level,
                DefinitionID = dto.DefinitionId,
                PortalID = dto.PortalId,
                SystemList = dto.IsSystem,
                SortOrder = dto.Sort,
                Description = dto.Description,
                EntryID = dto.Id,
                HasChildren = dto.HasChildren,
                Text = dto.Text,
                Value = dto.Value
            };
        }

        private ListEntryInfoDto ConvertListEntryInfoToDto(ListEntryInfo info)
        {
            return new ListEntryInfoDto()
            {
                Key = info.Key,
                ListName = info.ListName,
                DisplayName = info.DisplayName,
                ParentId = info.ParentID,
                ParentKey = info.ParentKey,
                Parent = info.Parent,
                Level = info.Level,
                DefinitionId = info.DefinitionID,
                PortalId = info.PortalID,
                IsSystem = info.SystemList,
                Sort = info.SortOrder,
                Description = info.Description,
                Id = info.EntryID,
                HasChildren = info.HasChildren,
                Text = info.TextNonLocalized,
                LocalizedText = info.Text,
                Value = info.Value
            };
        }

        #endregion
    }
}
