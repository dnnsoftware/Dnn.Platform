#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.FileSystem;

#endregion

namespace DotNetNuke.Entities.Content
{
	/// <summary>
	/// The ContentItem class which itself inherits from BaseEntityInfo paves the way for easily adding support for taxonomy,
	/// tagging and other ContentItem dependant features to your DotNetNuke extensions.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Content Items are a collection of individual pieces of content in a DotNetNuke site. Each content item is associated with a single Content Type.
	/// </para>
	/// <para>
	/// Only modules that implement content items (done so by the module developers) can take advantage of some of its benefits, such as Taxonomy. 
	/// </para>
	/// <para>
	/// Because ContentItem already implements IHydratable, you will not do so in your custom entity class. Instead, 
	/// you will need to create overrides of the KeyID property and the Fill method already implemented in the ContentItem class. 
	/// Don't forget to call ContentItem's FillInternal method in your Fill method override.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code lang="C#">
	/// [Serializable]
    /// public class DesktopModuleInfo : ContentItem, IXmlSerializable
    /// {
    ///		#region IHydratable Members
	///
    ///		public override void Fill(IDataReader dr)
    ///		{
    ///		    DesktopModuleID = Null.SetNullInteger(dr["DesktopModuleID"]);
    ///		    PackageID = Null.SetNullInteger(dr["PackageID"]);
    ///		    ModuleName = Null.SetNullString(dr["ModuleName"]);
    ///		    FriendlyName = Null.SetNullString(dr["FriendlyName"]);
    ///		    Description = Null.SetNullString(dr["Description"]);
    ///		    FolderName = Null.SetNullString(dr["FolderName"]);
    ///		    Version = Null.SetNullString(dr["Version"]);
    ///		    base.FillInternal(dr);
    ///		}
	///		
	///		#endregion
    /// }
	/// </code>
	/// </example>
	[Serializable]
    public class ContentItem : BaseEntityInfo, IHydratable
    {
        private NameValueCollection _metadata;

        private List<Term> _terms;

	    private List<IFileInfo> _files;
	    private List<IFileInfo> _videos;
	    private List<IFileInfo> _images;
        
        public ContentItem()
        {
            TabID = Null.NullInteger;
            ModuleID = Null.NullInteger;
            ContentTypeId = Null.NullInteger;
            ContentItemId = Null.NullInteger;
            StateID = Null.NullInteger;
        }

        #region Public Properties

		/// <summary>
		/// Gets or sets the content item id.
		/// </summary>
		/// <value>
		/// The content item id.
		/// </value>
        [XmlIgnore]
        [ScriptIgnore]
        public int ContentItemId { get; set; }

		/// <summary>
		/// Gets or sets the content.
		/// </summary>
		/// <value>
		/// The content.
		/// </value>
        [XmlElement("content")]
        public string Content { get; set; }

		/// <summary>
		/// Gets or sets the content key.
		/// </summary>
		/// <value>
		/// The content key.
		/// </value>
		[XmlElement("contentKey")]
        public string ContentKey { get; set; }

		/// <summary>
		/// Gets or sets the content type id.
		/// </summary>
		/// <value>
		/// The content type id.
		/// </value>
		/// <see cref="ContentType"/>
        [XmlIgnore]
        [ScriptIgnore]
        public int ContentTypeId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ContentItem"/> is indexed.
		/// </summary>
		/// <value>
		///   <c>true</c> if indexed; otherwise, <c>false</c>.
		/// </value>
        [XmlIgnore]
        [ScriptIgnore]
        public bool Indexed { get; set; }

		/// <summary>
		/// Gets the metadata.
		/// </summary>
		/// <value>metadata collection</value>
        [XmlIgnore]
        [ScriptIgnore]
        public NameValueCollection Metadata
        {
            get
            {
                return _metadata ?? (_metadata = this.GetMetaData(ContentItemId));
            }
        }

		/// <summary>
		/// Gets or sets the module ID.
		/// </summary>
		/// <value>
		/// The module ID.
		/// </value>
        [XmlElement("moduleID")]
        public int ModuleID { get; set; }

		/// <summary>
		/// Gets or sets the tab ID.
		/// </summary>
		/// <value>
		/// The tab ID.
		/// </value>
        [XmlElement("tabid")]
        public int TabID { get; set; }

