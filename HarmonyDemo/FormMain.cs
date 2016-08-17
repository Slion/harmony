using HarmonyHub;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HarmonyDemo
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {        
            // ConnectAsync already if we have an existing session cookie
            if (File.Exists("SessionToken"))
            {
                buttonOpen.Enabled = false;
                try
                {
                    await ConnectAsync();
                }
                finally
                {
                    buttonOpen.Enabled = true;
                }
            }

        }

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();

            buttonOpen.Enabled = false;
            try
            {
                await ConnectAsync();
            }
            finally
            {
                buttonOpen.Enabled = true;
            }
        }


        private async Task ConnectAsync()
        {

            toolStripStatusLabelConnection.Text = "Connecting... ";
            Program.Client = new Client(textBoxHarmonyHubAddress.Text);
            //First create our client and login
            if (File.Exists("SessionToken"))
            {
                var sessionToken = File.ReadAllText("SessionToken");
                Console.WriteLine("Reusing token: {0}", sessionToken);
                toolStripStatusLabelConnection.Text += $"Reusing token: {sessionToken}";
                Program.Client.Open(sessionToken);
            }
            else
            {
                if (string.IsNullOrEmpty(textBoxPassword.Text))
                {
                    toolStripStatusLabelConnection.Text = "Credentials missing!";
                    return;
                }

                toolStripStatusLabelConnection.Text += "authenticating with Logitech servers...";
                await Program.Client.Open(textBoxUserName.Text, textBoxPassword.Text);
                File.WriteAllText("SessionToken", Program.Client.Token);
            }

            toolStripStatusLabelConnection.Text = "Fetching Harmony Hub configuration...";

            //Fetch our config
            var harmonyConfig = await Program.Client.GetConfigAsync();
            PopulateTreeViewConfig(harmonyConfig);

            toolStripStatusLabelConnection.Text = "Ready";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aConfig"></param>
        private void PopulateTreeViewConfig(Config aConfig)
        {
            treeViewConfig.Nodes.Clear();
            //Add our devices
            foreach (Device device in aConfig.Devices)
            {
                TreeNode deviceNode = treeViewConfig.Nodes.Add(device.Id, $"{device.Label} ({device.DeviceTypeDisplayName}/{device.Model})");
                deviceNode.Tag = device;

                foreach (ControlGroup cg in device.ControlGroups)
                {
                    TreeNode cgNode = deviceNode.Nodes.Add(cg.Name);
                    cgNode.Tag = cg;

                    foreach (Function f in cg.Functions)
                    {
                        TreeNode fNode = cgNode.Nodes.Add(f.Name);
                        fNode.Tag = f;
                    }
                }
            }

            //treeViewConfig.ExpandAll();
        }

        private async void treeViewConfig_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //Upon function node double click we execute it
            var tag = e.Node.Tag as Function;
            if (tag != null && e.Node.Parent.Parent.Tag is Device)
            {
                Function f = tag;
                Device d = (Device)e.Node.Parent.Parent.Tag;

                toolStripStatusLabelConnection.Text = $"Sending {f.Name} to {d.Label}...";

                await Program.Client.SendCommandAsync(d.Id,f.Name);
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.HarmonyHubAddress = textBoxHarmonyHubAddress.Text;
            Properties.Settings.Default.Save();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            if (Program.Client != null)
            {
                Program.Client.Close();
            }
            treeViewConfig.Nodes.Clear();
        }

        private void buttonDeleteToken_Click(object sender, EventArgs e)
        {
            File.Delete("SessionToken");
        }
    }
}
