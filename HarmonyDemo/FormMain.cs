using HarmonyHub;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HarmonyDemo
{
    public partial class FormMain : Form
    {
        RichTextBoxTraceListener iLogsTraceListener;
        HarmonyTraceListener iHarmonTraceListener;

        public FormMain()
        {
            InitializeComponent();

            //Redirect traces
            iLogsTraceListener = new RichTextBoxTraceListener(richTextBoxLogs);
            iHarmonTraceListener = new HarmonyTraceListener(toolStripStatusLabelConnection);
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            // ConnectAsync already if we have an existing session cookie
            if (File.Exists("SessionToken"))
            {
                await HarmonyConnectAsync();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task HarmonyOpenAsync()
        {
            //Create client if it does not already exists or the hub address has changed
            if (Program.Client == null || !Program.Client.Host.Equals(textBoxHarmonyHubAddress.Text))
            {
                Program.Client = new Client(textBoxHarmonyHubAddress.Text);
                Program.Client.OnTaskChanged += TaskChangedHandler;
                Program.Client.OnConnectionClosedByServer += ConnectionClosedByServerHandler;
            }

            //First create our client and login
            if (File.Exists("SessionToken"))
            {
                var sessionToken = File.ReadAllText("SessionToken");
                Trace.WriteLine("Reusing token: {0}", sessionToken);
                toolStripStatusLabelConnection.Text += $"Reusing token: {sessionToken}";
                await Program.Client.TryOpenAsync(sessionToken);
            }
            else
            {
                if (string.IsNullOrEmpty(textBoxPassword.Text))
                {
                    toolStripStatusLabelConnection.Text = "Credentials missing!";
                    return;
                }

                await Program.Client.TryOpenAsync(textBoxUserName.Text, textBoxPassword.Text);
                File.WriteAllText("SessionToken", Program.Client.Token);
            }
        }

        /// <summary>
        /// Display our client status
        /// </summary>
        /// <param name="aSender"></param>
        /// <param name="aRequestPending"></param>
        void TaskChangedHandler(object aSender, bool aRequestPending)
        {
            // Consistency check
            Debug.Assert(aRequestPending == Program.Client.RequestPending);

            // Display request status
            if (aRequestPending)
            {
                toolStripStatusLabelRequest.Text = "Busy - ";
            }
            else if (Program.Client.IsOpen)
            {
                toolStripStatusLabelRequest.Text = "Open - ";
            }
            else if (Program.Client.IsClosed)
            {
                toolStripStatusLabelRequest.Text = "Closed - ";
            }

            toolStripStatusLabelRequest.Text += aRequestPending ? "Request pending" : "Request completed";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aSender"></param>
        /// <param name="aRequestWasCancelled"></param>
        void ConnectionClosedByServerHandler(object aSender, bool aRequestWasCancelled)
        {
            // Consistency check
            Debug.Assert(Program.Client.IsClosed);

            // Try opening our connection again to keep it alive
            treeViewConfig.Nodes.Clear();
            // Don't wait for results
            HarmonyConnectAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task HarmonyConnectAsync()
        {
            await HarmonyOpenAsync();

            //Added if statement to make sure "Missing credential" status remains if needed
            if (Program.Client.IsReady)
            {
                await HarmonyGetConfigAsync();
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task HarmonyGetConfigAsync()
        {
            //Fetch our config
            var harmonyConfig = await Program.Client.GetConfigAsync();
            if (harmonyConfig == null)
            {
                return;
            }
            PopulateTreeViewConfig(harmonyConfig);
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


        private async void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.HarmonyHubAddress = textBoxHarmonyHubAddress.Text;
            Properties.Settings.Default.Save();

            //Closing properly
            await Program.Client.CloseAsync();
        }

        private async void buttonClose_Click(object sender, EventArgs e)
        {
            if (Program.Client != null)
            {
                await Program.Client.CloseAsync();
            }
            treeViewConfig.Nodes.Clear();
        }

        private void buttonDeleteToken_Click(object sender, EventArgs e)
        {
            File.Delete("SessionToken");
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

                await Program.Client.SendCommandAsync(d.Id, f.Name);
            }
        }

        private async void buttonOpen_Click(object sender, EventArgs e)
        {
            await HarmonyOpenAsync();
        }

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            await HarmonyConnectAsync();
        }

        private async void buttonConfig_Click(object sender, EventArgs e)
        {
            await HarmonyGetConfigAsync();
        }

        /// <summary>
        /// We don't disable buttons ATM thus we can stress test our APIs.
        /// </summary>
        private void DisableButtons()
        {
            buttonClose.Enabled = false;
            buttonOpen.Enabled = false;
            buttonConnect.Enabled = false;
            buttonConfig.Enabled = false;
        }

        private void buttonClearLogs_Click(object sender, EventArgs e)
        {
            richTextBoxLogs.Clear();
        }
    }
}
