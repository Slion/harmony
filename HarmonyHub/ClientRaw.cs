using agsXMPP.protocol.client;
using System;
using System.Collections.Generic;
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

        protected TaskCompletionSource Tcs { get { return _tcs; } set { _tcs = value; OnTaskChanged?.Invoke(this, _tcs != null); } }

        /// <summary>
        /// Triggered whenever our task is changing.
        /// </summary>
        public event EventHandler<bool> OnTaskChanged;

        /// <summary>
        /// Tells whether our Harmony Hub client has a pending request currently awaiting response from the server.
        /// </summary>
        /// <returns></returns>
        public bool RequestPending { get { return _tcs != null; } }
    }
}
