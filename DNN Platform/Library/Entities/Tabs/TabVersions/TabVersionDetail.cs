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

using System;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    ///<summary>
    ///Class to represent a Tab Version Detail object. Each detail is related with a TabVersion and also with a ModuleInfo
    ///</summary>    
    [Serializable]
    public class TabVersionDetail: BaseEntityInfo
    {
      #region Public Properties

        /// <summary>
        /// Id of TabVersionDetail
        /// </summary>
        public int TabVersionDetailId { get; set; }

        /// <summary>
        /// Id of the related TabVersion master of the detail
        /// </summary>
        public int TabVersionId { get; set; }

        /// <summary>
        /// Id of the Module which tracks the detail
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Version number of the module when the detail was tracked
        /// </summary>
        public int ModuleVersion { get; set; }

        /// <summary>
        /// Pane name where the Module was when the detail was tracked
        /// </summary>
        public string PaneName { get; set; }

        /// <summary>
        /// Order into the pane where the Module was when the detail was tracked
        /// </summary>
        public int ModuleOrder { get; set; }

        /// <summary>
        /// Action which provoked the detail
        /// </summary>
        public TabVersionDetailAction Action { get; set; }

        #endregion
    }
}