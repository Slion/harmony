﻿using System;
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
    public class RichTextBoxTraceListener : TraceListener
    {
        RichTextBox iRichTextBox = null;

        public RichTextBoxTraceListener(RichTextBox aRichTextBox)
        {
            iRichTextBox = aRichTextBox;
            Debug.Listeners.Add(this);
        }

        public new void Dispose()
        {
            //Sometimes we were getting a crash when closing our form
            //Let's see if that helps.
            Debug.Listeners.Remove(this);
            base.Dispose();            
        }

        public override void WriteLine(string aString)
        {
            //Add time stamp and new line characters
            Write(DateTime.Now.ToString("MM/dd HH:mm:ss.fff: ") + aString + "\r\n");
        }

        public override void Write(string aString)
        {
            //Allows iRichTextBox to be updated from different thread
            if (iRichTextBox.InvokeRequired)
            {
                // Fire and forget invocation
                // Using the synchronous variant Invoke tends to result in deadlock here
                iRichTextBox.BeginInvoke(new MethodInvoker(delegate ()
                {
                    iRichTextBox.Text += aString;
                }));
            }
            else
            {
                iRichTextBox.Text += aString;
            }
        }

    }
}