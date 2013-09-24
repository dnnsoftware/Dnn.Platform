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
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LanguageInstaller installs Language Packs to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	01/29/2008  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class LanguageInstaller : FileInstaller
    {
		#region Private Members

        private readonly LanguagePackType LanguagePackType;
        private LanguagePackInfo InstalledLanguagePack;
        private Locale Language;
        private LanguagePackInfo LanguagePack;
        private Locale TempLanguage;

		#endregion

        public LanguageInstaller(LanguagePackType type)
        {
            LanguagePackType = type;
        }
		
		#region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("languageFiles")
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	01/29/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "languageFiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("languageFile")
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	01/29/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "languageFile";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	03/28/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get { return "resx, xml, tdf,template"; }
        }
		
		#endregion

		#region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteLanguage method deletes the Language
        /// from the data Store.
        /// </summary>
        /// <history>
        /// 	[cnurse]	02/11/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void DeleteLanguage()
        {
            try
            {
				//Attempt to get the LanguagePack
                LanguagePackInfo tempLanguagePack = LanguagePackController.GetLanguagePackByPackage(Package.PackageID);

                //Attempt to get the Locale
                Locale language = LocaleController.Instance.GetLocale(tempLanguagePack.LanguageID);
                if (tempLanguagePack != null)
                {
                    LanguagePackController.DeleteLanguagePack(tempLanguagePack);
                }

                // fix DNN-26330	 Removing a language pack extension removes the language
                // we should not delete language when deleting language pack, as there is just a loose relationship
                //if (language != null && tempLanguagePack.PackageType == LanguagePackType.Core)
                //{
                //    Localization.Localization.DeleteLanguage(language);
                //}

                Log.AddInfo(string.Format(Util.LANGUAGE_UnRegistered, language.Text));
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }
		
		#endregion

		#region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadCustomManifest method reads the custom manifest items
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node</param>
        /// <history>
        /// 	[cnurse]	08/22/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void ReadCustomManifest(XPathNavigator nav)
        {
            Language = new Locale();
            LanguagePack = new LanguagePackInfo();

            //Get the Skin name
            Language.Code = Util.ReadElement(nav, "code");
            Language.Text = Util.ReadElement(nav, "displayName");
            Language.Fallback = Util.ReadElement(nav, "fallback");

            if (LanguagePackType == LanguagePackType.Core)
            {
                LanguagePack.DependentPackageID = -2;
            }
            else
            {
                string packageName = Util.ReadElement(nav, "package");
                PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == packageName);
                if (package != null)
                {
                    LanguagePack.DependentPackageID = package.PackageID;
                }
            }
			
            //Call base class
            base.ReadCustomManifest(nav);
        }
		
		#endregion

		#region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Modules this is not neccessary</remarks>
        /// <history>
        /// 	[cnurse]	01/15/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
            if (LanguagePackType == LanguagePackType.Core || LanguagePack.DependentPackageID > 0)
            {
               base.Commit();             
            }
            else
            {
                Completed = true;
                Skipped = true;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the language component
        /// </summary>
        /// <history>
        /// 	[cnurse]	02/11/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            if (LanguagePackType == LanguagePackType.Core || LanguagePack.DependentPackageID > 0)
            {
                try
                {
                    //Attempt to get the LanguagePack
                    InstalledLanguagePack = LanguagePackController.GetLanguagePackByPackage(Package.PackageID);
                    if (InstalledLanguagePack != null)
                    {
                        LanguagePack.LanguagePackID = InstalledLanguagePack.LanguagePackID;
                    }

                    //Attempt to get the Locale
                    TempLanguage = LocaleController.Instance.GetLocale(Language.Code);
                    if (TempLanguage != null)
                    {
                        Language.LanguageId = TempLanguage.LanguageId;
                    }
                    if (LanguagePack.PackageType == LanguagePackType.Core)
                    {
                        //Update language
                        Localization.Localization.SaveLanguage(Language);
                    }

                    //Set properties for Language Pack
                    LanguagePack.PackageID = Package.PackageID;
                    LanguagePack.LanguageID = Language.LanguageId;

                    //Update LanguagePack
                    LanguagePackController.SaveLanguagePack(LanguagePack);

                    Log.AddInfo(string.Format(Util.LANGUAGE_Registered, Language.Text));

                    //install (copy the files) by calling the base class
                    base.Install();
                }
                catch (Exception ex)
                {
                    Log.AddFailure(ex);
                }
            }
            else
            {
                Completed = true;
                Skipped = true;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the component in the event 
        /// that one of the other components fails
        /// </summary>
        /// <history>
        /// 	[cnurse]	02/11/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
			//If Temp Language exists then we need to update the DataStore with this 
            if (TempLanguage == null)
            {
				//No Temp Language - Delete newly added Language
                DeleteLanguage();
            }
            else
            {
				//Temp Language - Rollback to Temp
                Localization.Localization.SaveLanguage(TempLanguage);
            }
            
			//Call base class to prcoess files
			base.Rollback();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the language component
        /// </summary>
        /// <history>
        /// 	[cnurse]	02/11/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            DeleteLanguage();

            //Call base class to prcoess files
            base.UnInstall();
        }

		#endregion
    }
}
