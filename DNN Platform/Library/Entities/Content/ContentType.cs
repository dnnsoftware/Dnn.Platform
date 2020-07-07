// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content
{
    using System;
    using System.Data;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// Content type of a content item.
    /// </summary>
    /// <remarks>
    /// Content Types, simply put, are a way of telling the framework what module/functionality is associated with a Content Item.
    /// Each product (ie. module) that wishes to allow categorization of data (via Taxonomy or Folksonomy) for it's content items
    ///  will likely need to create its own content type.
    /// </remarks>
    [Serializable]
    public class ContentType : ContentTypeMemberNameFixer, IHydratable
    {
        private const string desktopModuleContentTypeName = "DesktopModule";
        private const string moduleContentTypeName = "Module";
        private const string tabContentTypeName = "Tab";

        private static ContentType _desktopModule;
        private static ContentType _module;
        private static ContentType _tab;

        public ContentType()
            : this(Null.NullString)
        {
        }

        public ContentType(string contentType)
        {
            this.ContentTypeId = Null.NullInteger;
            this.ContentType = contentType;
        }

        public static ContentType DesktopModule
        {
            get
            {
                return _desktopModule ?? (_desktopModule = new ContentTypeController().GetContentTypes().FirstOrDefault(type => type.ContentType == desktopModuleContentTypeName));
            }
        }

        public static ContentType Module
        {
            get
            {
                return _module ?? (_module = new ContentTypeController().GetContentTypes().FirstOrDefault(type => type.ContentType == moduleContentTypeName));
            }
        }

        public static ContentType Tab
        {
            get
            {
                return _tab ?? (_tab = new ContentTypeController().GetContentTypes().FirstOrDefault(type => type.ContentType == tabContentTypeName));
            }
        }

        /// <summary>
        /// Gets or sets the content type id.
        /// </summary>
        /// <value>
        /// The content type id.
        /// </value>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// Gets or sets the key ID.
        /// </summary>
        /// <value>
        /// ContentTypeID.
        /// </value>
        public int KeyID
        {
            get
            {
                return this.ContentTypeId;
            }

            set
            {
                this.ContentTypeId = value;
            }
        }

        /// <summary>
        /// Fill this content object will the information from data reader.
        /// </summary>
        /// <param name="dr">The data reader.</param>
        /// <seealso cref="IHydratable.Fill"/>
        public void Fill(IDataReader dr)
        {
            this.ContentTypeId = Null.SetNullInteger(dr["ContentTypeID"]);
            this.ContentType = Null.SetNullString(dr["ContentType"]);
        }

        /// <summary>
        /// override ToString to return content type.
        /// </summary>
        /// <returns>
        /// property ContentType's value.
        /// </returns>
        public override string ToString()
        {
            return this.ContentType;
        }
    }
}
