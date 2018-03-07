using Cantarus.Libraries.Encryption;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Search;
using System;
using System.Collections.Generic;

namespace Cantarus.Modules.PolyDeploy.Components
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Controller class for PolyDeploy
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class FeatureController : IUpgradeable
    {

        #region Optional Interfaces

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ExportModule implements the IPortable ExportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be exported</param>
        /// -----------------------------------------------------------------------------
        public string ExportModule(int ModuleID)
        {
            //string strXML = "";

            //List<PolyDeployInfo> colPolyDeploys = GetPolyDeploys(ModuleID);
            //if (colPolyDeploys.Count != 0)
            //{
            //    strXML += "<PolyDeploys>";

            //    foreach (PolyDeployInfo objPolyDeploy in colPolyDeploys)
            //    {
            //        strXML += "<PolyDeploy>";
            //        strXML += "<content>" + DotNetNuke.Common.Utilities.XmlUtils.XMLEncode(objPolyDeploy.Content) + "</content>";
            //        strXML += "</PolyDeploy>";
            //    }
            //    strXML += "</PolyDeploys>";
            //}

            //return strXML;

            throw new System.NotImplementedException("The method or operation is not implemented.");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ImportModule implements the IPortable ImportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be imported</param>
        /// <param name="Content">The content to be imported</param>
        /// <param name="Version">The version of the module to be imported</param>
        /// <param name="UserId">The Id of the user performing the import</param>
        /// -----------------------------------------------------------------------------
        public void ImportModule(int ModuleID, string Content, string Version, int UserID)
        {
            //XmlNode xmlPolyDeploys = DotNetNuke.Common.Globals.GetContent(Content, "PolyDeploys");
            //foreach (XmlNode xmlPolyDeploy in xmlPolyDeploys.SelectNodes("PolyDeploy"))
            //{
            //    PolyDeployInfo objPolyDeploy = new PolyDeployInfo();
            //    objPolyDeploy.ModuleId = ModuleID;
            //    objPolyDeploy.Content = xmlPolyDeploy.SelectSingleNode("content").InnerText;
            //    objPolyDeploy.CreatedByUser = UserID;
            //    AddPolyDeploy(objPolyDeploy);
            //}

            throw new System.NotImplementedException("The method or operation is not implemented.");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchItems implements the ISearchable Interface
        /// </summary>
        /// <param name="ModInfo">The ModuleInfo for the module to be Indexed</param>
        /// -----------------------------------------------------------------------------
        public DotNetNuke.Services.Search.SearchItemInfoCollection GetSearchItems(DotNetNuke.Entities.Modules.ModuleInfo ModInfo)
        {
            //SearchItemInfoCollection SearchItemCollection = new SearchItemInfoCollection();

            //List<PolyDeployInfo> colPolyDeploys = GetPolyDeploys(ModInfo.ModuleID);

            //foreach (PolyDeployInfo objPolyDeploy in colPolyDeploys)
            //{
            //    SearchItemInfo SearchItem = new SearchItemInfo(ModInfo.ModuleTitle, objPolyDeploy.Content, objPolyDeploy.CreatedByUser, objPolyDeploy.CreatedDate, ModInfo.ModuleID, objPolyDeploy.ItemId.ToString(), objPolyDeploy.Content, "ItemId=" + objPolyDeploy.ItemId.ToString());
            //    SearchItemCollection.Add(SearchItem);
            //}

            //return SearchItemCollection;

            throw new System.NotImplementedException("The method or operation is not implemented.");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpgradeModule implements the IUpgradeable Interface
        /// </summary>
        /// <param name="Version">The current version of the module</param>
        /// -----------------------------------------------------------------------------
        public string UpgradeModule(string version)
        {
            string result = string.Format("Upgrade logic for {0} completed.", version);

            switch (version)
            {
                case "00.07.00":
                    Upgrade_00_07_00();
                    break;

                default:
                    result = string.Format("No upgrade logic for {0}.", version);
                    break;

            }

            return result;
        }

        #endregion

        #region Upgrade Logic

        /// <summary>
        /// Upgrades to 00.07.00
        /// 
        /// Operations:
        /// - Generate a Salt for existing APIUser objects.
        /// - Hash existing APIKeys using the new Salt and store in new field.
        /// - Encrypt existing EncryptionKeys using plain text APIKey and
        ///     store in new field.
        /// </summary>
        private void Upgrade_00_07_00()
        {
            APIUserDataController dc = new APIUserDataController();

            foreach(APIUser apiUser in dc.Get())
            {
                // Generate a salt.
                byte[] saltBytes = Cantarus.Libraries.Encryption.Utilities.GenerateRandomBytes(32);
                apiUser.Salt = BitConverter.ToString(saltBytes);

                // Use existing plain text api key and salt to create a hashed api key.
                apiUser.APIKey_Sha = Cantarus.Libraries.Encryption.Utilities.SHA256Hash(apiUser.APIKey + apiUser.Salt);

                // Encrypt existing plain text encryption key and store in new field.
                apiUser.EncryptionKey_Enc = Crypto.Encrypt(apiUser.EncryptionKey, apiUser.APIKey);
            }
        }

        #endregion
    }

}
