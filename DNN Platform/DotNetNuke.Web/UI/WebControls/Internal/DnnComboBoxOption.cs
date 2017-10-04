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
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls.Extensions;

#endregion

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    ///<remarks>
    /// This class is added only for internal use, please don't reference it in any other places as it may removed in future.
    /// </remarks>
    [DataContract]
    public class DnnComboBoxOption
    {
        [DataMember(Name = "valueField")]
        public string ValueField { get; } = "value";

        [DataMember(Name = "labelField")]
        public string LabelField { get; } = "text";

        [DataMember(Name = "searchField")]
        public string SearchField { get; } = "text";

        [DataMember(Name = "create")]
        public bool Create { get; set; } = false;

        [DataMember(Name = "preload")]
        public string Preload { get; set; }

        [DataMember(Name = "highlight")]
        public bool Highlight { get; set; }

        [DataMember(Name = "allowEmptyOption")]
        public bool AllowEmptyOption { get; set; }

        [DataMember(Name = "plugins")]
        public IList<string> Plugins { get; set; } = new List<string>();

        [DataMember(Name = "checkbox")]
        public bool Checkbox { get; set; }

        [DataMember(Name = "maxOptions")]
        public int MaxOptions { get; set; }

        [DataMember(Name = "maxItems")]
        public int MaxItems { get; set; }

        [IgnoreDataMember]
        public IEnumerable<ListItem> Items { get; set; }

        [DataMember(Name = "options")]
        public IEnumerable<OptionItem> Options
        {
            get { return Items?.Select(i => new OptionItem {Text = i.Text, Value = i.Value, Selected = i.Selected}); }
        }

        [DataMember(Name = "localization")]
        public IDictionary<string, string> Localization { get; set; } = new Dictionary<string, string>();

        #region Events

        [DataMember(Name = "render")]
        public RenderOption Render { get; set; }

        [DataMember(Name = "load")]
        public string Load { get; set; }

        [DataMember(Name = "onChange")]
        public string OnChangeEvent { get; set; }

        #endregion
    }

    [DataContract]
    public class RenderOption
    {
        [DataMember(Name = "option")]
        public string Option { get; set; }
    }

    [DataContract]
    public class OptionItem
    {
        public OptionItem()
        {
            
        }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }

        [DataMember(Name = "selected")]
        public bool Selected { get; set; }
    }
}