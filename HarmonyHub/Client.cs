using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.Sasl;
using agsXMPP.Xml.Dom;
using HarmonyHub.Internals;
using HarmonyHub.Utils;
using System.Threading;

namespace HarmonyHub
{
    /// <summary>
    ///     Client to interrogate and control Logitech Harmony Hub.
    /// </summary>
    public class Client
    {

        /// <summary>
        /// 
        /// </summary>
        private class TaskResult
        {
            public bool Success = false;
            public string ResultString = "";
            public IQ ResultIQ = null;
        }

        /// <summary>
        /// 
        /// </summary>
        private class TaskCompletionSource : TaskCompletionSource<TaskResult>
        {
            public TaskCompletionSource(TaskType aType, string aId = "") { Type = aType; Id = aId; }

            public TaskType Type;
            public string Id;
        }


        private enum TaskType
        {
            Open,
            Close,
            String,
            IQ,
            FireAndForget //Poor naming I know
        }


        private TaskCompletionSource _tcs = null;

        // The connection
        private XmppClientConnection _xmpp;

        /// <summary>
        /// This event is triggered when the current activity is changed
        /// </summary>
        public event EventHandler<string> OnActivityChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public Client(string host, int port = 5222)
        {
            Host = host;
            Port = port;
            CreateXMPP(host, port);
        }



        /// <summary>
        ///     Read the token used for the connection, maybe to store it and use it another time.
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// The host name this client is connecting to. Can also be an IP address.
        /// It's tipycally HarmonyHub.
        /// </summary>
        public readonly string Host;

        /// <summary>
        /// The port this client is connecting to. 
        /// Tipycally set to 5222.
        /// </summary>
        public readonly int Port;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        private void CreateXMPP(string host, int port)
        {
            Trace.WriteLine("XMPP: Create");

            Debug.Assert(_xmpp == null);
            _xmpp = new XmppClientConnection(host, port)
            {
                UseStartTLS = false,
                UseSSL = false,
                UseCompression = false,
                AutoResolveConnectServer = false,
                AutoAgents = false,
                AutoPresence = true,
                AutoRoster = true,
                // Keep alive is needed otherwise the server closes the connection after 60s.                
                KeepAlive = true,
                // Keep alive interval must be under 60s.
                KeepAliveInterval = 45

            };
            // Configure Sasl not to use auto and PLAIN for authentication
            _xmpp.OnSaslStart += SaslStartHandler;
            _xmpp.OnLogin += OnLoginHandler;
            _xmpp.OnIq += OnIqHandler;
            _xmpp.OnMessage += OnMessage;
            _xmpp.OnSocketError += ErrorHandler;
            _xmpp.OnClose += OnCloseHandler;
            _xmpp.OnError += ErrorHandler;
            _xmpp.OnXmppConnectionStateChanged += XmppConnectionStateHandler;
            //TODO: add handlers for all missing events and put some logs
        }



