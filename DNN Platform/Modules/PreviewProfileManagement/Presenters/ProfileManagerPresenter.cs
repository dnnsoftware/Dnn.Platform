#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

#region "Usings"

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.PreviewProfileManagement.Components;
using DotNetNuke.Modules.PreviewProfileManagement.ViewModels;
using DotNetNuke.Modules.PreviewProfileManagement.Views;
using DotNetNuke.Services.Mobile;
using DotNetNuke.Web.Mvp;

#endregion

namespace DotNetNuke.Modules.PreviewProfileManagement.Presenters
{
    /// <summary>
    /// Presenter of profile management list view.
    /// </summary>
    public class ProfileManagerPresenter : ModulePresenter<IProfileManagerView, ProfileManagerViewModel>
    {

		#region "Private Properties"

    	private IPreviewProfileController _previewProfileController;
        private string _highlightDataPath;

		#endregion

        #region "Public Properties"

        /// <summary>
        /// Highlight profiles data path.
        /// </summary>
        public string HighlightDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_highlightDataPath))
                {
                    if (View == null || !(View is Control))
                    {
                        throw new ArgumentException("Highlight data path should be assign when view is not a control");
                    }

                    var dataPath = Path.Combine((View as Control).TemplateSourceDirectory, "Resources\\highlightDevices.xml");
                    _highlightDataPath = HttpContext.Server.MapPath(dataPath);
                }

                return _highlightDataPath;
            }
            set
            {
                _highlightDataPath = value;
            }
        }

        #endregion

		#region "Constructors"

		/// <summary>
		/// ProfileManagerPresenter constructor.
        /// </summary>
        /// <param name="view">the profile manager view.</param>
		public ProfileManagerPresenter(IProfileManagerView view)
			: this(view, new PreviewProfileController())
        {
        }

		/// <summary>
		///  ProfileManagerPresenter constructor.
		/// </summary>
		/// <param name="view">the profile manager view.</param>
		/// <param name="controller">The profile controller.</param>
		public ProfileManagerPresenter(IProfileManagerView view, IPreviewProfileController controller)
			: base(view)
		{
            _previewProfileController = controller;

            Initialize();
		}

		#endregion

		#region "Private Methods"

		private void Initialize()
        {
			View.SaveProfile += new EventHandler<ProfileEventArgs>(SaveProfile);
			View.DeleteProfile += new EventHandler<PrimaryKeyEventArgs>(DeleteProfile);
			View.GetEditProfile += new EventHandler<PrimaryKeyEventArgs>(GetEditProfile);
			View.GetProfiles += new EventHandler(GetProfiles);
			View.GetHighlightProfiles += new EventHandler(GetHighlightProfiles);
        }

		#endregion

		#region "Event Handlers"

		/// <summary>
		/// Get a profile for edit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void GetEditProfile(object sender, PrimaryKeyEventArgs e)
		{
			View.Model.EditProfile = _previewProfileController.GetProfileById(ModuleContext.PortalId, e.Id);
		}

		/// <summary>
		/// Get profile list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void GetProfiles(object sender, EventArgs e)
		{
			View.Model.PreviewProfiles = _previewProfileController.GetProfilesByPortal(ModuleContext.PortalId);
		}

		/// <summary>
		/// Get highlight profile list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void GetHighlightProfiles(object sender, EventArgs e)
		{
            if (!File.Exists(HighlightDataPath))
            {
                throw new ArgumentException("Highlight profile databse doesn't exist.");
            }

            var serializer = new XmlSerializer(typeof(List<PreviewProfile>));
            var profiles = (List<PreviewProfile>)serializer.Deserialize(File.OpenRead(HighlightDataPath));

            View.Model.HighlightProfiles = profiles.Cast<IPreviewProfile>().ToList();
		}

		/// <summary>
		/// Save profile.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void SaveProfile(object sender, ProfileEventArgs e)
		{
			bool moveFirst = false;
			var profileList = _previewProfileController.GetProfilesByPortal(e.Profile.PortalId);

			if(e.Profile.Id == Null.NullInteger)
			{
				moveFirst = true;
			}

			//save profile
			_previewProfileController.Save(e.Profile);

			//if the profile is new, then move it to the top of list.
			if(moveFirst && profileList.Count > 0)
			{
				int moveId = e.Profile.Id;
				int nextId = profileList[0].Id;

				SortProfiles(moveId, nextId);
			}
		}

		/// <summary>
		/// Delete a profile.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void DeleteProfile(object sender, PrimaryKeyEventArgs e)
		{
			_previewProfileController.Delete(ModuleContext.PortalId, e.Id);
		}

		#endregion

		#region "Public Methods"

		/// <summary>
		/// Sort the profile list.
		/// </summary>
		/// <param name="moveId">the item which need to move.</param>
		/// <param name="nextId">the item that move item will insert before it.</param>
		/// <returns></returns>
		public string SortProfiles(int moveId, int nextId)
		{
			var moveProfile = _previewProfileController.GetProfileById(ModuleContext.PortalId, moveId);
			var nextProfile = _previewProfileController.GetProfileById(ModuleContext.PortalId, nextId);
			var allItems = _previewProfileController.GetProfilesByPortal(ModuleContext.PortalId);

			if (nextId > 0)
			{
				if (nextProfile.SortOrder > moveProfile.SortOrder)
				{
					var effectItems = allItems.Where(r => r.SortOrder > moveProfile.SortOrder && r.SortOrder < nextProfile.SortOrder).ToList();
					effectItems.ForEach(r =>
					{
						r.SortOrder--;
						_previewProfileController.Save(r);
					});

					moveProfile.SortOrder = nextProfile.SortOrder - 1;
					_previewProfileController.Save(moveProfile);
				}
				else
				{
					int nextOrder = nextProfile.SortOrder;
					var effectItems = allItems.Where(r => r.SortOrder >= nextProfile.SortOrder && r.SortOrder < moveProfile.SortOrder).ToList();
					effectItems.ForEach(r =>
					{
						r.SortOrder++;
						_previewProfileController.Save(r);
					});

					moveProfile.SortOrder = nextOrder;
					_previewProfileController.Save(moveProfile);
				}
			}
			else
			{
				var effectItems = allItems.Where(r => r.SortOrder > moveProfile.SortOrder).ToList();
				effectItems.ForEach(r =>
				{
					r.SortOrder--;
					_previewProfileController.Save(r);
				});

				moveProfile.SortOrder = allItems.Count;
				_previewProfileController.Save(moveProfile);
			}
			return string.Empty;
		}

		#endregion
    }
}