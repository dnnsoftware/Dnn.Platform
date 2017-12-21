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

using System.Collections.Generic;
using Dnn.PersonaBar.Library.Model;

namespace Dnn.PersonaBar.Library.Repository
{
    /// <summary>
    /// Interface responsible for persona bar extensions.
    /// </summary>
    public interface IPersonaBarExtensionRepository
    {
        /// <summary>
        /// save extension.
        /// </summary>
        /// <param name="extension"></param>
        void SaveExtension(PersonaBarExtension extension);

        /// <summary>
        /// delete extension.
        /// </summary>
        /// <param name="extension"></param>
        void DeleteExtension(PersonaBarExtension extension);

        /// <summary>
        /// delete extension.
        /// </summary>
        /// <param name="identifier"></param>
        void DeleteExtension(string identifier);

        /// <summary>
        /// get persona bar extensions.
        /// </summary>
        /// <returns></returns>
        IList<PersonaBarExtension> GetExtensions();

        /// <summary>
        /// get persona bar extensions for menu.
        /// </summary>
        /// <returns></returns>
        IList<PersonaBarExtension> GetExtensions(int menuId);

    }
}
