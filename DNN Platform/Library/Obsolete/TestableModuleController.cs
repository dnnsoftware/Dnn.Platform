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
using System;
using System.ComponentModel;
using System.Reflection;

using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Modules.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use ModuleController instead. Scheduled removal in v10.0.0.")]
    public class TestableModuleController : ServiceLocator<IModuleController, TestableModuleController>, IModuleController
    {
        protected override Func<IModuleController> GetFactory()
        {
            return () => new TestableModuleController();
        }

         public ModuleInfo GetModule(int moduleId, int tabId)
         {
             return ModuleController.Instance.GetModule(moduleId, tabId, false);
         }

         public void UpdateModuleSetting(int moduleId, string settingName, string settingValue)
         {
             ModuleController.Instance.UpdateModuleSetting(moduleId, settingName, settingValue);
         }

         public void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue)
         {
             ModuleController.Instance.UpdateTabModuleSetting(tabModuleId, settingName, settingValue);
         }
    }
}