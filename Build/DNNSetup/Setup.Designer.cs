namespace DotNetNukeSetup
{
    partial class Setup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.folderSiteRoot = new System.Windows.Forms.FolderBrowserDialog();
            this.txtPackage = new System.Windows.Forms.TextBox();
            this.txtSiteRoot = new System.Windows.Forms.TextBox();
            this.btnSelectPackage = new System.Windows.Forms.Button();
            this.btnSiteRoot = new System.Windows.Forms.Button();
            this.openPackage = new System.Windows.Forms.OpenFileDialog();
            this.txtDatabaseServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSetup = new System.Windows.Forms.Button();
            this.btnAddSocial = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.txtWebSite = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtObjectQualifier = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDatabaseName = new System.Windows.Forms.TextBox();
            this.btnUsersRoles = new System.Windows.Forms.Button();
            this.chkSocial = new System.Windows.Forms.CheckBox();
            this.btnOpenIE = new System.Windows.Forms.Button();
            this.btnBigGreenButton = new System.Windows.Forms.Button();
            this.btnCreateCoreModulePages = new System.Windows.Forms.Button();
            this.btnAddFolderFiles = new System.Windows.Forms.Button();
            this.cmbFoldersPerLevel = new System.Windows.Forms.ComboBox();
            this.cmbLevels = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.cmbFilesPerFolder = new System.Windows.Forms.ComboBox();
            this.chkVolume = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbModulesPerPage = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.cmbPageLevels = new System.Windows.Forms.ComboBox();
            this.cmdPagesPerLevel = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.cmbRootPages = new System.Windows.Forms.ComboBox();
            this.btnSmtp = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtPackage
            // 
            this.txtPackage.Location = new System.Drawing.Point(107, 80);
            this.txtPackage.Name = "txtPackage";
            this.txtPackage.Size = new System.Drawing.Size(210, 20);
            this.txtPackage.TabIndex = 0;
            // 
            // txtSiteRoot
            // 
            this.txtSiteRoot.Location = new System.Drawing.Point(106, 106);
            this.txtSiteRoot.Name = "txtSiteRoot";
            this.txtSiteRoot.Size = new System.Drawing.Size(210, 20);
            this.txtSiteRoot.TabIndex = 1;
            // 
            // btnSelectPackage
            // 
            this.btnSelectPackage.Location = new System.Drawing.Point(323, 80);
            this.btnSelectPackage.Name = "btnSelectPackage";
            this.btnSelectPackage.Size = new System.Drawing.Size(25, 20);
            this.btnSelectPackage.TabIndex = 2;
            this.btnSelectPackage.Text = "...";
            this.btnSelectPackage.UseVisualStyleBackColor = true;
            this.btnSelectPackage.Click += new System.EventHandler(this.btnSelectPackage_Click);
            // 
            // btnSiteRoot
            // 
            this.btnSiteRoot.Location = new System.Drawing.Point(323, 105);
            this.btnSiteRoot.Name = "btnSiteRoot";
            this.btnSiteRoot.Size = new System.Drawing.Size(25, 20);
            this.btnSiteRoot.TabIndex = 3;
            this.btnSiteRoot.Text = "...";
            this.btnSiteRoot.UseVisualStyleBackColor = true;
            this.btnSiteRoot.Click += new System.EventHandler(this.btnSiteRoot_Click);
            // 
            // openPackage
            // 
            this.openPackage.Filter = "DotNetNuke Install Packages|*.zip";
            // 
            // txtDatabaseServer
            // 
            this.txtDatabaseServer.Location = new System.Drawing.Point(106, 185);
            this.txtDatabaseServer.Name = "txtDatabaseServer";
            this.txtDatabaseServer.Size = new System.Drawing.Size(210, 20);
            this.txtDatabaseServer.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Package Location";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 109);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Site Root Folder";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 187);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Database Server";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnSetup
            // 
            this.btnSetup.Location = new System.Drawing.Point(164, 131);
            this.btnSetup.Name = "btnSetup";
            this.btnSetup.Size = new System.Drawing.Size(152, 23);
            this.btnSetup.TabIndex = 10;
            this.btnSetup.Text = "Setup ";
            this.btnSetup.UseVisualStyleBackColor = true;
            this.btnSetup.Click += new System.EventHandler(this.btnSetup_Click);
            // 
            // btnAddSocial
            // 
            this.btnAddSocial.Location = new System.Drawing.Point(160, 603);
            this.btnAddSocial.Name = "btnAddSocial";
            this.btnAddSocial.Size = new System.Drawing.Size(150, 23);
            this.btnAddSocial.TabIndex = 11;
            this.btnAddSocial.Text = "Add Social Data";
            this.btnAddSocial.UseVisualStyleBackColor = true;
            this.btnAddSocial.Click += new System.EventHandler(this.btnAddSocial_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 686);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(869, 22);
            this.statusStrip1.TabIndex = 13;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(90, 17);
            this.toolStripStatusLabel1.Text = "DNN Site Demo";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 265);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Site to Use";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtWebSite
            // 
            this.txtWebSite.Location = new System.Drawing.Point(105, 262);
            this.txtWebSite.Name = "txtWebSite";
            this.txtWebSite.Size = new System.Drawing.Size(210, 20);
            this.txtWebSite.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 238);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Object Qualifier";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtObjectQualifier
            // 
            this.txtObjectQualifier.Location = new System.Drawing.Point(105, 236);
            this.txtObjectQualifier.Name = "txtObjectQualifier";
            this.txtObjectQualifier.Size = new System.Drawing.Size(210, 20);
            this.txtObjectQualifier.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 213);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Database Name";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtDatabaseName
            // 
            this.txtDatabaseName.Location = new System.Drawing.Point(106, 211);
            this.txtDatabaseName.Name = "txtDatabaseName";
            this.txtDatabaseName.Size = new System.Drawing.Size(210, 20);
            this.txtDatabaseName.TabIndex = 17;
            // 
            // btnUsersRoles
            // 
            this.btnUsersRoles.Location = new System.Drawing.Point(164, 331);
            this.btnUsersRoles.Name = "btnUsersRoles";
            this.btnUsersRoles.Size = new System.Drawing.Size(150, 23);
            this.btnUsersRoles.TabIndex = 19;
            this.btnUsersRoles.Text = "Add Users and Roles";
            this.btnUsersRoles.UseVisualStyleBackColor = true;
            this.btnUsersRoles.Click += new System.EventHandler(this.btnUsersRoles_Click);
            // 
            // chkSocial
            // 
            this.chkSocial.AutoSize = true;
            this.chkSocial.Location = new System.Drawing.Point(107, 603);
            this.chkSocial.Name = "chkSocial";
            this.chkSocial.Size = new System.Drawing.Size(55, 17);
            this.chkSocial.TabIndex = 20;
            this.chkSocial.Text = "Social";
            this.chkSocial.UseVisualStyleBackColor = true;
            this.chkSocial.CheckedChanged += new System.EventHandler(this.chkSocial_CheckedChanged);
            // 
            // btnOpenIE
            // 
            this.btnOpenIE.Location = new System.Drawing.Point(160, 633);
            this.btnOpenIE.Name = "btnOpenIE";
            this.btnOpenIE.Size = new System.Drawing.Size(151, 23);
            this.btnOpenIE.TabIndex = 21;
            this.btnOpenIE.Text = "Open IE";
            this.btnOpenIE.UseVisualStyleBackColor = true;
            this.btnOpenIE.Click += new System.EventHandler(this.btnOpenIE_Click);
            // 
            // btnBigGreenButton
            // 
            this.btnBigGreenButton.BackColor = System.Drawing.Color.Lime;
            this.btnBigGreenButton.Location = new System.Drawing.Point(14, 1);
            this.btnBigGreenButton.Name = "btnBigGreenButton";
            this.btnBigGreenButton.Size = new System.Drawing.Size(334, 73);
            this.btnBigGreenButton.TabIndex = 22;
            this.btnBigGreenButton.Text = "Big Green Button";
            this.btnBigGreenButton.UseVisualStyleBackColor = false;
            this.btnBigGreenButton.Click += new System.EventHandler(this.btnBigGreenButton_Click);
            // 
            // btnCreateCoreModulePages
            // 
            this.btnCreateCoreModulePages.Location = new System.Drawing.Point(164, 463);
            this.btnCreateCoreModulePages.Name = "btnCreateCoreModulePages";
            this.btnCreateCoreModulePages.Size = new System.Drawing.Size(149, 23);
            this.btnCreateCoreModulePages.TabIndex = 23;
            this.btnCreateCoreModulePages.Text = "Add Module Pages";
            this.btnCreateCoreModulePages.UseVisualStyleBackColor = true;
            this.btnCreateCoreModulePages.Click += new System.EventHandler(this.btnCreateCoreModulePages_Click);
            // 
            // btnAddFolderFiles
            // 
            this.btnAddFolderFiles.Location = new System.Drawing.Point(164, 565);
            this.btnAddFolderFiles.Name = "btnAddFolderFiles";
            this.btnAddFolderFiles.Size = new System.Drawing.Size(147, 23);
            this.btnAddFolderFiles.TabIndex = 24;
            this.btnAddFolderFiles.Text = "Add Folders and Files";
            this.btnAddFolderFiles.UseVisualStyleBackColor = true;
            this.btnAddFolderFiles.Click += new System.EventHandler(this.btnAddFolderFiles_Click);
            // 
            // cmbFoldersPerLevel
            // 
            this.cmbFoldersPerLevel.FormattingEnabled = true;
            this.cmbFoldersPerLevel.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbFoldersPerLevel.Location = new System.Drawing.Point(106, 516);
            this.cmbFoldersPerLevel.Name = "cmbFoldersPerLevel";
            this.cmbFoldersPerLevel.Size = new System.Drawing.Size(53, 21);
            this.cmbFoldersPerLevel.TabIndex = 25;
            this.cmbFoldersPerLevel.Text = "2";
            // 
            // cmbLevels
            // 
            this.cmbLevels.FormattingEnabled = true;
            this.cmbLevels.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbLevels.Location = new System.Drawing.Point(105, 489);
            this.cmbLevels.Name = "cmbLevels";
            this.cmbLevels.Size = new System.Drawing.Size(54, 21);
            this.cmbLevels.TabIndex = 26;
            this.cmbLevels.Text = "2";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 492);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 13);
            this.label7.TabIndex = 27;
            this.label7.Text = "Levels";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 516);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(84, 13);
            this.label8.TabIndex = 28;
            this.label8.Text = "Folders per level";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 546);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(75, 13);
            this.label9.TabIndex = 30;
            this.label9.Text = "Files per folder";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbFilesPerFolder
            // 
            this.cmbFilesPerFolder.FormattingEnabled = true;
            this.cmbFilesPerFolder.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbFilesPerFolder.Location = new System.Drawing.Point(107, 546);
            this.cmbFilesPerFolder.Name = "cmbFilesPerFolder";
            this.cmbFilesPerFolder.Size = new System.Drawing.Size(53, 21);
            this.cmbFilesPerFolder.TabIndex = 29;
            this.cmbFilesPerFolder.Text = "5";
            // 
            // chkVolume
            // 
            this.chkVolume.AutoSize = true;
            this.chkVolume.Location = new System.Drawing.Point(105, 301);
            this.chkVolume.Name = "chkVolume";
            this.chkVolume.Size = new System.Drawing.Size(98, 17);
            this.chkVolume.TabIndex = 31;
            this.chkVolume.Text = "Is Data Volume";
            this.chkVolume.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 441);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(92, 13);
            this.label10.TabIndex = 37;
            this.label10.Text = "Modules per page";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbModulesPerPage
            // 
            this.cmbModulesPerPage.FormattingEnabled = true;
            this.cmbModulesPerPage.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbModulesPerPage.Location = new System.Drawing.Point(107, 441);
            this.cmbModulesPerPage.Name = "cmbModulesPerPage";
            this.cmbModulesPerPage.Size = new System.Drawing.Size(53, 21);
            this.cmbModulesPerPage.TabIndex = 36;
            this.cmbModulesPerPage.Text = "3";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 411);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(80, 13);
            this.label11.TabIndex = 35;
            this.label11.Text = "Pages per level";
            this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 387);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(38, 13);
            this.label12.TabIndex = 34;
            this.label12.Text = "Levels";
            this.label12.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbPageLevels
            // 
            this.cmbPageLevels.FormattingEnabled = true;
            this.cmbPageLevels.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbPageLevels.Location = new System.Drawing.Point(105, 384);
            this.cmbPageLevels.Name = "cmbPageLevels";
            this.cmbPageLevels.Size = new System.Drawing.Size(54, 21);
            this.cmbPageLevels.TabIndex = 33;
            this.cmbPageLevels.Text = "3";
            // 
            // cmdPagesPerLevel
            // 
            this.cmdPagesPerLevel.FormattingEnabled = true;
            this.cmdPagesPerLevel.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmdPagesPerLevel.Location = new System.Drawing.Point(106, 411);
            this.cmdPagesPerLevel.Name = "cmdPagesPerLevel";
            this.cmdPagesPerLevel.Size = new System.Drawing.Size(53, 21);
            this.cmdPagesPerLevel.TabIndex = 32;
            this.cmdPagesPerLevel.Text = "3";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 357);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(63, 13);
            this.label13.TabIndex = 39;
            this.label13.Text = "Root Pages";
            this.label13.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbRootPages
            // 
            this.cmbRootPages.FormattingEnabled = true;
            this.cmbRootPages.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbRootPages.Location = new System.Drawing.Point(105, 354);
            this.cmbRootPages.Name = "cmbRootPages";
            this.cmbRootPages.Size = new System.Drawing.Size(54, 21);
            this.cmbRootPages.TabIndex = 38;
            this.cmbRootPages.Text = "3";
            // 
            // btnSmtp
            // 
            this.btnSmtp.Location = new System.Drawing.Point(164, 158);
            this.btnSmtp.Name = "btnSmtp";
            this.btnSmtp.Size = new System.Drawing.Size(150, 23);
            this.btnSmtp.TabIndex = 40;
            this.btnSmtp.Text = "SMTP";
            this.btnSmtp.UseVisualStyleBackColor = true;
            this.btnSmtp.Click += new System.EventHandler(this.btnSmtp_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(600, 158);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 41;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 708);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnSmtp);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.cmbRootPages);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cmbModulesPerPage);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.cmbPageLevels);
            this.Controls.Add(this.cmdPagesPerLevel);
            this.Controls.Add(this.chkVolume);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.cmbFilesPerFolder);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cmbLevels);
            this.Controls.Add(this.cmbFoldersPerLevel);
            this.Controls.Add(this.btnAddFolderFiles);
            this.Controls.Add(this.btnCreateCoreModulePages);
            this.Controls.Add(this.btnBigGreenButton);
            this.Controls.Add(this.btnOpenIE);
            this.Controls.Add(this.chkSocial);
            this.Controls.Add(this.btnUsersRoles);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtDatabaseName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtObjectQualifier);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.txtWebSite);
            this.Controls.Add(this.btnAddSocial);
            this.Controls.Add(this.btnSetup);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDatabaseServer);
            this.Controls.Add(this.btnSiteRoot);
            this.Controls.Add(this.btnSelectPackage);
            this.Controls.Add(this.txtSiteRoot);
            this.Controls.Add(this.txtPackage);
            this.Name = "Setup";
            this.Text = "Setup DotNetNuke Site";
            this.Load += new System.EventHandler(this.Setup_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderSiteRoot;
        private System.Windows.Forms.TextBox txtPackage;
        private System.Windows.Forms.TextBox txtSiteRoot;
        private System.Windows.Forms.Button btnSelectPackage;
        private System.Windows.Forms.Button btnSiteRoot;
        private System.Windows.Forms.OpenFileDialog openPackage;
        private System.Windows.Forms.TextBox txtDatabaseServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSetup;
        private System.Windows.Forms.Button btnAddSocial;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtWebSite;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtObjectQualifier;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDatabaseName;
        private System.Windows.Forms.Button btnUsersRoles;
        private System.Windows.Forms.CheckBox chkSocial;
        private System.Windows.Forms.Button btnOpenIE;
        private System.Windows.Forms.Button btnBigGreenButton;
        private System.Windows.Forms.Button btnCreateCoreModulePages;
        private System.Windows.Forms.Button btnAddFolderFiles;
        private System.Windows.Forms.ComboBox cmbFoldersPerLevel;
        private System.Windows.Forms.ComboBox cmbLevels;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmbFilesPerFolder;
        private System.Windows.Forms.CheckBox chkVolume;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cmbModulesPerPage;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cmbPageLevels;
        private System.Windows.Forms.ComboBox cmdPagesPerLevel;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox cmbRootPages;
        private System.Windows.Forms.Button btnSmtp;
        private System.Windows.Forms.Button button1;
    }
}

