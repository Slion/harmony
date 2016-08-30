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
    ///     Logitech Harmony Hub client controller.
    /// </summary>
    public class Client: ClientBase
    {
        #region Public Events
        /// <summary>
        /// This event is triggered when the current activity is changed
        /// </summary>
        public event EventHandler<string> OnActivityChanged;
        #endregion

        #region Public Properties
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
        /// Tells whether our Harmony Hub client is ready to issue new requests.
        /// </summary>
        /// <returns></returns>
        public bool IsReady { get { return _xmpp.XmppConnectionState == XmppConnectionState.SessionStarted && !RequestPending; } }

        /// <summary>
        /// Tells whether our Harmony Hub client has been disconnected.
        /// </summary>
        /// <returns></returns>
        public bool IsClosed { get { return _xmpp.XmppConnectionState == XmppConnectionState.Disconnected && !RequestPending; } }

        /// <summary>
        /// Tells whether our Harmony Hub client has an open connection.
        /// </summary>
        /// <returns></returns>
        public bool IsOpen { get { return _xmpp.XmppConnectionState == XmppConnectionState.SessionStarted; } }
        #endregion

        #region Private Properties
        /// <summary>
        /// Client connection to our XMPP server: the Harmony Hub.
        /// </summary>
        private XmppClientConnection _xmpp;
        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aHost"></param>
        /// <param name="aKeepAlive"></param>
        /// <param name="aKeepAliveInterval"></param>
        /// <param name="aPort"></param>
        public Client(string aHost, bool aKeepAlive = true, int aKeepAliveInterval = 40, int aPort = 5222)
        {
            Host = aHost;
            Port = aPort;
            CreateXMPP(aHost, aKeepAlive, aKeepAliveInterval, aPort);
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
        /// Non leaving variant of the above.
        /// </summary>
        /// <param name="aToken"></param>
        /// <returns></returns>
        public async Task<bool> TryOpenAsync(string aToken)
        {
            try
            {
                await OpenAsync(aToken).ConfigureAwait(false);
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
        /// Open client connection to our Harmony Hub through Logitech servers using some Logitech account credentials.
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
        /// Non leaving variant of the above.
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
        /// Close connection with Harmony Hub
        /// </summary>
        public async Task CloseAsync()
        {
            Trace.WriteLine("Harmony: Close");

            if (IsClosed)
            {
                //All is well then
                return;
                // In fact trying to close a connection that's already been closed would result in the close task not completing since we don't have any timeout for now.
                // That because the OnClose handler is not called if the connection is already dead.
            }

            //Cancel current task
            CancelCurrentTask();

            TaskCompletionSource tcs = CreateTask(TaskType.Close);
            _xmpp.Close();
            Trace.WriteLine("Harmony: Closing...");
            await tcs.Task.ConfigureAwait(false);
            Trace.WriteLine("Harmony: Closed");
        }

        /// <summary>
        ///     Request the configuration from the hub
        /// </summary>
        /// <returns>HarmonyConfig</returns>
        public async Task<Config> GetConfigAsync()
        {
            Trace.WriteLine("Harmony-logs: GetConfigAsync");
            Trace.WriteLine("Harmony: Fetching configuration...");

            var iq = await SendDocumentAsync(HarmonyDocuments.ConfigDocument()).ConfigureAwait(false);
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
            await SendDocumentAsync(HarmonyDocuments.StartActivityDocument(activityId)).ConfigureAwait(false);
        }

        /// <summary>
        ///     Send message to HarmonyHub to request current activity
        ///     Result is parsed by OnIq based on ClientCommandType
        /// </summary>
        /// <returns>string with the current activity</returns>
        public async Task<string> GetCurrentActivityAsync()
        {
            Trace.WriteLine("Harmony: GetCurrentActivityAsync");
            var iq = await SendDocumentAsync(HarmonyDocuments.GetCurrentActivityDocument()).ConfigureAwait(false);
            var currentActivityData = GetData(iq.ResultIQ);
            if (currentActivityData != null)
            {
                return currentActivityData.Split('=')[1];
            }
            throw new Exception("No data found in IQ");
        }

        /// <summary>
        ///     Send a command to the given device through your Harmony Hub.
        ///     The returned task will complete once we receive acknowledgment from the Hub. 
        /// </summary>
        /// <param name="deviceId">string with the ID of the device</param>
        /// <param name="command">string with the command for the device</param>
        /// <param name="press">true for press, false for release</param>
        /// <param name="timestamp">Timestamp for the command, e.g. send a press with 0 and a release with 100</param>
        public async Task SendCommandAsync(string deviceId, string command, bool press = true, int? timestamp = null)
        {
            Trace.WriteLine("Harmony-logs: SendCommandAsync");
            var document = HarmonyDocuments.IrCommandDocument(deviceId, command, press, timestamp);
            await SendDocumentAsync(document, TaskType.SendCommmand).ConfigureAwait(false);
        }

        /// <summary>
        /// Non leaving variant of our 'send command'.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="command"></param>
        /// <param name="press"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public async Task<bool> TrySendCommandAsync(string deviceId, string command, bool press = true, int? timestamp = null)
        {
            try
            {
                await SendCommandAsync(deviceId, command, press, timestamp).ConfigureAwait(false);
                return IsReady;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Harmony: failed to send command");
                Trace.WriteLine("Harmony-logs: Exception: " + ex.ToString());
                return false;
            }
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
            var now = (int)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            var press = HarmonyDocuments.IrCommandDocument(deviceId, command, true, now - timespan);
            await SendDocumentAsync(press, TaskType.SendCommmand).ConfigureAwait(false);
            var release = HarmonyDocuments.IrCommandDocument(deviceId, command, false, now);
            await SendDocumentAsync(release, TaskType.SendCommmand).ConfigureAwait(false);
        }

        /// <summary>
        /// Non leaving variant of our 'send key press'.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="command"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        public async Task<bool> TrySendKeyPressAsync(string deviceId, string command, int timespan = 100)
        {
            try
            {
                await SendKeyPressAsync(deviceId, command, timespan).ConfigureAwait(false);
                return IsReady;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Harmony: failed to send key press");
                Trace.WriteLine("Harmony-logs: Exception: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        ///     Send message to HarmonyHub to request to turn off all devices
        /// </summary>
        public async Task TurnOffAsync()
        {
            Trace.WriteLine("Harmony: TurnOffAsync");
            var currentActivity = await GetCurrentActivityAsync().ConfigureAwait(false);
            if (currentActivity != "-1")
            {
                await StartActivityAsync("-1").ConfigureAwait(false);
            }
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
            Trace.WriteLine("XMPP: OnMessage: " + message.ToString());

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

            if (!RequestPending)
            {
                //Task must have been cancelled
                return;
            }

            // Make sure that's the expected task
            Debug.Assert(Tcs.Type==TaskType.Open);
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
            if (!RequestPending)
            {
                //The server is closing our connection or our task has been cancelled somehow
                Trace.WriteLine("Harmony-logs: server closed our connection");
                TriggerOnConnectionClosedByServer(false);
                return;
            }

            // Make sure that's the expected task
            if (Tcs.Type == TaskType.Close)
            {
                // Complete our Close task with success
                Trace.WriteLine("Harmony-logs: close request completed");
                ReleaseTask().TrySetResult(new TaskResult { Success = true });
            }
            else
            {
                // Looks like the server closed our connection while we were awaiting a response
                // Cancel our outstanding request then
                Trace.WriteLine("Harmony-logs: server closed our connection, abort pending request");
                ReleaseTask().TrySetCanceled();
                TriggerOnConnectionClosedByServer(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="state"></param>
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

            if (!RequestPending)
            {
                //Not expecting anything
                Trace.WriteLine("XMPP: OnIq: No pending request");
                return;
            }

            if (string.IsNullOrEmpty(iq.Id))
            {
                Trace.WriteLine("XMPP: empty Iq ID.");
                if (Tcs.Type == TaskType.SendCommmand)
                {
                    Trace.WriteLine("Harmony: command acknowledged.");
                    ReleaseTask().TrySetResult(new TaskResult { Success = true });
                }                
            }
            // Check if the incoming response ID matches our request ID
            else if (iq.Id.Equals(Tcs.Id))
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

            if (!RequestPending)
            {
                //Not expecting anything
                Trace.WriteLine("XMPP: error: No pending request");
                return;
            }

            //Propagate our exception then
            ReleaseTask().TrySetException(ex);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Send a document, ignore the response (but wait shortly for a possible error)
        /// </summary>
        /// <param name="aDocument">Document</param>
        /// <param name="aTaskType">The type of task this document is associated with</param>
        /// <returns>Task to await on</returns>
        private async Task<TaskResult> SendDocumentAsync(Document aDocument, TaskType aTaskType = TaskType.IQ)
        {
            Trace.WriteLine($"Harmony-logs: SendDocumentAsync: {aTaskType}");
            //Throw an exception if we are not ready
            CheckReadiness();
            // Create the IQ to send
            var iqToSend = GenerateIq(aDocument);

            // Prepate the TaskCompletionSource, which is used to await the result
            TaskCompletionSource tcs = CreateTask(aTaskType, iqToSend.Id);

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
        ///     Send message to HarmonyHub with UserAuthToken, wait for SessionToken
        /// </summary>
        /// <param name="userAuthToken"></param>
        /// <returns></returns>
        private async Task<string> SwapAuthTokenAsync(string userAuthToken)
        {
            Trace.WriteLine("Harmony-logs: SwapAuthToken");
            var response = await SendDocumentAsync(HarmonyDocuments.LogitechPairDocument(userAuthToken)).ConfigureAwait(false);
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

        /// <summary>
        /// Create our XMPP client connection.
        /// </summary>
        /// <param name="aHost"></param>
        /// <param name="aKeepAlive"></param>
        /// <param name="aKeepAliveInterval"></param>
        /// <param name="aPort"></param>
        private void CreateXMPP(string aHost, bool aKeepAlive, int aKeepAliveInterval, int aPort)
        {
            Trace.WriteLine("XMPP: Create");

            Debug.Assert(_xmpp == null);
            _xmpp = new XmppClientConnection(aHost, aPort)
            {
                UseStartTLS = false,
                UseSSL = false,
                UseCompression = false,
                AutoResolveConnectServer = false,
                AutoAgents = false,
                AutoPresence = true,
                AutoRoster = true,
                // Keep alive is needed otherwise the server closes the connection after 60s.                
                KeepAlive = aKeepAlive,
                // Keep alive interval should be under 60s.
                KeepAliveInterval = aKeepAliveInterval

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
        /// 
        /// </summary>
        private void CheckReadiness()
        {
            if (!IsReady)
            {
                Trace.WriteLine("Harmony: Exception: Not ready");
                InvalidOperationException ex = new InvalidOperationException("Harmony client not ready");
                ex.Source = "HarmonyHub";
                throw ex;
            }
        }

        #endregion

    }
}