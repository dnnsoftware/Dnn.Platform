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

using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.Index
    /// Class:      SearchContentModuleInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchContentModuleInfo class represents an extendension (by containment)
    /// of ModuleInfo to add a parametere that determines whether a module is Searchable
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SearchContentModuleInfo
    {
#pragma warning disable 0618
        protected ISearchable MModControllerType;
#pragma warning restore 0618
        protected ModuleSearchBase SearchBaseControllerType;
        protected ModuleInfo MModInfo;

#pragma warning disable 0618
        public ISearchable ModControllerType
        {
            get
            {
                return MModControllerType;
            }
            set
            {
                MModControllerType = value;
            }
        }
#pragma warning restore 0618

        public ModuleSearchBase ModSearchBaseControllerType
        {
            get
            {
                return SearchBaseControllerType;
            }
            set
            {
                SearchBaseControllerType = value;
            }
        }

        public ModuleInfo ModInfo
        {
            get
            {
                return MModInfo;
            }
            set
            {
                MModInfo = value;
            }
        }
    }
}
