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
            this.components = new System.ComponentModel.Container();
            this.textBoxHarmonyHubAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelConnection = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelSpring = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelRequest = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageHarmony = new System.Windows.Forms.TabPage();
            this.buttonConfig = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonDeleteToken = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.treeViewConfig = new System.Windows.Forms.TreeView();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.tabPageLogs = new System.Windows.Forms.TabPage();
            this.buttonClearLogs = new System.Windows.Forms.Button();
            this.richTextBoxLogs = new System.Windows.Forms.RichTextBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxKeepAlive = new System.Windows.Forms.CheckBox();
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
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelConnection,
            this.toolStripStatusLabelSpring,
            this.toolStripStatusLabelRequest});
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
            // toolStripStatusLabelSpring
            // 
            this.toolStripStatusLabelSpring.Name = "toolStripStatusLabelSpring";
            this.toolStripStatusLabelSpring.Size = new System.Drawing.Size(477, 17);
            this.toolStripStatusLabelSpring.Spring = true;
            // 
            // toolStripStatusLabelRequest
            // 
            this.toolStripStatusLabelRequest.Name = "toolStripStatusLabelRequest";
            this.toolStripStatusLabelRequest.Size = new System.Drawing.Size(84, 17);
            this.toolStripStatusLabelRequest.Text = "Request Status";
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
            this.tabPageHarmony.Controls.Add(this.buttonConfig);
            this.tabPageHarmony.Controls.Add(this.buttonConnect);
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
            // buttonConfig
            // 
            this.buttonConfig.Location = new System.Drawing.Point(6, 93);
            this.buttonConfig.Name = "buttonConfig";
            this.buttonConfig.Size = new System.Drawing.Size(101, 23);
            this.buttonConfig.TabIndex = 16;
            this.buttonConfig.Text = "Config";
            this.buttonConfig.UseVisualStyleBackColor = true;
            this.buttonConfig.Click += new System.EventHandler(this.buttonConfig_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(6, 6);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(101, 23);
            this.buttonConnect.TabIndex = 15;
            this.buttonConnect.Text = "Connect";
            this.toolTip.SetToolTip(this.buttonConnect, "Open and get config");
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonDeleteToken
            // 
            this.buttonDeleteToken.Location = new System.Drawing.Point(6, 122);
            this.buttonDeleteToken.Name = "buttonDeleteToken";
            this.buttonDeleteToken.Size = new System.Drawing.Size(101, 23);
            this.buttonDeleteToken.TabIndex = 14;
            this.buttonDeleteToken.Text = "Delete Token";
            this.buttonDeleteToken.UseVisualStyleBackColor = true;
            this.buttonDeleteToken.Click += new System.EventHandler(this.buttonDeleteToken_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(6, 64);
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
            this.buttonOpen.Location = new System.Drawing.Point(6, 35);
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
            this.buttonClearLogs.Click += new System.EventHandler(this.buttonClearLogs_Click);
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
            // checkBoxKeepAlive
            // 
            this.checkBoxKeepAlive.AutoSize = true;
            this.checkBoxKeepAlive.Location = new System.Drawing.Point(206, 22);
            this.checkBoxKeepAlive.Name = "checkBoxKeepAlive";
            this.checkBoxKeepAlive.Size = new System.Drawing.Size(76, 17);
            this.checkBoxKeepAlive.TabIndex = 12;
            this.checkBoxKeepAlive.Text = "Keep alive";
            this.checkBoxKeepAlive.UseVisualStyleBackColor = true;
            this.checkBoxKeepAlive.CheckedChanged += new System.EventHandler(this.checkBoxKeepAlive_CheckedChanged);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 579);
            this.Controls.Add(this.checkBoxKeepAlive);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.statusStrip);
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
        private System.Windows.Forms.Button buttonConfig;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSpring;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelRequest;
        private System.Windows.Forms.CheckBox checkBoxKeepAlive;
    }
}

