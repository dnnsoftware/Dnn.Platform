/*
' Copyright (c) 2011 DotNetNuke Corporation
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System.Collections.Generic;
//using System.Xml;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Search;

namespace DotNetNuke.Modules.Journal.Components {

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Controller class for Journal
    /// </summary>
    /// -----------------------------------------------------------------------------

    //uncomment the interfaces to add the support.
    public class FeatureController //: IPortable, ISearchable, IUpgradeable
    {


        #region Optional Interfaces

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ExportModule implements the IPortable ExportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be exported</param>
        /// -----------------------------------------------------------------------------
        public string ExportModule(int ModuleID) {
            //string strXML = "";

            //List<JournalInfo> colJournals = GetJournals(ModuleID);
            //if (colJournals.Count != 0)
            //{
            //    strXML += "<Journals>";

            //    foreach (JournalInfo objJournal in colJournals)
            //    {
            //        strXML += "<Journal>";
            //        strXML += "<content>" + DotNetNuke.Common.Utilities.XmlUtils.XMLEncode(objJournal.Content) + "</content>";
            //        strXML += "</Journal>";
            //    }
            //    strXML += "</Journals>";
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
        public void ImportModule(int ModuleID, string Content, string Version, int UserId) {
            //XmlNode xmlJournals = DotNetNuke.Common.Globals.GetContent(Content, "Journals");
            //foreach (XmlNode xmlJournal in xmlJournals.SelectNodes("Journal"))
            //{
            //    JournalInfo objJournal = new JournalInfo();
            //    objJournal.ModuleId = ModuleID;
            //    objJournal.Content = xmlJournal.SelectSingleNode("content").InnerText;
            //    objJournal.CreatedByUser = UserID;
            //    AddJournal(objJournal);
            //}

            throw new System.NotImplementedException("The method or operation is not implemented.");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchItems implements the ISearchable Interface
        /// </summary>
        /// <param name="ModInfo">The ModuleInfo for the module to be Indexed</param>
        /// -----------------------------------------------------------------------------
        #pragma warning disable 0618
        public DotNetNuke.Services.Search.SearchItemInfoCollection GetSearchItems(DotNetNuke.Entities.Modules.ModuleInfo ModInfo) {
            //SearchItemInfoCollection SearchItemCollection = new SearchItemInfoCollection();

            //List<JournalInfo> colJournals = GetJournals(ModInfo.ModuleID);

            //foreach (JournalInfo objJournal in colJournals)
            //{
            //    SearchItemInfo SearchItem = new SearchItemInfo(ModInfo.ModuleTitle, objJournal.Content, objJournal.CreatedByUser, objJournal.CreatedDate, ModInfo.ModuleID, objJournal.ItemId.ToString(), objJournal.Content, "ItemId=" + objJournal.ItemId.ToString());
            //    SearchItemCollection.Add(SearchItem);
            //}

            //return SearchItemCollection;

            throw new System.NotImplementedException("The method or operation is not implemented.");
        }
        #pragma warning restore 0618

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpgradeModule implements the IUpgradeable Interface
        /// </summary>
        /// <param name="Version">The current version of the module</param>
        /// -----------------------------------------------------------------------------
        public string UpgradeModule(string Version) {
            throw new System.NotImplementedException("The method or operation is not implemented.");
        }

        #endregion

    }

}
