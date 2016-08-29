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
        /// Define the output of any task.
        /// </summary>
        protected class TaskResult
        {
            /// <summary>
            /// Tells whether our task was completed succesfully 
            /// </summary>
            public bool Success = false;
            /// <summary>
            /// Used when our task output is expected to be a string
            /// </summary>
            public string ResultString = "";
            /// <summary>
            /// Used when our task output is expected to be an XMPP IQ.
            /// </summary>
            public IQ ResultIQ = null;
        }

        /// <summary>
        /// Task Completion source object used to provide async APIs to an Harmony Hub.
        /// </summary>
        protected class TaskCompletionSource : TaskCompletionSource<TaskResult>
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="aType"></param>
            /// <param name="aId"></param>
            public TaskCompletionSource(TaskType aType, string aId = "") { Type = aType; Id = aId; }

            /// <summary>
            /// Specifies the type of our task.
            /// </summary>
            public TaskType Type;

            /// <summary>
            /// Allow us to corelate the server response with this request.
            /// </summary>
            public string Id;
        }

        /// <summary>
        /// Defines various type of request we can send to our Harmony Hub server.
        /// </summary>
        protected enum TaskType
        {
            /// <summary>
            /// Opening our connection
            /// </summary>
            Open,
            /// <summary>
            /// Closing our connection
            /// </summary>
            Close,
            /// <summary>
            /// A non specified IQ request
            /// </summary>
            IQ,
            /// <summary>
            /// A send command request
            /// </summary>
            SendCommmand
        }

        private TaskCompletionSource _tcs;

        /// <summary>
        /// Provide access to the currently running task.
        /// </summary>
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
