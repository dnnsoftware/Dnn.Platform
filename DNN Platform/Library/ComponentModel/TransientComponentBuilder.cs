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

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.ComponentModel
{
    internal class TransientComponentBuilder : IComponentBuilder
    {
        private readonly string _Name;
        private readonly Type _Type;

        /// <summary>
        /// Initializes a new instance of the TransientComponentBuilder class.
        /// </summary>
        /// <param name="name">The name of the component</param>
        /// <param name="type">The type of the component</param>
        public TransientComponentBuilder(string name, Type type)
        {
            _Name = name;
            _Type = type;
        }

        #region IComponentBuilder Members

        public object BuildComponent()
        {
            return Reflection.CreateObject(_Type);
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        #endregion
    }
}
