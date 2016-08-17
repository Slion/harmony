﻿using System;
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
    public class Client : IDisposable
    {
        /// <summary>
        ///     This has the login state..
        ///     When the OnLoginHandler is triggered this is set with true,
        ///     When an error occurs before this, the expeception is set.
        ///     Everywhere where this is awaited the state is returned, but blocks until there is something.
        /// </summary>
        private readonly TaskCompletionSource<bool> _loginTaskCompletionSource = new TaskCompletionSource<bool>();

        // A lookup to correlate request and responses
        private readonly IDictionary<string, TaskCompletionSource<IQ>> _resultTaskCompletionSources = new ConcurrentDictionary<string, TaskCompletionSource<IQ>>();
        // The connection
        private readonly XmppClientConnection _xmpp;

        /// <summary>
        /// This event is triggered when the current activity is changed
        /// </summary>
        public event EventHandler<string> OnActivityChanged;

        /// <summary>
        ///     Constructor with standard settings for a new Client
        /// </summary>
        /// <param name="host">IP or hostname</param>
        /// <param name="token">Auth-token, or guest</param>
        /// <param name="port">The port to connect to, default 5222</param>
        public Client(string host, int port = 5222)
        {
            _xmpp = new XmppClientConnection(host, port)
            {
                UseStartTLS = false,
                UseSSL = false,
                UseCompression = false,
                AutoResolveConnectServer = false,
                AutoAgents = false,
                AutoPresence = true,
                AutoRoster = true
            };
            // Configure Sasl not to use auto and PLAIN for authentication
            _xmpp.OnSaslStart += SaslStartHandler;
            _xmpp.OnLogin += OnLoginHandler;
            _xmpp.OnIq += OnIqResponseHandler;
            _xmpp.OnMessage += OnMessage;
            _xmpp.OnSocketError += ErrorHandler;
            _xmpp.OnClose += OnCloseHandler;
            _xmpp.OnError += ErrorHandler;
            //TODO: add handlers for all missing events and put some logs
        }

        /// <summary>
        ///     Read the token used for the connection, maybe to store it and use it another time.
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        ///     Cleanup and close
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Open client connection with Harmony Hub
        /// </summary>
        /// <param name="aToken">token which is created via an authentication via myharmony.com</param>
        /// <returns></returns>
        public void Open(string aToken)
        {
            Token = aToken;

            // Open the connection, do the login
            _xmpp.Open($"{Token}@x.com", Token);

            //Results should be comming in OnLogin
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aUserName"></param>
        /// <param name="aPassword"></param>
        public async Task Open(string aUserName, string aPassword)
        {
            string userAuthToken = await Authentication.GetUserAuthToken(aUserName, aPassword);
            if (string.IsNullOrEmpty(userAuthToken))
            {
                throw new Exception("Could not get token from Logitech server.");
            }

            // Make a guest connection only to exchange the session token via the user authentication token
            Open("guest");
            Token = await SwapAuthToken(userAuthToken).ConfigureAwait(false);
            Close();
            if (string.IsNullOrEmpty(Token))
            {
                throw new Exception("Could not swap token on Harmony Hub.");
            }

            Open(Token);
        }


        /// <summary>
        /// Close connection with Harmony Hub
        /// </summary>
        public void Close()
        {
            _xmpp.Close();

            _xmpp.OnIq -= OnIqResponseHandler;
            _xmpp.OnMessage -= OnMessage;
            _xmpp.OnLogin -= OnLoginHandler;
            _xmpp.OnSocketError -= ErrorHandler;
            _xmpp.OnSaslStart -= SaslStartHandler;
            _xmpp.OnClose -= OnCloseHandler;
            _xmpp.OnError -= ErrorHandler;
            
        }


        /// <summary>
        ///     Send a document, ignore the response (but wait shortly for a possible error)
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="waitTimeout">the time to wait for a possible error, if this is too small errors are ignored.</param>
        /// <returns>Task to await on</returns>
        private async Task FireAndForgetAsync(Document document, int waitTimeout = 50)
        {
            // Check if the login was made, this blocks until there is a state
            // And throws an exception if the login failed.
            await _loginTaskCompletionSource.Task.ConfigureAwait(false);

            // Create the IQ to send
            var iqToSend = GenerateIq(document);

            // Prepate the TaskCompletionSource, which is used to await the result
            var resultTaskCompletionSource = new TaskCompletionSource<IQ>();
            _resultTaskCompletionSources[iqToSend.Id] = resultTaskCompletionSource;

            Debug.WriteLine("Sending (ignoring response):");
            Debug.WriteLine(iqToSend.ToString());
            // Start the sending
            _xmpp.Send(iqToSend);

            // Await, to make sure there wasn't an error
            var task = await Task.WhenAny(resultTaskCompletionSource.Task, Task.Delay(waitTimeout)).ConfigureAwait(false);

            // Remove the result task, as we no longer need it.
            _resultTaskCompletionSources.Remove(iqToSend.Id);

            // This makes sure the exception, if there was one, is unwrapped
            await task;

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
        private async Task<IQ> RequestResponseAsync(Document document, int timeout = 10000)
        {
            // Check if the login was made, this blocks until there is a state
            // And throws an exception if the login failed.
            await _loginTaskCompletionSource.Task.ConfigureAwait(false);

            // Create the IQ to send
            var iqToSend = GenerateIq(document);

            // Prepate the TaskCompletionSource, which is used to await the result
            var resultTaskCompletionSource = new TaskCompletionSource<IQ>();
            _resultTaskCompletionSources[iqToSend.Id] = resultTaskCompletionSource;

            Debug.WriteLine("Sending:");
            Debug.WriteLine(iqToSend.ToString());

            // Create the action which is called when a timeout occurs
            System.Action timeoutAction = () =>
            {
                // Remove the registration, it is no longer needed
                _resultTaskCompletionSources.Remove(iqToSend.Id);
                // Pass the timeout exception to the await
                resultTaskCompletionSource.TrySetException(new TimeoutException($"Timeout while waiting on response {iqToSend.Id} after {timeout}"));

            };
            // Start the sending
            _xmpp.Send(iqToSend);

            // Setup the timeout handling
            var cancellationTokenSource = new CancellationTokenSource(timeout);
            using (cancellationTokenSource.Token.Register(timeoutAction))
            {
                // Await / block until an reply arrives or the timeout happens
                return await resultTaskCompletionSource.Task.ConfigureAwait(false);
            }
        }

        #region Authentication

        /// <summary>
        ///     Send message to HarmonyHub with UserAuthToken, wait for SessionToken
        /// </summary>
        /// <param name="userAuthToken"></param>
        /// <returns></returns>
        public async Task<string> SwapAuthToken(string userAuthToken)
        {
            var iq = await RequestResponseAsync(HarmonyDocuments.LogitechPairDocument(userAuthToken)).ConfigureAwait(false);
            var sessionData = GetData(iq);
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
            throw new Exception("Wrong data");
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
            _loginTaskCompletionSource.TrySetResult(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void OnCloseHandler(object sender)
        {
            Debug.WriteLine("XMPP: OnClose");
        }


        /// <summary>
        ///     Lookup the TaskCompletionSource for the IQ message and try to set the result.
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="iq">IQ</param>
        private void OnIqResponseHandler(object sender, IQ iq)
        {
            Debug.WriteLine("Received event " + iq.Id);
            Debug.WriteLine(iq.ToString());
            TaskCompletionSource<IQ> resulTaskCompletionSource;
            if (iq.Id != null && _resultTaskCompletionSources.TryGetValue(iq.Id, out resulTaskCompletionSource))
            {
                // Error handling from XMPP
                if (iq.Error != null)
                {
                    var errorMessage = iq.Error.ErrorText;
                    Debug.WriteLine(errorMessage);
                    resulTaskCompletionSource.TrySetException(new Exception(errorMessage));
                    // Result task is longer needed in the lookup
                    _resultTaskCompletionSources.Remove(iq.Id);
                    return;
                }

                // Message processing (error handling)
                if (iq.HasTag("oa"))
                {
                    var oaElement = iq.SelectSingleElement("oa");

                    // Check error code
                    var errorCode = oaElement.GetAttribute("errorcode");
                    // 100 -> continue
                    if ("100".Equals(errorCode))
                    {
                        // Ignoring 100 continue
                        Debug.WriteLine("Ignoring, expecting more to come.");

                        // TODO: Insert code to handle progress updates for the startActivity
                    }
                    // 200 -> OK
                    else if ("200".Equals(errorCode))
                    {
                        resulTaskCompletionSource.TrySetResult(iq);

                        // Result task is longer needed in the lookup
                        _resultTaskCompletionSources.Remove(iq.Id);
                    }
                    else
                    {
                        // We didn't get a 100 or 200, this must mean there was an error
                        var errorMessage = oaElement.GetAttribute("errorstring");
                        Debug.WriteLine(errorMessage);
                        // Set the exception on the TaskCompletionSource, it will be picked up in the await
                        resulTaskCompletionSource.TrySetException(new Exception(errorMessage));

                        // Result task is longer needed in the lookup
                        _resultTaskCompletionSources.Remove(iq.Id);
                    }
                }
                else
                {
                    Debug.WriteLine("Unexpected content");
                }
            }
            else
            {
                Debug.WriteLine("No matching result task found.");
            }
        }

        /// <summary>
        ///     Help with login errors
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="ex">Exception</param>
        private void ErrorHandler(object sender, Exception ex)
        {
            if (_loginTaskCompletionSource.Task.Status == TaskStatus.Created)
            {
                _loginTaskCompletionSource.TrySetException(ex);
            }
            else
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region Send Messages to HarmonyHub

        /// <summary>
        ///     Request the configuration from the hub
        /// </summary>
        /// <returns>HarmonyConfig</returns>
        public async Task<Config> GetConfigAsync()
        {
            var iq = await RequestResponseAsync(HarmonyDocuments.ConfigDocument()).ConfigureAwait(false);
            var config = GetData(iq);
            if (config != null)
            {
                return Serializer.FromJson<Config>(config);
            }
            throw new Exception("No data found");
        }

        /// <summary>
        ///     Send message to HarmonyHub to start a given activity
        ///     Result is parsed by OnIq based on ClientCommandType
        /// </summary>
        /// <param name="activityId">string</param>
        public async Task StartActivityAsync(string activityId)
        {
            await RequestResponseAsync(HarmonyDocuments.StartActivityDocument(activityId)).ConfigureAwait(false);
        }

        /// <summary>
        ///     Send message to HarmonyHub to request current activity
        ///     Result is parsed by OnIq based on ClientCommandType
        /// </summary>
        /// <returns>string with the current activity</returns>
        public async Task<string> GetCurrentActivityAsync()
        {
            var iq = await RequestResponseAsync(HarmonyDocuments.GetCurrentActivityDocument()).ConfigureAwait(false);
            var currentActivityData = GetData(iq);
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
            var currentActivity = await GetCurrentActivityAsync().ConfigureAwait(false);
            if (currentActivity != "-1")
            {
                await StartActivityAsync("-1").ConfigureAwait(false);
            }
        }

        #endregion
    }
}