		/// <summary>
		/// Gets the terms.
		/// </summary>
		/// <value>Terms Collection</value>
        [XmlIgnore]
        [ScriptIgnore]
        public List<Term> Terms
        {
            get
            {
                return _terms ?? (_terms = this.GetTerms(ContentItemId));
            }
        }

        /// <summary>The title of the ContentItem.</summary>
	    [XmlElement("contentTitle")]
	    public string ContentTitle
	    {
	        get
	        {
	            return Metadata[AttachmentController.TitleKey];
	        }
            set
            {
                Metadata[AttachmentController.TitleKey] = value;
            }
	    }

	    /// <summary>
	    /// Files that are attached to this ContentItem.
	    /// </summary>
	    [XmlIgnore]
        [ScriptIgnore]
	    public List<IFileInfo> Files
	    {
	        get { return _files ?? (_files = AttachmentController.DeserializeFileInfo(Metadata[AttachmentController.FilesKey]).ToList()); }
	    }

	    /// <summary>
        /// Video files attached to this ContentItem.
        /// </summary>
        [XmlIgnore]
        [ScriptIgnore]
        public List<IFileInfo> Videos
	    {
            get { return _videos ?? (_videos = AttachmentController.DeserializeFileInfo(Metadata[AttachmentController.VideoKey]).ToList()); }
	    }

        /// <summary>
        /// Images associated with this ContentItem.
        /// </summary>
        [XmlIgnore]
        [ScriptIgnore]
        public List<IFileInfo> Images
	    {
            get { return _images ?? (_images = AttachmentController.DeserializeFileInfo(Metadata[AttachmentController.ImageKey]).ToList()); }
	    }

        /// <summary>
        /// Gets or sets the Content Workflow State ID.
        /// </summary>
        /// <value>
        /// The Content Workflow State ID.
        /// </value>
        [XmlIgnore]
        public int StateID { get; set; }

        #endregion

        #region Protected Methods

		/// <summary>
		/// Fills the internal.
		/// </summary>
		/// <param name="dr">The data reader contains module information.</param>
		/// <remarks>
		/// Please remember to call base.FillInternal or base.Fill method in your Fill method.
		/// </remarks>
        protected override void FillInternal(IDataReader dr)
        {
            base.FillInternal(dr);

            ContentItemId = Null.SetNullInteger(dr["ContentItemID"]);
            Content = Null.SetNullString(dr["Content"]);
            ContentTypeId = Null.SetNullInteger(dr["ContentTypeID"]);
            TabID = Null.SetNullInteger(dr["TabID"]);
            ModuleID = Null.SetNullInteger(dr["ModuleID"]);
            ContentKey = Null.SetNullString(dr["ContentKey"]);
            Indexed = Null.SetNullBoolean(dr["Indexed"]);

            var schema = dr.GetSchemaTable();
            if (schema != null)
            {
                if (schema.Select("ColumnName = 'StateID'").Length > 0)
                {
                    StateID = Null.SetNullInteger(dr["StateID"]);
                }
            }
        }

		protected void Clone(ContentItem cloneItem, ContentItem originalItem)
		{
			CloneBaseProperties(cloneItem, originalItem);

			cloneItem.ContentItemId = originalItem.ContentItemId;
			cloneItem.Content = originalItem.Content;
			cloneItem.ContentTypeId = originalItem.ContentTypeId;
			cloneItem.TabID = originalItem.TabID;
			cloneItem.ModuleID = originalItem.ModuleID;
			cloneItem.ContentKey = originalItem.ContentKey;
            cloneItem.Indexed = originalItem.Indexed;
            cloneItem.StateID = originalItem.StateID;
		}
        
	    #endregion

        #region IHydratable Implementation

		/// <summary>
		/// Fill this content object will the information from data reader.
		/// </summary>
		/// <param name="dr">The data reader.</param>
		/// <seealso cref="IHydratable.Fill"/>
        public virtual void Fill(IDataReader dr)
        {
            FillInternal(dr);
        }

		/// <summary>
		/// Gets or sets the key ID.
		/// </summary>
		/// <value>
		/// The key ID.
		/// </value>
		/// <remarks>
		/// If you derive class has its own key id, please override this property and set the value to your own key id.
		/// </remarks>
        [XmlIgnore]
        public virtual int KeyID
        {
            get
            {
                return ContentItemId;
            }
            set
            {
                ContentItemId = value;
            }
        }

        #endregion
    }
}