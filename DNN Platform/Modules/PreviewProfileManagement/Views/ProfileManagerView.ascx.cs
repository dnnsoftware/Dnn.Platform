#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

#region "Usings"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Framework;
using DotNetNuke.Modules.PreviewProfileManagement.Components;
using DotNetNuke.Modules.PreviewProfileManagement.Presenters;
using DotNetNuke.Modules.PreviewProfileManagement.ViewModels;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mobile;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Mvp;
using DotNetNuke.Web.UI.WebControls;

using Telerik.Web.UI;
using Telerik.Web.UI.Grid;

using WebFormsMvp;

#endregion

namespace DotNetNuke.Modules.PreviewProfileManagement.Views
{
	/// <summary>
	/// 
	/// </summary>
	[PresenterBinding(typeof(ProfileManagerPresenter))]
	public partial class ProfileManagerView : ModuleView<ProfileManagerViewModel>, IProfileManagerView, IClientAPICallbackEventHandler
	{
		#region "Public Events"

		/// <summary>
		/// Event for get profile data.
		/// </summary>
		public event EventHandler GetProfiles;

		/// <summary>
		/// Event for get highlight profile data, this data will be used for auto complete the device name.
		/// </summary>
		public event EventHandler GetHighlightProfiles;

		/// <summary>
		/// Event for save a profile.
		/// </summary>
		public event EventHandler<ProfileEventArgs> SaveProfile;

		/// <summary>
		/// Event for delete a profile.
		/// </summary>
		public event EventHandler<PrimaryKeyEventArgs> DeleteProfile;

		/// <summary>
		/// Event for get a profile to edit.
		/// </summary>
		public event EventHandler<PrimaryKeyEventArgs> GetEditProfile;

		#endregion

		#region "Event Handlers"

		/// <summary>
		/// OnLoad Event.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			btnSave.Click += new EventHandler(btnSave_Click);
            //ProfilesList.ItemCommand += new DataGridCommandEventHandler(ProfilesList_ItemCommand);
            //ProfilesList.ItemDataBound += new DataGridItemEventHandler(ProfilesList_ItemDataBound);

            ProfilesList.ItemCommand += new Telerik.Web.UI.GridCommandEventHandler(ProfilesList_ItemCommand);
            ProfilesList.ItemDataBound += new Telerik.Web.UI.GridItemEventHandler(ProfilesList_ItemDataBound);

			BindControls();

            if(!IsPostBack)
            {
                //Localization.LocalizeDataGrid(ref ProfilesList, LocalResourceFile);
            }

			ClientAPI.RegisterClientVariable(Page, "ActionCallback", ClientAPI.GetCallbackEventReference(this, "[ACTIONTOKEN]", "success", "this", "error"), true);
		}

