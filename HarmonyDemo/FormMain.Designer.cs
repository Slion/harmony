namespace HarmonyDemo
{
    partial class FormMain
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
            this.textBoxHarmonyHubAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelLogitechUserName = new System.Windows.Forms.Label();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelConnection = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageHarmony = new System.Windows.Forms.TabPage();
            this.buttonDeleteToken = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.treeViewConfig = new System.Windows.Forms.TreeView();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.tabPageLogs = new System.Windows.Forms.TabPage();
            this.buttonClearLogs = new System.Windows.Forms.Button();
            this.richTextBoxLogs = new System.Windows.Forms.RichTextBox();
            this.statusStrip.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.tabPageHarmony.SuspendLayout();
            this.tabPageLogs.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxHarmonyHubAddress
            // 
            this.textBoxHarmonyHubAddress.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::HarmonyDemo.Properties.Settings.Default, "HarmonyHubAddress", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBoxHarmonyHubAddress.Location = new System.Drawing.Point(35, 38);
            this.textBoxHarmonyHubAddress.Name = "textBoxHarmonyHubAddress";
            this.textBoxHarmonyHubAddress.Size = new System.Drawing.Size(100, 20);
            this.textBoxHarmonyHubAddress.TabIndex = 0;
            this.textBoxHarmonyHubAddress.Text = global::HarmonyDemo.Properties.Settings.Default.HarmonyHubAddress;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Harmony Hub Address:";
            // 
            // labelLogitechUserName
            // 
            this.labelLogitechUserName.AutoSize = true;
            this.labelLogitechUserName.Location = new System.Drawing.Point(186, 22);
            this.labelLogitechUserName.Name = "labelLogitechUserName";
            this.labelLogitechUserName.Size = new System.Drawing.Size(103, 13);
            this.labelLogitechUserName.TabIndex = 3;
            this.labelLogitechUserName.Text = "Logitech user name:";
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::HarmonyDemo.Properties.Settings.Default, "LogitechUserName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBoxUserName.Location = new System.Drawing.Point(189, 38);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(134, 20);
            this.textBoxUserName.TabIndex = 2;
            this.textBoxUserName.Text = global::HarmonyDemo.Properties.Settings.Default.LogitechUserName;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(350, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Logitech password:";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(353, 38);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(134, 20);
            this.textBoxPassword.TabIndex = 4;
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelConnection});
            this.statusStrip.Location = new System.Drawing.Point(0, 557);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(680, 22);
            this.statusStrip.TabIndex = 8;
            this.statusStrip.Text = "App Status";
            // 
            // toolStripStatusLabelConnection
            // 
            this.toolStripStatusLabelConnection.Name = "toolStripStatusLabelConnection";
            this.toolStripStatusLabelConnection.Size = new System.Drawing.Size(104, 17);
            this.toolStripStatusLabelConnection.Text = "Connection Status";
            // 
            // tabControlMain
            // 
            this.tabControlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlMain.Controls.Add(this.tabPageHarmony);
            this.tabControlMain.Controls.Add(this.tabPageLogs);
            this.tabControlMain.Location = new System.Drawing.Point(12, 64);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(656, 473);
            this.tabControlMain.TabIndex = 11;
            // 
            // tabPageHarmony
            // 
            this.tabPageHarmony.Controls.Add(this.buttonDeleteToken);
            this.tabPageHarmony.Controls.Add(this.buttonClose);
            this.tabPageHarmony.Controls.Add(this.treeViewConfig);
            this.tabPageHarmony.Controls.Add(this.buttonOpen);
            this.tabPageHarmony.Location = new System.Drawing.Point(4, 22);
            this.tabPageHarmony.Name = "tabPageHarmony";
            this.tabPageHarmony.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHarmony.Size = new System.Drawing.Size(648, 447);
            this.tabPageHarmony.TabIndex = 0;
            this.tabPageHarmony.Text = "Harmony";
            this.tabPageHarmony.UseVisualStyleBackColor = true;
            // 
            // buttonDeleteToken
            // 
            this.buttonDeleteToken.Location = new System.Drawing.Point(6, 64);
            this.buttonDeleteToken.Name = "buttonDeleteToken";
            this.buttonDeleteToken.Size = new System.Drawing.Size(101, 23);
            this.buttonDeleteToken.TabIndex = 14;
            this.buttonDeleteToken.Text = "Delete Token";
            this.buttonDeleteToken.UseVisualStyleBackColor = true;
            this.buttonDeleteToken.Click += new System.EventHandler(this.buttonDeleteToken_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(6, 35);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(101, 23);
            this.buttonClose.TabIndex = 13;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // treeViewConfig
            // 
            this.treeViewConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewConfig.Location = new System.Drawing.Point(122, 6);
            this.treeViewConfig.Name = "treeViewConfig";
            this.treeViewConfig.Size = new System.Drawing.Size(520, 435);
            this.treeViewConfig.TabIndex = 12;
            this.treeViewConfig.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewConfig_NodeMouseDoubleClick);
            // 
            // buttonOpen
            // 
            this.buttonOpen.Location = new System.Drawing.Point(6, 6);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(101, 23);
            this.buttonOpen.TabIndex = 11;
            this.buttonOpen.Text = "Open";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // tabPageLogs
            // 
            this.tabPageLogs.Controls.Add(this.buttonClearLogs);
            this.tabPageLogs.Controls.Add(this.richTextBoxLogs);
            this.tabPageLogs.Location = new System.Drawing.Point(4, 22);
            this.tabPageLogs.Name = "tabPageLogs";
            this.tabPageLogs.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogs.Size = new System.Drawing.Size(648, 447);
            this.tabPageLogs.TabIndex = 1;
            this.tabPageLogs.Text = "Logs";
            this.tabPageLogs.UseVisualStyleBackColor = true;
            // 
            // buttonClearLogs
            // 
            this.buttonClearLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearLogs.Location = new System.Drawing.Point(567, 6);
            this.buttonClearLogs.Name = "buttonClearLogs";
            this.buttonClearLogs.Size = new System.Drawing.Size(75, 23);
            this.buttonClearLogs.TabIndex = 4;
            this.buttonClearLogs.Text = "Clear";
            this.buttonClearLogs.UseVisualStyleBackColor = true;
            // 
            // richTextBoxLogs
            // 
            this.richTextBoxLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxLogs.DetectUrls = false;
            this.richTextBoxLogs.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxLogs.Location = new System.Drawing.Point(6, 6);
            this.richTextBoxLogs.Name = "richTextBoxLogs";
            this.richTextBoxLogs.ReadOnly = true;
            this.richTextBoxLogs.Size = new System.Drawing.Size(636, 435);
            this.richTextBoxLogs.TabIndex = 3;
            this.richTextBoxLogs.Text = "";
            this.richTextBoxLogs.WordWrap = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 579);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.labelLogitechUserName);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxHarmonyHubAddress);
            this.Name = "FormMain";
            this.Text = "Harmony Demo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.tabControlMain.ResumeLayout(false);
            this.tabPageHarmony.ResumeLayout(false);
            this.tabPageLogs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxHarmonyHubAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelLogitechUserName;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelConnection;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageHarmony;
        private System.Windows.Forms.TabPage tabPageLogs;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.TreeView treeViewConfig;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonDeleteToken;
        private System.Windows.Forms.Button buttonClearLogs;
        private System.Windows.Forms.RichTextBox richTextBoxLogs;
    }
}

