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
using System.Collections.Generic;

using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvp;

using Moq;

using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvp
{
    [TestFixture]
    public class ModuleSettingsPresenterTests
    {
        #region Private Members

        private const int _moduleSettingCount = 2;
        private const string _moduleSettingName = "moduleKey{0}";
        private const string _moduleSettingValue = "value{0}";

        private const int _tabModuleSettingCount = 3;
        private const string _tabModuleSettingName = "tabModuleKey{0}";
        private const string _tabModuleSettingValue = "value{0}";

        #endregion

        #region Embedded Classes for Testing

        public class TestSettingsPresenter : ModuleSettingsPresenter<ISettingsView<SettingsModel>, SettingsModel>
        {
            public TestSettingsPresenter(ISettingsView<SettingsModel> view)
                : base(view)
            {

            }
        }
        
        #endregion

        #region Tests

        [Test]
        public void ModuleSettingsPresenter_Load_Initialises_Both_Dictionaries_On_PostBack()
        {
            //Arrange
            var view = new Mock<ISettingsView<SettingsModel>>();
            view.SetupGet(v => v.Model).Returns(new SettingsModel());

            var presenter = new TestSettingsPresenter(view.Object) { ModuleContext = CreateModuleContext() };
            presenter.IsPostBack = true;

            //Act
            view.Raise(v => v.Load += null, EventArgs.Empty);

            //Assert
            Assert.IsInstanceOf<Dictionary<string, string>>(view.Object.Model.ModuleSettings);
            Assert.AreEqual(0, view.Object.Model.ModuleSettings.Count);

            Assert.IsInstanceOf<Dictionary<string, string>>(view.Object.Model.TabModuleSettings);
            Assert.AreEqual(0, view.Object.Model.TabModuleSettings.Count);
        }

        [Test]
        public void ModuleSettingsPresenter_Load_Does_Not_Initialise_Dictionaries_If_Not_PostBack()
        {
            //Arrange
            var view = new Mock<ISettingsView<SettingsModel>>();
            view.SetupGet(v => v.Model).Returns(new SettingsModel());

            var presenter = new TestSettingsPresenter(view.Object) { ModuleContext = CreateModuleContext() };
            presenter.IsPostBack = false;

            //Act
            view.Raise(v => v.Load += null, EventArgs.Empty);

            //Assert
            Assert.IsNull(view.Object.Model.ModuleSettings);
            Assert.IsNull(view.Object.Model.TabModuleSettings);
        }

        [Test]
        public void ModuleSettingsPresenter_LoadSettings_Loads_Both_Dictionaries()
        {
            //Arrange
            var view = new Mock<ISettingsView<SettingsModel>>();
            view.SetupGet(v => v.Model).Returns(new SettingsModel());

            var presenter = new TestSettingsPresenter(view.Object) { ModuleContext = CreateModuleContext() };
            presenter.IsPostBack = false;

            view.Raise(v => v.Load += null, EventArgs.Empty);

            //Act
            view.Raise(v => v.OnLoadSettings += null, EventArgs.Empty);

            //Assert
            Assert.IsInstanceOf<Dictionary<string, string>>(view.Object.Model.ModuleSettings);
            Assert.AreEqual(_moduleSettingCount, view.Object.Model.ModuleSettings.Count);

            Assert.IsInstanceOf<Dictionary<string, string>>(view.Object.Model.TabModuleSettings);
            Assert.AreEqual(_tabModuleSettingCount, view.Object.Model.TabModuleSettings.Count);
        }

        [Test]
        public void ModuleSettingsPresenter_SaveSettings_Saves_ModuleSettings()
        {
            //Arrange
            var view = new Mock<ISettingsView<SettingsModel>>();
            view.SetupGet(v => v.Model).Returns(new SettingsModel());

            var controller = new Mock<IModuleController>();


            var presenter = new TestSettingsPresenter(view.Object) { ModuleContext = CreateModuleContext() };
            presenter.IsPostBack = true;
            view.Raise(v => v.Load += null, EventArgs.Empty);

            //Act
            view.Raise(v => v.OnSaveSettings += null, EventArgs.Empty);

            //Assert
            foreach (var setting in view.Object.Model.ModuleSettings)
            {
                var key = setting.Key;
                var value = setting.Value;
                controller.Verify(c => c.UpdateModuleSetting(It.IsAny<int>(), key, value), Times.Once());
            }
        }

        [Test]
        public void ModuleSettingsPresenter_SaveSettings_Saves_TabModuleSettings()
        {
            //Arrange
            var view = new Mock<ISettingsView<SettingsModel>>();
            view.SetupGet(v => v.Model).Returns(new SettingsModel());

            var controller = new Mock<IModuleController>();


            var presenter = new TestSettingsPresenter(view.Object) { ModuleContext = CreateModuleContext() };
            presenter.IsPostBack = true;
            view.Raise(v => v.Load += null, EventArgs.Empty);

            //Act
            view.Raise(v => v.OnSaveSettings += null, EventArgs.Empty);

            //Assert
            foreach (var setting in view.Object.Model.TabModuleSettings)
            {
                var key = setting.Key;
                var value = setting.Value;
                controller.Verify(c => c.UpdateTabModuleSetting(It.IsAny<int>(), key, value), Times.Once());
            }
        }

        private ModuleInstanceContext CreateModuleContext()
        {
            var context = new ModuleInstanceContext {Configuration = new ModuleInfo()};
            for (int i = 1; i <= _moduleSettingCount; i++)
            {
                context.Configuration.ModuleSettings.Add(String.Format(_moduleSettingName, i), String.Format(_moduleSettingValue, i));
            }
            for (int i = 1; i <= _tabModuleSettingCount; i++)
            {
                context.Configuration.TabModuleSettings.Add(String.Format(_tabModuleSettingName, i), String.Format(_tabModuleSettingValue, i));
            }

            return context;
        }

        #endregion
    }
}