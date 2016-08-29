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
    /// Implement some basics of our Harmony Hub client
    /// </summary>
    public class ClientBase : ClientRaw
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aType"></param>
        /// <param name="aId"></param>
        /// <returns></returns>
        protected TaskCompletionSource CreateTask(TaskType aType, string aId = "")
        {
            Debug.Assert(!RequestPending);
            // Set as our currently running task
            Tcs = new TaskCompletionSource(aType, aId);
            return Tcs;
        }

        /// <summary>
        /// Release our current task.
        /// </summary>
        /// <returns></returns>
        protected TaskCompletionSource ReleaseTask()
        {
            Debug.Assert(RequestPending);
            TaskCompletionSource tcs = Tcs;
            Tcs = null;
            return tcs;
        }

        /// <summary>
        /// Cancel the current task if any
        /// </summary>
        public void CancelCurrentTask()
        {
            if (RequestPending)
            {
                TaskCompletionSource tcs = ReleaseTask();
                tcs.TrySetCanceled();
            }
        }
    }
}
