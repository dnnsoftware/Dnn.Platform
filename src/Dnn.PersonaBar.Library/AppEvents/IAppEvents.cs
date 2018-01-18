#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

namespace Dnn.PersonaBar.Library.AppEvents
{
    /// <summary>
    /// This interface defines methods that need to be called at various points during
    /// the application lifecycle. All modules that need to have any housekeeping applied
    /// during these events, need to create a concrete class that extends this interface.
    /// The main application start event will enumerate these (through reflection) and
    /// call the related methods in these classes
    /// </summary>
    public interface IAppEvents
    {
        /// <summary>
        /// Method called after the application starts to perform any required startup actions by the implemetor.
        /// </summary>
        /// <remarks>This method must not used multi-threading and must perform it's task as fast as possible.</remarks>
        void ApplicationBegin();

        /// <summary>
        /// Method called before the application stops to perform any required shutdown actions by the implemetor.
        /// </summary>
        /// <remarks>This method must not used multi-threading and must perform it's task as fast as possible.</remarks>
        void ApplicationEnd();
    }
}
