// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Dnn.DynamicContent;
using DotNetNuke.Common.Utilities;

namespace Dnn.Tests.DynamicContent.UnitTests.Builders
{
    class DynamicContentTypeBuilder
    {
        private int _createdByUserId = -1;
        private int _lastModifiedByUserId = -1;
        private DateTime _createdOnDate = DateTime.Now;
        private DateTime _lastModifiedOnDate = DateTime.Now;
        private int _contentTypeId = 8;
        private string _description = "A simple Content Type that describes an employee";
        private bool _isDynamic = true;
        private string _name = "Employee";
        private int _portalId = Null.NullInteger;

        public DynamicContentTypeBuilder WithCreatedByUserId(int createdByUserId)
        {
            _createdByUserId = createdByUserId;
            return this;
        }

        public DynamicContentTypeBuilder WithLastModifiedByUserId(int lastModifiedByUserId)
        {
            _lastModifiedByUserId = lastModifiedByUserId;
            return this;
        }

        public DynamicContentTypeBuilder WithCreatedOnDate(DateTime createdOnDate)
        {
            _createdOnDate = createdOnDate;
            return this;
        }

        public DynamicContentTypeBuilder WithLastModifiedOnDate(DateTime lastModifiedOnDate)
        {
            _lastModifiedOnDate = lastModifiedOnDate;
            return this;
        }

        public DynamicContentTypeBuilder WithContentTypeId(int contentTypeId)
        {
            _contentTypeId = contentTypeId;
            return this;
        }

        public DynamicContentTypeBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public DynamicContentTypeBuilder WithIsDynamic(bool isDynamic)
        {
            _isDynamic = isDynamic;
            return this;
        }

        public DynamicContentTypeBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public DynamicContentTypeBuilder WithPortalId(int portalId)
        {
            _portalId = portalId;
            return this;
        }

        public DynamicContentType Build()
        {
            return new DynamicContentType
            {
                CreatedByUserId = _createdByUserId,
                LastModifiedByUserId = _lastModifiedByUserId,
                CreatedOnDate = _createdOnDate,
                LastModifiedOnDate = _lastModifiedOnDate,
                ContentTypeId = _contentTypeId,
                Description = _description,
                IsDynamic = _isDynamic,
                Name = _name,
                PortalId = _portalId
            };
        }
    }
}
