using agsXMPP.protocol.client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyHub
{
    /// <summary>
    /// Protect direct access to some properties.
    /// </summary>
    public class ClientRaw
    {

        /// <summary>
        /// 
        /// </summary>
        protected class TaskResult
        {
            public bool Success = false;
            public string ResultString = "";
            public IQ ResultIQ = null;
        }

        /// <summary>
        /// 
        /// </summary>
        protected class TaskCompletionSource : TaskCompletionSource<TaskResult>
        {
            public TaskCompletionSource(TaskType aType, string aId = "") { Type = aType; Id = aId; }

            public TaskType Type;
            public string Id;
        }


        protected enum TaskType
        {
            Open,
            Close,
            String,
            IQ, // Generic, remove it
            SendCommmand
        }

        private TaskCompletionSource _tcs;

        protected TaskCompletionSource Tcs { get { return _tcs; } set { _tcs = value; TriggerOnTaskChanged(); } }

        /// <summary>
        /// Triggered whenever our task is changing.
        /// </summary>
        public event EventHandler<bool> OnTaskChanged;

        /// <summary>
        /// Triggered whenever the server is closing our connection.
        /// That's notably useful for clients wanting to re-connect.
        /// </summary>
        public event EventHandler<bool> OnConnectionClosedByServer;

        /// <summary>
        /// 
        /// </summary>
        private void TriggerOnTaskChanged()
        {
            Trace.WriteLine(RequestPending ? "Harmony-logs: Request pending" : "Harmony-logs: Request completed");
            OnTaskChanged?.Invoke(this, RequestPending);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aTaskWasCancelled"></param>
        protected void TriggerOnConnectionClosedByServer(bool aTaskWasCancelled)
        {
            OnConnectionClosedByServer?.Invoke(this, aTaskWasCancelled);
        }

        /// <summary>
        /// Tells whether our Harmony Hub client has a pending request currently awaiting response from the server.
        /// </summary>
        /// <returns></returns>
        public bool RequestPending { get { return _tcs != null; } }
    }
}
