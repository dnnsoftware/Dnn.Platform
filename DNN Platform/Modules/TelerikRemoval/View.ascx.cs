// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemoval
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Maintenance.Telerik;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <summary>The main view of the DNN Telerik Removal module.</summary>
    public partial class View : PortalModuleBase
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        public View()
        {
            this.serviceProvider = this.DependencyProvider;
        }

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        internal View(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>Event handler for <see cref="CheckBox.CheckedChanged"/>.</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected void BackupConfirmationCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.RemoveTelerikButton.Enabled = this.BackupConfirmationCheckBox.Checked;
        }

        /// <summary>Event handler for the <see cref="Page.OnLoadComplete(EventArgs)"/> event.</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.Page.IsPostBack)
            {
                this.InitMainMultiView();
                this.InitRemoveTelerikButton();
                this.RegisterClientScripts();
            }
        }

        /// <summary>Click event handler for <c>RemoveTelerikButton</c>.</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected void RemoveTelerikButton_Click(object sender, EventArgs e)
        {
            if (!this.BackupConfirmationCheckBox.Checked)
            {
                return;
            }

            var uninstaller = this.GetService<ITelerikUninstaller>();

            uninstaller.Execute();

            this.UninstallReportRepeater.DataSource = uninstaller.Progress;

            this.MainMultiView.ActiveViewIndex = 3; // UninstallReportView

            this.DataBind();
        }

        /// <summary>
        /// Converts a boolean value into a check mark if <see langword="true"/>,
        /// a cross mark if <see langword="false"/>, or an empty string if null.
        /// If the input value is not of <c>Boolean</c> type or <c>Null</c>,
        /// the unmodified input value is returned.
        /// </summary>
        /// <param name="value">The input value to convert.</param>
        /// <returns>
        /// The check mark button icon ✅ (&amp;#9989;) if <c>value</c> is <see langword="true"/>,
        /// the cross mark icon ❌ (&amp;#10060;) if <c>value</c> is <see langword="false"/>,
        /// an empty <c>String</c> if <c>value</c> is <c>Null</c>,
        /// or the unmodified input value otherwise.
        /// </returns>
        protected string ConvertBooleanToIcon(object value)
        {
            if (value is null)
            {
                return string.Empty;
            }

            if (typeof(bool) != value.GetType())
            {
                return $"{value}";
            }

            return ((bool)value) ? "&#9989;" : "&#10060;";
        }

        private static Table CreateTable(IEnumerable<string> items, int maxRows, int maxColumns)
        {
            var capacity = maxRows * maxColumns;

            var row = new TableRow();
            var list = StartNewColumn(row);

            foreach (var item in items.Take(capacity - 1))
            {
                list.Items.Add(new ListItem(item));

                if (list.Items.Count == maxRows)
                {
                    list = StartNewColumn(row);
                }
            }

            if (capacity < items.Count())
            {
                list.Items.Add(new ListItem("..."));
            }

            foreach (TableCell cell in row.Cells)
            {
                cell.Width = Unit.Percentage(100.0 / row.Cells.Count);
            }

            var table = new Table
            {
                Rows = { row },
                Width = Unit.Percentage(100.0),
            };

            return table;
        }

        private static ListControl StartNewColumn(TableRow row)
        {
            var list = new BulletedList();

            var cell = new TableCell
            {
                Controls = { list },
                VerticalAlign = VerticalAlign.Top,
            };

            row.Cells.Add(cell);

            return list;
        }

        private void InitMainMultiView()
        {
            if (!this.UserInfo.IsSuperUser)
            {
                this.MainMultiView.ActiveViewIndex = 0; // RegularUserView
                return;
            }

            var telerikUtils = this.GetService<ITelerikUtils>();

            if (!telerikUtils.TelerikIsInstalled())
            {
                this.MainMultiView.ActiveViewIndex = 1; // NotInstalledView
                return;
            }

            this.MainMultiView.ActiveViewIndex = 2; // InstalledView

            var version = telerikUtils.GetTelerikVersion().ToString();
            this.TelerikInstalledVersionLabel.Text = version;

            var assemblies = telerikUtils.GetAssembliesThatDependOnTelerik()
                .Select(a => Path.GetFileName(a));

            if (!assemblies.Any())
            {
                this.InstalledMultiView.ActiveViewIndex = 0; // InstalledButNotUsedView
                return;
            }

            this.InstalledMultiView.ActiveViewIndex = 1; // InstalledAndUsedView

            this.TelerikInstalledAndUsedInfoLabel.Text = $"TelerikInstalledAndUsedInfo";
            this.TelerikInstalledAndUsedWarningLabel.Text = $"TelerikInstalledAndUsedWarning";

            var table = CreateTable(assemblies, maxRows: 3, maxColumns: 3);
            this.AssemblyListPlaceHolder.Controls.Add(table);
        }

        private void InitRemoveTelerikButton()
        {
            this.RemoveTelerikButton.Enabled = false;
        }

        private void RegisterClientScripts()
        {
            this.Page.ClientScript.RegisterOnSubmitStatement(
                typeof(View),
                nameof(View),
                "$('#telerikRemoval').addClass('running');");
        }

        private T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
