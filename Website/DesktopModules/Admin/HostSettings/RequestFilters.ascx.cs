#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.HttpModules.RequestFilter;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Host
{
    /// <summary>
    /// The FriendlyUrls PortalModuleBase is used to edit the friendly urls
    /// for the application.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	07/06/2006 Created
    /// </history>
    public partial class RequestFilters : PortalModuleBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (RequestFilters));
		#region "Private Fields"
        private List<RequestFilterRule> _Rules;
		
		#endregion
		
		#region "Private Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the mode of the control
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The mode is used to determine when the user is creating a new rule 
        /// and allows the system to know to remove "blank" rules if the user cancels
        /// the edit.
        /// </returns>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private bool AddMode
        {
            get
            {
                bool _Mode = Null.NullBoolean;
                if (ViewState["Mode"] != null)
                {
                    _Mode = Convert.ToBoolean(ViewState["Mode"]);
                }
                return _Mode;
            }
            set
            {
                ViewState["Mode"] = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads and writes to the list of Request Filter rules
        /// </summary>
        /// <value>
        /// Generic List(Of RequestFilterRule)
        /// </value>
        /// <returns>
        /// </returns>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private List<RequestFilterRule> Rules
        {
            get
            {
                if (_Rules == null)
                {
                    _Rules = RequestFilterSettings.GetSettings().Rules;
                }
                return _Rules;
            }
            set
            {
                _Rules = value;
            }
        }
		
		#endregion

		#region "Private Methods"

        /// <summary>
        /// Adds a confirmation dialog to the delete button.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks></remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        private static void AddConfirmActiontoDeleteButton(DataListItemEventArgs e)
        {
            var cmdDelete = (ImageButton) e.Item.FindControl("cmdDelete");
            ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteItem"));
        }

        /// <summary>
        /// Binds the selected values of the Request Filter Rule dropdown lists.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks></remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        private void BindDropDownValues(DataListItemEventArgs e)
        {
            var rule = (RequestFilterRule) e.Item.DataItem;

            var ddlOperator = (DnnComboBox) e.Item.FindControl("ddlOperator");
            if (ddlOperator != null && rule != null)
            {
                ddlOperator.SelectedValue = rule.Operator.ToString();
            }
			var ddlAction = (DnnComboBox)e.Item.FindControl("ddlAction");
            if (ddlAction != null && rule != null)
            {
                ddlAction.SelectedValue = rule.Action.ToString();
            }
        }

        /// <summary>
        /// BindRules updates the datalist with the values of the current list of rules.
        /// </summary>
        /// <remarks></remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        private void BindRules()
        {
            rptRules.DataSource = Rules;
            rptRules.DataBind();
        }

		#endregion

		#region "Protected methods"

        /// <summary>
        /// Retrieves the list of rules from the viewstate rather than constantly 
        /// re-reading the configuration file.
        /// </summary>
        /// <param name="savedState"></param>
        /// <remarks></remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        protected override void LoadViewState(object savedState)
        {
            var myState = (object[]) savedState;
            if ((myState[0] != null))
            {
                base.LoadViewState(myState[0]);
            }
            if ((myState[1] != null))
            {
                var configRules = new List<RequestFilterRule>();

                //Deserialize into RewriterConfiguration
				var xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(Convert.ToString(myState[1]));
	            var nodesList = xmlDocument.SelectNodes("/ArrayOfRequestFilterRule/RequestFilterRule");
	            if (nodesList != null)
	            {
		            foreach (XmlNode node in nodesList)
		            {
			            var rule = CBO.DeserializeObject<RequestFilterRule>(XmlReader.Create(new StringReader(node.OuterXml)));
			            configRules.Add(rule);
		            }
	            }

	            Rules = configRules;
            }
        }

        /// <summary>
        /// Saves the rules to the viewstate to avoid constantly re-reading
        /// the configuration file.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        protected override object SaveViewState()
        {
            var configRules = new List<RequestFilterRule>();
            configRules = Rules;

            object baseState = base.SaveViewState();
            var allStates = new object[2];
            allStates[0] = baseState;
            allStates[1] = XmlUtils.Serialize(configRules);

            return allStates;
        }

		#endregion

		#region "Event Handlers"

        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdAddRule.Click += AddRule;
            rptRules.ItemDataBound += rptRules_ItemDataBound;
            rptRules.EditCommand += EditRule;
            rptRules.DeleteCommand += DeleteRule;
            rptRules.UpdateCommand += SaveRule;
            rptRules.CancelCommand += CancelEdit;

            //Bind the rules (as long as not postback)
            if (!Page.IsPostBack)
            {
                BindRules();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddRule runs when the Add button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void AddRule(object sender, EventArgs e)
        {
            //Add a new empty rule and set the editrow to the new row
            Rules.Add(new RequestFilterRule());
            rptRules.EditItemIndex = Rules.Count - 1;

            //Set the AddMode to true
            AddMode = true;

            //Rebind the collection
            BindRules();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteRule runs when the Delete button for a specified rule is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void DeleteRule(object source, DataListCommandEventArgs e)
        {
            //Get the index of the row to delete
            int index = e.Item.ItemIndex;

            //Remove the rule from the rules collection
            Rules.RemoveAt(index);
            try
            {
                //Save the new collection
                RequestFilterSettings.Save(Rules);
            }
            catch (UnauthorizedAccessException exc)
            {
                Logger.Debug(exc);

                lblErr.InnerText = Localization.GetString("unauthorized", LocalResourceFile);
                lblErr.Visible = true;
                //This forces the system to reload the settings from DotNetNuke.Config
                //since we have already deleted the entry from the Rules list.
                Rules = null;
            }
			
            //Rebind the collection
            BindRules();
        }

        /// <summary>
        /// EditRule runs when the Edit button is clicked.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        protected void EditRule(object source, DataListCommandEventArgs e)
        {
            lblErr.Visible = true;

            //Set the AddMode to false
            AddMode = false;

            //Set the editrow
            rptRules.EditItemIndex = e.Item.ItemIndex;

            //Rebind the collection
            BindRules();
        }

        /// <summary>
        /// SaveRule runs when the Save button is clicked.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// The Save button is displayed for a specific request filter rule
        /// when the user enters the edit mode.
        /// </remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        protected void SaveRule(object source, DataListCommandEventArgs e)
        {
            //Get the index of the row to save
            int index = rptRules.EditItemIndex;

            RequestFilterRule rule = Rules[index];
            var txtServerVar = (TextBox) e.Item.FindControl("txtServerVar");
            var txtValue = (TextBox) e.Item.FindControl("txtValue");
            var txtLocation = (TextBox) e.Item.FindControl("txtLocation");
			var ddlOperator = (DnnComboBox)e.Item.FindControl("ddlOperator");
			var ddlAction = (DnnComboBox)e.Item.FindControl("ddlAction");
            if (!String.IsNullOrEmpty(txtServerVar.Text) && !String.IsNullOrEmpty(txtValue.Text))
            {
                rule.ServerVariable = txtServerVar.Text;
                rule.Location = txtLocation.Text;
                rule.Operator = (RequestFilterOperatorType) Enum.Parse(typeof (RequestFilterOperatorType), ddlOperator.SelectedValue);
                rule.Action = (RequestFilterRuleType) Enum.Parse(typeof (RequestFilterRuleType), ddlAction.SelectedValue);

                //A rule value may be a semicolon delimited list of values.  So we need to use a helper function to 
                //parse the list.  If this is a regex, then only one value is supported.
                rule.SetValues(txtValue.Text, rule.Operator);

                //Save the modified collection
                RequestFilterSettings.Save(Rules);
            }
            else
            {
                if (AddMode)
                {
					//Remove the temporary added row
                    Rules.RemoveAt(Rules.Count - 1);
                }
            }
            AddMode = false;

            //Reset Edit Index
            rptRules.EditItemIndex = -1;
            BindRules();
        }

        /// <summary>
        /// CancelEdit runs when the Cancel button is clicked.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// The Cancel button is displayed for a specific request filter rule
        /// when the user enters the edit mode.  Clicking the cancel button will
        /// return the user to normal view mode with saving any of their changes
        /// to the specific Request Filter Rule.
        /// </remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        protected void CancelEdit(object source, DataListCommandEventArgs e)
        {
            if (AddMode)
            {
				//Remove the temporary added row
                Rules.RemoveAt(Rules.Count - 1);
                AddMode = false;
            }
            //Clear editrow
            rptRules.EditItemIndex = -1;

            //Rebind the collection
            BindRules();
        }

        /// <summary>
        /// The ItemDataBound event is used to set the value of the Operator and Action
        /// dropdownlists based on the current values for the specific Request Filter Rule.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        protected void rptRules_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            switch (e.Item.ItemType)
            {
                case ListItemType.AlternatingItem:
                    AddConfirmActiontoDeleteButton(e);
                    break;
                case ListItemType.Item:
                    AddConfirmActiontoDeleteButton(e);
                    break;
                case ListItemType.EditItem:
                    BindDropDownValues(e);
                    break;
            }
        }

        /// <summary>
        /// The PreRender event is used to disable the "Add Rule" button when the user is in edit mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        /// <history>
        /// 	[jbrinkman]	5/28/2007  Created
        /// </history>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //If the user is editing a rule, then disable the "Add Rule" button
            cmdAddRule.Visible = rptRules.EditItemIndex == -1 || !AddMode;
        }
		
		#endregion
    }
}