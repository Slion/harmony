using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace HarmonyDemo
{
    /// <summary>
    /// Redirected Trace and Debug output to the given Rich Text Box.
    /// 
    /// </summary>
    public class HarmonyTraceListener : TraceListener
    {
        ToolStripStatusLabel iLabel = null;

        public HarmonyTraceListener(ToolStripStatusLabel aLabel)
        {
            iLabel = aLabel;
            Debug.Listeners.Add(this);
        }

        public override void WriteLine(string aString)
        {
            Write(aString);
        }

        public override void Write(string aString)
        {
            if (aString.StartsWith("Harmony:"))
            {
                iLabel.Text = aString;
            }
        }

    }
}