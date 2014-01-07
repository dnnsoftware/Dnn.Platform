// // DotNetNuke® - http://www.dotnetnuke.com
// // Copyright (c) 2002-2014
// // by DotNetNuke Corporation
// // 
// // Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// // documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// // the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// // to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// // 
// // The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// // of the Software.
// // 
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// // TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// // THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// // CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// // DEALINGS IN THE SOFTWARE.
using System;

namespace DotNetNuke.Entities.Modules.Internal
{
    internal class ModuleControllerImpl : IModuleController
    {
        readonly ModuleController _legacyController = new ModuleController();

        public ModuleInfo GetModule(int moduleId, int tabId)
        {
            return _legacyController.GetModule(moduleId, tabId);
        }

        public void UpdateModuleSetting(int moduleId, string settingName, string settingValue)
        {
            _legacyController.UpdateModuleSetting(moduleId, settingName, settingValue);
        }

        public void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue)
        {
            _legacyController.UpdateTabModuleSetting(tabModuleId, settingName, settingValue);
        }
    }
}