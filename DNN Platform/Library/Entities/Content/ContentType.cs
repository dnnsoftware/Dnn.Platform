#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Data;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Entities.Content
{
    /// <summary>
    /// This class exists solely to maintain compatibility between the original VB version
    /// which supported ContentType.ContentType and the c# version which doesn't allow members with
    /// the same naem as their parent type
    /// </summary>
    [Serializable]
    public abstract class ContentTypeMemberNameFixer
    {
        public string ContentType { get; set; }
    }

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
        #region Private Members

        private static ContentType _desktopModule;
        private static ContentType _module;
        private static ContentType _tab;

        private const string desktopModuleContentTypeName = "DesktopModule";
        private const string moduleContentTypeName = "Module";
        private const string tabContentTypeName = "Tab";

        #endregion

        #region Constructors

        public ContentType() : this(Null.NullString)
        {
        }

        public ContentType(string contentType)
        {
            ContentTypeId = Null.NullInteger;
            ContentType = contentType;
        }

        #endregion

        #region Public Static Properties

        public static ContentType DesktopModule
	    {
	        get
	        {
	            return _desktopModule ?? (_desktopModule = new ContentTypeController().GetContentTypes().FirstOrDefault(type => type.ContentType ==  desktopModuleContentTypeName));
	        }
	    }

	    public static ContentType Module
	    {
	        get
	        {
	            return _module ?? (_module = new ContentTypeController().GetContentTypes().FirstOrDefault(type => type.ContentType ==  moduleContentTypeName));
	        }
	    }

        public static ContentType Tab 
        {
            get
            {
                return _tab ?? (_tab = new ContentTypeController().GetContentTypes().FirstOrDefault(type => type.ContentType == tabContentTypeName));
            }
        }

        #endregion

        /// <summary>
		/// Gets or sets the content type id.
		/// </summary>
		/// <value>
		/// The content type id.
		/// </value>
        public int ContentTypeId { get; set; }

        #region IHydratable Implementation

		/// <summary>
		/// Fill this content object will the information from data reader.
		/// </summary>
		/// <param name="dr">The data reader.</param>
		/// <seealso cref="IHydratable.Fill"/>
        public void Fill(IDataReader dr)
        {
            ContentTypeId = Null.SetNullInteger(dr["ContentTypeID"]);
            ContentType = Null.SetNullString(dr["ContentType"]);
        }

		/// <summary>
		/// Gets or sets the key ID.
		/// </summary>
		/// <value>
		/// ContentTypeID
		/// </value>
        public int KeyID
        {
            get
            {
                return ContentTypeId;
            }
            set
            {
                ContentTypeId = value;
            }
        }

        #endregion

		/// <summary>
		/// override ToString to return content type
		/// </summary>
		/// <returns>
		/// property ContentType's value.
		/// </returns>
        public override string ToString()
        {
            return ContentType;
        }
    }
}