        /// <summary>
        /// Open client connection with Harmony Hub
        /// </summary>
        /// <param name="aToken">token which is created via an authentication via myharmony.com</param>
        /// <returns></returns>
        public async Task OpenAsync(string aToken)
        {
            Trace.WriteLine("Harmony: Open with token");
            //Make sure we are closed first
            if (!IsClosed)
            {
                await CloseAsync().ConfigureAwait(false);
            }

            if (!IsClosed)
            {
                Trace.WriteLine("Harmony: Abort, connection not closed");
                return;
            }

            // Create our task
            TaskCompletionSource tcs = CreateTask(TaskType.Open);

            Trace.WriteLine("Harmony: Opening connection...");
            Token = aToken;
            // Open the connection, do the login
            _xmpp.Open($"{Token}@x.com", Token);

            //Results should be comming in OnLogin
            await tcs.Task.ConfigureAwait(false);
            Trace.WriteLine("Harmony: Ready");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aType"></param>
        /// <param name="aId"></param>
        /// <returns></returns>
        TaskCompletionSource CreateTask(TaskType aType, string aId="")
        {
            Debug.Assert(_tcs==null);
            // Set as our currently running task
            _tcs = new TaskCompletionSource(aType,aId);
            return _tcs;
        }

        /// <summary>
        /// Release our current task.
        /// </summary>
        /// <returns></returns>
        TaskCompletionSource ReleaseTask()
        {
            Debug.Assert(_tcs != null);
            TaskCompletionSource tcs = _tcs;
            _tcs = null;
            return tcs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aUserName"></param>
        /// <param name="aPassword"></param>
        public async Task OpenAsync(string aUserName, string aPassword)
        {
            Trace.WriteLine("Harmony: Open with user name and password");

            //Make sure we are closed first
            //Make sure we are closed first
            if (!IsClosed)
            {
                await CloseAsync().ConfigureAwait(false);
            }

            if (!IsClosed)
            {
                Trace.WriteLine("Harmony: Abort, connection not closed");
                return;
            }

            Trace.WriteLine("Harmony: Connecting to logitech servers...");
            string userAuthToken = await Authentication.GetUserAuthToken(aUserName, aPassword).ConfigureAwait(false); ;
            if (string.IsNullOrEmpty(userAuthToken))
            {
                throw new Exception("Could not get token from Logitech server.");
            }

            // Make a guest connection only to exchange the session token via the user authentication token
            Trace.WriteLine("Harmony: Opening guest connection...");
            await OpenAsync("guest").ConfigureAwait(false); ;
            Token = await SwapAuthTokenAsync(userAuthToken).ConfigureAwait(false);
            await CloseAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(Token))
            {
                throw new Exception("Could not swap token on Harmony Hub.");
            }
            await OpenAsync(Token).ConfigureAwait(false);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aUserName"></param>
        /// <param name="aPassword"></param>
        /// <returns></returns>
        public async Task<bool> TryOpenAsync(string aUserName, string aPassword)
        {
            try
            {
                await OpenAsync(aUserName, aPassword).ConfigureAwait(false); ;
                return IsReady;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Harmony: failed to open connection");
                Trace.WriteLine("Harmony-logs: Exception: " + ex.ToString());                
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aToken"></param>
        /// <returns></returns>
        public async Task<bool> TryOpenAsync(string aToken)
        {
            try
            {
                await OpenAsync(aToken).ConfigureAwait(false); ;
                return IsReady;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Harmony: failed to open connection");
                Trace.WriteLine("Harmony-logs: Exception: " + ex.ToString());
                return false;
            }
        }



        /// <summary>
        /// Cancel the current task if any
        /// </summary>
        public void CancelCurrentTask()
        {
            if (_tcs != null)
            {
                TaskCompletionSource tcs = ReleaseTask();
                tcs.TrySetCanceled();
            }
        }

        /// <summary>
        /// Close connection with Harmony Hub
        /// </summary>
        public async Task CloseAsync()
        {
            Trace.WriteLine("Harmony: Close");

            //Cancel current task
            CancelCurrentTask();

            TaskCompletionSource tcs = CreateTask(TaskType.Close);
            _xmpp.Close();
            Trace.WriteLine("Harmony: Closing...");
            await tcs.Task.ConfigureAwait(false);
            Trace.WriteLine("Harmony: Closed");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsReady { get { return _xmpp.XmppConnectionState == XmppConnectionState.SessionStarted && _tcs == null;  } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsClosed { get { return _xmpp.XmppConnectionState == XmppConnectionState.Disconnected && _tcs == null; } }


        /// <summary>
        ///     Send a document, ignore the response (but wait shortly for a possible error)
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="waitTimeout">the time to wait for a possible error, if this is too small errors are ignored.</param>
        /// <returns>Task to await on</returns>
        private async Task<TaskResult> FireAndForgetAsync(Document document, int waitTimeout = 50)
        {
            Trace.WriteLine("Harmony-logs: FireAndForgetAsync");
            Debug.Assert(IsReady);
            // Create the IQ to send
            var iqToSend = GenerateIq(document);

            // Prepate the TaskCompletionSource, which is used to await the result
            TaskCompletionSource tcs = CreateTask(TaskType.FireAndForget, iqToSend.Id);

            Trace.WriteLine("XMPP Sending Iq:");
            Trace.WriteLine(iqToSend.ToString());
            // Start the sending
            _xmpp.Send(iqToSend);

            //Wait for completion
            return await tcs.Task.ConfigureAwait(false);

        }

        /// <summary>
        ///     Generate an IQ for the supplied Document
        /// </summary>
        /// <param name="document">Document</param>
        /// <returns>IQ</returns>
        private static IQ GenerateIq(Document document)
        {
            // Create the IQ to send
            var iqToSend = new IQ
            {
                Type = IqType.get,
                Namespace = "",
                From = "1",
                To = "guest"
            };

            // Add the real content for the Harmony
            iqToSend.AddChild(document);

            // Generate an unique ID, this is used to correlate the reply to the request
            iqToSend.GenerateId();
            return iqToSend;
        }

        /// <summary>
        ///     Get the data from the IQ response object
        /// </summary>
        /// <param name="iq">IQ response object</param>
        /// <returns>string with the data of the element</returns>
        private string GetData(IQ iq)
        {
            if (iq.HasTag("oa"))
            {
                var oaElement = iq.SelectSingleElement("oa");
                // Keep receiving messages until we get a 200 status
                // Activity commands send 100 (continue) until they finish
                var errorCode = oaElement.GetAttribute("errorcode");
                if ("200".Equals(errorCode))
                {
                    return oaElement.GetData();
                }
            }
            return null;
        }


        /// <summary>
        ///     Send a document, await the response and return it
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="timeout">Timeout for waiting on the response, if this passes a timeout exception is thrown</param>
        /// <returns>IQ response</returns>
        private async Task<TaskResult> RequestResponseAsync(Document document, int timeout = 10000)
        {
            Trace.WriteLine("Harmony-logs: RequestResponseAsync");
            Debug.Assert(IsReady);
            // Create the IQ to send
            var iqToSend = GenerateIq(document);

            // Prepate the TaskCompletionSource, which is used to await the result
            TaskCompletionSource tcs = CreateTask(TaskType.IQ, iqToSend.Id);

            Trace.WriteLine("XMPP sending Iq:");
            Trace.WriteLine(iqToSend.ToString());

            // Start the sending
            _xmpp.Send(iqToSend);

            // Await / block until an reply arrives or the timeout happens
            return await tcs.Task.ConfigureAwait(false);
        }

        #region Authentication

        /// <summary>
        ///     Send message to HarmonyHub with UserAuthToken, wait for SessionToken
        /// </summary>
        /// <param name="userAuthToken"></param>
        /// <returns></returns>
        private async Task<string> SwapAuthTokenAsync(string userAuthToken)
        {
            Trace.WriteLine("Harmony-logs: SwapAuthToken");
            var response = await RequestResponseAsync(HarmonyDocuments.LogitechPairDocument(userAuthToken)).ConfigureAwait(false);
            var sessionData = GetData(response.ResultIQ);
            if (sessionData != null)
            {
                foreach (var pair in sessionData.Split(':'))
                {
                    if (pair.StartsWith("identity"))
                    {
                        return pair.Split('=')[1];
                    }
                }
            }
            throw new Exception("Harmony: SwapAuthToken failed");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        ///     Handle incomming messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnMessage(object sender, Message message)
        {
            if (!message.HasTag("event"))
            {
                return;
            }
            // Check for the activity changed data, see here: https://github.com/swissmanu/harmonyhubjs-client/blob/master/docs/protocol/startActivityFinished.md
            var eventElement = message.SelectSingleElement("event");
            var eventData = eventElement.GetData();
            if (eventData == null)
            {
                return;
            }
            foreach (var pair in eventData.Split(':'))
            {
                if (!pair.StartsWith("activityId"))
                {
                    continue;
                }
                var activityId = pair.Split('=')[1];
                OnActivityChanged?.Invoke(this, activityId);
            }
        }

        /// <summary>
        ///     Configure Sasl not to use auto and PLAIN for authentication
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="saslEventArgs">SaslEventArgs</param>
        private void SaslStartHandler(object sender, SaslEventArgs saslEventArgs)
        {
            saslEventArgs.Auto = false;
            saslEventArgs.Mechanism = "PLAIN";
        }

        /// <summary>
        ///     Handle login by completing the _loginTaskCompletionSource
        /// </summary>
        /// <param name="sender"></param>
        private void OnLoginHandler(object sender)
        {
            Trace.WriteLine("XMPP: OnLogin - completing login task");

            if (_tcs == null)
            {
                //Task must have been cancelled
                return;
            }

            // Make sure that's the expected task
            Debug.Assert(_tcs.Type==TaskType.Open);
            // Complete our task
            ReleaseTask().TrySetResult(new TaskResult { Success = true});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void OnCloseHandler(object sender)
        {
            Trace.WriteLine("XMPP: OnClose");
            if (_tcs == null)
            {
                //Task must have been cancelled
                return;
            }

            // Make sure that's the expected task
            Debug.Assert(_tcs.Type == TaskType.Close);
            // Complete our task
            ReleaseTask().TrySetResult(new TaskResult { Success = true });
        }


        private void XmppConnectionStateHandler(object sender, XmppConnectionState state)
        {
            Trace.WriteLine("XMPP state change: " + state.ToString());
        }


        /// <summary>
        ///     Lookup the TaskCompletionSource for the IQ message and try to set the result.
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="iq">IQ</param>
        private void OnIqHandler(object sender, IQ iq)
        {
            Trace.WriteLine("XMPP OnIq: " + iq.Id);
            Trace.WriteLine(iq.ToString());

            if (_tcs == null)
            {
                //Not expecting anything
                Trace.WriteLine("XMPP: OnIq: No pending request");
                return;
            }

            if (string.IsNullOrEmpty(iq.Id))
            {
                Trace.WriteLine("XMPP: empty Iq ID.");
                if (_tcs.Type == TaskType.FireAndForget)
                {
                    Trace.WriteLine("Harmony: command acknowledged.");
                    ReleaseTask().TrySetResult(new TaskResult { Success = true });
                }                
            }
            // Check if the incoming response ID matches our request ID
            else if (iq.Id.Equals(_tcs.Id))
            {
                // Error handling from XMPP
                if (iq.Error != null)
                {
                    var errorMessage = iq.Error.ErrorText;
                    Trace.WriteLine("XMPP Iq error: " + errorMessage);
                    //TODO: Should we just provide complete without succcess?
                    ReleaseTask().TrySetException(new Exception(errorMessage));
                }
                // Message processing (error handling)
                else if (iq.HasTag("oa"))
                {
                    var oaElement = iq.SelectSingleElement("oa");

                    // Check error code
                    var errorCode = oaElement.GetAttribute("errorcode");
                    // 100 -> continue
                    if ("100".Equals(errorCode))
                    {
                        // Ignoring 100 continue
                        Trace.WriteLine("XMPP oa errorcode 100, keep going...");

                        // TODO: Insert code to handle progress updates for the startActivity
                    }
                    // 200 -> OK
                    else if ("200".Equals(errorCode))
                    {
                        Trace.WriteLine("XMPP oa errorcode 200, completing pending task.");
                        // Complete our task
                        ReleaseTask().TrySetResult(new TaskResult { Success = true, ResultIQ = iq});
                    }
                    else
                    {
                        // We didn't get a 100 or 200, this must mean there was an error
                        var errorMessage = oaElement.GetAttribute("errorstring");
                        Trace.WriteLine($"XMPP oa errorcode: {errorCode} {errorMessage}");
                        // SL: Should we really throw an exception?
                        // Set the exception on the TaskCompletionSource, it will be picked up in the await
                        ReleaseTask().TrySetException(new Exception(errorMessage));
                    }
                }
                else
                {
                    Trace.WriteLine("XMPP: oa tag not found.");
                }
            }
            else
            {
                Trace.WriteLine("XMPP: task not found.");
            }
        }

        /// <summary>
        ///     Help with login errors
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="ex">Exception</param>
        private void ErrorHandler(object sender, Exception ex)
        {
            Trace.WriteLine("XMPP: error " + ex.ToString());

            if (_tcs == null)
            {
                //Not expecting anything
                Trace.WriteLine("XMPP: error: No pending request");
                return;
            }

            //Propagate our exception then
            ReleaseTask().TrySetException(ex);
        }

        #endregion

        #region Send Messages to HarmonyHub

        /// <summary>
        ///     Request the configuration from the hub
        /// </summary>
        /// <returns>HarmonyConfig</returns>
        public async Task<Config> GetConfigAsync()
        {
            Trace.WriteLine("Harmony-logs: GetConfigAsync");
            Trace.WriteLine("Harmony: Fetching configuration...");

            if (!IsReady)
            {
                Trace.WriteLine("Harmony: Abort, connection not ready");
                return null;
            }

            var iq = await RequestResponseAsync(HarmonyDocuments.ConfigDocument()).ConfigureAwait(false);
            Trace.WriteLine("Harmony: Parsing configuration...");
            var rawConfig = GetData(iq.ResultIQ);
            if (rawConfig != null)
            {
                Config config = Serializer.FromJson<Config>(rawConfig);
                Trace.WriteLine("Harmony: Ready");
                return config;
            }
            throw new Exception("Harmony: Configuration not found");
        }

        /// <summary>
        ///     Send message to HarmonyHub to start a given activity
        ///     Result is parsed by OnIq based on ClientCommandType
        /// </summary>
        /// <param name="activityId">string</param>
        public async Task StartActivityAsync(string activityId)
        {
            Trace.WriteLine("Harmony: StartActivityAsync");

            if (!IsReady)
            {
                Trace.WriteLine("Harmony: Abort, connection not ready");
                return;
            }

            await RequestResponseAsync(HarmonyDocuments.StartActivityDocument(activityId)).ConfigureAwait(false);
        }

        /// <summary>
        ///     Send message to HarmonyHub to request current activity
        ///     Result is parsed by OnIq based on ClientCommandType
        /// </summary>
        /// <returns>string with the current activity</returns>
        public async Task<string> GetCurrentActivityAsync()
        {
            Trace.WriteLine("Harmony: GetCurrentActivityAsync");

            if (!IsReady)
            {
                Trace.WriteLine("Harmony: Abort, connection not ready");
                return "";
            }

            var iq = await RequestResponseAsync(HarmonyDocuments.GetCurrentActivityDocument()).ConfigureAwait(false);
            var currentActivityData = GetData(iq.ResultIQ);
            if (currentActivityData != null)
            {
                return currentActivityData.Split('=')[1];
            }
            throw new Exception("No data found in IQ");
        }

        /// <summary>
        ///     Send message to HarmonyHub to request to press a button
        ///     Result is parsed by OnIq based on ClientCommandType
        /// </summary>
        /// <param name="deviceId">string with the ID of the device</param>
        /// <param name="command">string with the command for the device</param>
        /// <param name="press">true for press, false for release</param>
        /// <param name="timestamp">Timestamp for the command, e.g. send a press with 0 and a release with 100</param>
        public async Task SendCommandAsync(string deviceId, string command, bool press = true, int? timestamp = null)
        {
            Trace.WriteLine("Harmony-logs: SendCommandAsync");

            if (!IsReady)
            {
                Trace.WriteLine("Harmony: Abort, connection not ready");
                return;
            }

            var document = HarmonyDocuments.IrCommandDocument(deviceId, command, press, timestamp);
            await FireAndForgetAsync(document).ConfigureAwait(false);
        }

        /// <summary>
        ///     Send a message that a button was pressed
        ///     Result is parsed by OnIq based on ClientCommandType
        /// </summary>
        /// <param name="deviceId">string with the ID of the device</param>
        /// <param name="command">string with the command for the device</param>
        /// <param name="timespan">The time between the press and release, default 100ms</param>
        public async Task SendKeyPressAsync(string deviceId, string command, int timespan = 100)
        {
            Trace.WriteLine("Harmony: SendKeyPressAsync");

            if (!IsReady)
            {
                Trace.WriteLine("Harmony: Abort, connection not ready");
                return;
            }

            var now = (int)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            var press = HarmonyDocuments.IrCommandDocument(deviceId, command, true, now -timespan);
            await FireAndForgetAsync(press).ConfigureAwait(false);
            var release = HarmonyDocuments.IrCommandDocument(deviceId, command, false, timespan);
            await FireAndForgetAsync(release).ConfigureAwait(false);
        }

        /// <summary>
        ///     Send message to HarmonyHub to request to turn off all devices
        /// </summary>
        public async Task TurnOffAsync()
        {
            Trace.WriteLine("Harmony: TurnOffAsync");

            if (!IsReady)
            {
                Trace.WriteLine("Harmony: Abort, connection not ready");
                return;
            }

            var currentActivity = await GetCurrentActivityAsync().ConfigureAwait(false);
            if (currentActivity != "-1")
            {
                await StartActivityAsync("-1").ConfigureAwait(false);
            }
        }

        #endregion
    }
}