        void ProfilesList_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.Item || e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.EditFormItem )
            {
                IPreviewProfile profile = e.Item.DataItem as IPreviewProfile;
                e.Item.Attributes.Add("data", profile.Id.ToString());
            }
        }

        void ProfilesList_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "Edit":
                    //ProfilesList.EditItemIndex = e.Item.ItemIndex;
                    //ProfilesList.EditIndexes.Add(e.Item.ItemIndex);                    
                    e.Item.Edit = true;
                    
                    LoadProfiles(true);
                    AddProfile.Visible = false;
                    break;
                case "Save":
                    if (Page.IsValid)
                    {
                        GetEditProfile(this, new PrimaryKeyEventArgs(Convert.ToInt32(e.CommandArgument)));

                        var gridDataItem = (GridDataItem)e.Item;
                        var editName = (gridDataItem["DeviceName"].Controls.Cast<Control>().First(c => c is TextBox) as TextBox).Text;
                        var editWidth = Convert.ToInt32((gridDataItem["Width"].Controls.Cast<Control>().First(c => c is TextBox) as TextBox).Text);
                        var editHeight = Convert.ToInt32((gridDataItem["Height"].Controls.Cast<Control>().First(c => c is TextBox) as TextBox).Text);
                        var editUserAgent = (gridDataItem["UserAgent"].Controls.Cast<Control>().First(c => c is TextBox) as TextBox).Text;

                        Model.EditProfile.Name = editName;
                        Model.EditProfile.Width = editWidth;
                        Model.EditProfile.Height = editHeight;
                        Model.EditProfile.UserAgent = editUserAgent;

                        SaveProfile(this, new ProfileEventArgs(Model.EditProfile));

                        //ProfilesList.EditItemIndex = -1;
                        ProfilesList.EditIndexes.Clear();
                        LoadProfiles(true);
                        AddProfile.Visible = true;
                    }
                    break;
                case "Cancel":
                    //ProfilesList.EditItemIndex = -1;
                    ProfilesList.EditIndexes.Clear();

                    LoadProfiles(true);
                    AddProfile.Visible = true;
                    break;
                case "Delete":
                    DeleteProfile(this, new PrimaryKeyEventArgs(Convert.ToInt32(e.CommandArgument)));
                    //ProfilesList.EditItemIndex = -1;
                    ProfilesList.EditIndexes.Clear();

                    LoadProfiles(true);
                    AddProfile.Visible = true;
                    break;
            }
        }

        //private void ProfilesList_ItemDataBound(object sender, DataGridItemEventArgs e)
        //{
        //    if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.EditItem)
        //    {
        //        IPreviewProfile profile = e.Item.DataItem as IPreviewProfile;
        //        e.Item.Attributes.Add("data", profile.Id.ToString());
        //    }
        //}

		private void btnSave_Click(object sender, EventArgs e)
		{
			if (Page.IsValid)
			{
				var name = cbName.Text;
				var width = Convert.ToInt32(txtWidth.Text);
				var height = Convert.ToInt32(txtHeight.Text);
				var userAgent = txtUserAgent.Text;

				var profile = new PreviewProfile { Name = name, Width = width, Height = height, UserAgent = userAgent, PortalId = ModuleContext.PortalId };

				SaveProfile(this, new ProfileEventArgs(profile));

				cbName.SelectedIndex = -1;
				cbName.Text = txtWidth.Text = txtHeight.Text = txtUserAgent.Text = string.Empty;

				LoadProfiles(true);
			}
		}

        //private void ProfilesList_ItemCommand(object source, DataGridCommandEventArgs e)
        //{
        //    switch (e.CommandName)
        //    {
        //        case "Edit":
        //            ProfilesList.EditItemIndex = e.Item.ItemIndex;
        //            LoadProfiles(true);
        //            AddProfile.Visible = false;
        //            break;
        //        case "Save":
        //            if (Page.IsValid)
        //            {
        //                GetEditProfile(this, new PrimaryKeyEventArgs(Convert.ToInt32(e.CommandArgument)));

        //                var editName = (e.Item.Cells[3].Controls.Cast<Control>().First(c => c is TextBox) as TextBox).Text;
        //                var editWidth = Convert.ToInt32((e.Item.Cells[4].Controls.Cast<Control>().First(c => c is TextBox) as TextBox).Text);
        //                var editHeight = Convert.ToInt32((e.Item.Cells[5].Controls.Cast<Control>().First(c => c is TextBox) as TextBox).Text);
        //                var editUserAgent = (e.Item.Cells[6].Controls.Cast<Control>().First(c => c is TextBox) as TextBox).Text;

        //                Model.EditProfile.Name = editName;
        //                Model.EditProfile.Width = editWidth;
        //                Model.EditProfile.Height = editHeight;
        //                Model.EditProfile.UserAgent = editUserAgent;

        //                SaveProfile(this, new ProfileEventArgs(Model.EditProfile));

        //                ProfilesList.EditItemIndex = -1;
        //                LoadProfiles(true);
        //                AddProfile.Visible = true;
        //            }
        //            break;
        //        case "Cancel":
        //            ProfilesList.EditItemIndex = -1;
        //            LoadProfiles(true);
        //            AddProfile.Visible = true;
        //            break;
        //        case "Delete":
        //            DeleteProfile(this, new PrimaryKeyEventArgs(Convert.ToInt32(e.CommandArgument)));
        //            ProfilesList.EditItemIndex = -1;
        //            LoadProfiles(true);
        //            AddProfile.Visible = true;
        //            break;
        //    }
        //}

		protected void ValidateName(object sender, ServerValidateEventArgs e)
		{
			if (string.IsNullOrEmpty(e.Value))
			{
				e.IsValid = false;
			}
			else
			{
				var validator = sender as CustomValidator;
				GetProfiles(this, new EventArgs());

				if (validator.ValidationGroup == "AddProfile")
				{
					e.IsValid = !Model.PreviewProfiles.Any(p => p.Name == e.Value);
				}
				else if(validator.ValidationGroup == "EditProfile")
				{
					var initValue = validator.Attributes["InitValue"];
					
					if(e.Value != initValue)
					{
						e.IsValid = !Model.PreviewProfiles.Any(p => p.Name == e.Value);
					}
				}
			}
		}

		#endregion

		#region "Private Methods"

		private void BindControls()
		{
			LoadProfiles();
            LoadHighlightProfiles();
		}

		private void LoadProfiles()
		{
			LoadProfiles(false);
		}

        private void LoadHighlightProfiles()
        {
            if (!IsPostBack)
            {
                GetHighlightProfiles(this, new EventArgs());

                cbName.Items.Clear();
                foreach (var profile in Model.HighlightProfiles)
                {
                    var text = profile.Name;
                    var value = string.Format("id:\"{0}\", width: \"{1}\", height:\"{2}\", userAgent: \"{3}\"", cbName.Items.Count, profile.Width, profile.Height, profile.UserAgent);

                    cbName.Items.Add(new DnnComboBoxItem(text, value));
                }
            }
        }

		private void LoadProfiles(bool rebind)
		{
			GetProfiles(this, new EventArgs());
			ProfilesList.DataSource = Model.PreviewProfiles;

			if (!IsPostBack || rebind)
			{
				ProfilesList.DataBind();
			}
		}

		#endregion

		#region "IClientAPICallbackEventHandler Implementation"

		/// <summary>
		/// IClientAPICallbackEventHandler.RaiseClientAPICallbackEvent
		/// </summary>
		/// <param name="eventArgument"></param>
		/// <returns></returns>
		public string RaiseClientAPICallbackEvent(string eventArgument)
		{
			IDictionary<string, string> arguments = new Dictionary<string, string>();
			foreach (var arg in eventArgument.Split('&'))
			{
				arguments.Add(arg.Split('=')[0], arg.Split('=')[1]);
			}
			switch (arguments["action"])
			{
				case "sort":
					var moveId = Convert.ToInt32(arguments["moveId"]);
					var nextId = Convert.ToInt32(arguments["nextId"]);

					new ProfileManagerPresenter(this).SortProfiles(moveId, nextId);
					break;
			}

			return string.Empty;
		}

		#endregion
	}
}