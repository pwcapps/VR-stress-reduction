using System;
using System.Threading;
using WebSocket4Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CortexAccess
{
    public class StreamDataEventArgs
    {
        public StreamDataEventArgs(int  streamType, JObject data, int requestType = 0)
        {
            StreamType = streamType;
            RequestType = requestType;
            Data = data;
        }
        public int RequestType { get; private set; }
        public int StreamType { get; private set; }
        public JObject Data { get; private set; }
    }
    public class MessageErrorEventArgs
    {
        public MessageErrorEventArgs(int code, string messageError)
        {
            Code = code;
            MessageError = messageError;
        }
        public int Code { get; set; }
        public string MessageError { get; set; }
    }

    // CortexClient class work with Cortex Service directly
    public sealed class CortexClient
    {
        const string Url = "wss://emotivcortex.com:54321";

        // Member variables
        private static readonly CortexClient instance = new CortexClient();

        private WebSocket _wSC; // Websocket Client
        private int _nextRequestId; // Unique id for each request
        private bool _isWSConnected;

        private string m_CurrentMessage = string.Empty;
        //Events
        private AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        private AutoResetEvent m_OpenedEvent = new AutoResetEvent(false);
        private AutoResetEvent m_CloseEvent = new AutoResetEvent(false);


        public event EventHandler<bool> OnConnected;
        public event EventHandler<MessageErrorEventArgs> OnMessageError;
        public event EventHandler<StreamDataEventArgs> OnStreamDataReceived;
        public event EventHandler<StreamDataEventArgs> OnEventReceived;

        // Constructor
        static CortexClient()
        {

        }
        private CortexClient()
        {
            _nextRequestId = 1;

            _wSC = new WebSocket(Url);
            _wSC.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(WebSocketClient_Error);
            _wSC.Opened += new EventHandler(WebSocketClient_Opened);
            _wSC.Closed += new EventHandler(WebSocketClient_Closed);

            _wSC.MessageReceived += new EventHandler<MessageReceivedEventArgs>(WebSocketClient_MessageReceived);
        }
        // Properties
        public static CortexClient Instance
        {
            get
            {
                return instance;
            }
        }

        public bool IsWSConnected
        {
            get
            {
                return _isWSConnected;
            }
        }

        // Method
        // Send request to Cortex Service
        public int GenerateRequestedID(int streamType, int requestType)
        {
            //int epocTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            string requestID = streamType.ToString() + requestType.ToString() + _nextRequestId.ToString();
            ++_nextRequestId;
            if( _nextRequestId >=100000)
            {
                _nextRequestId = 1; // reset _nextRequestID to avoid overrange Int32 for requestID
            }
            return Int32.Parse(requestID);
        }
        public int SendTextMessage(JObject param, int streamType, string method, bool hasParam = true, int requestType = 0)
        {
            if(!IsWSConnected)
            {
                return -1;
            }
            int requestID = GenerateRequestedID(streamType, requestType);
            JObject request = new JObject(
            new JProperty("jsonrpc", "2.0"),
            new JProperty("id", requestID),
            new JProperty("method", method));

            if (hasParam)
            {
                request.Add("params", param);
            }
            //Console.WriteLine("Send " + method);
            //Console.WriteLine(request.ToString());

            // send the json message
            _wSC.Send(request.ToString());
            return requestID;
        }
        //Open socket
        public void Open()
        {
            //Open websocket
            _wSC.Open();

            if (!m_OpenedEvent.WaitOne(10000))
            {
                Console.WriteLine("Failed to Opened session ontime");
                //Assert.Fail("Failed to Opened session ontime");
            }
            if (_wSC.State == WebSocketState.Open)
            {
                _isWSConnected = true;
                OnConnected(this, true);
            }
            else
            {
                _isWSConnected = false;
                OnConnected(this, false);
            }

        }
        //Close Socket
        public void Close()
        {
            _wSC.Close();

            if (!m_CloseEvent.WaitOne(10000))
            {
                Assert.Fail("Failed to close session ontime");
            }
            Assert.AreEqual(WebSocketState.Closed, _wSC.State);
            _nextRequestId = 1;
        }

        // Handle receieved message 
        private void WebSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            m_CurrentMessage = e.Message;
            m_MessageReceiveEvent.Set();
            //Console.WriteLine("Received: " + e.Message);

            JObject response = JObject.Parse(e.Message);

            if(response["warning"] != null)
            {
                JObject warning = (JObject)response["warning"];
                string messageWarning = "";
                int code = -1;
                if (warning["message"].Type == JTokenType.String)
                {
                    messageWarning = warning["message"].ToString();
                }else if(warning["message"].Type == JTokenType.Object)
                {
                    Console.WriteLine("Received Warning Object");
                }
                if(warning["code"] != null)
                    code = (int)warning["code"];

                Console.WriteLine("Received: " + messageWarning);

                OnMessageError(this, new MessageErrorEventArgs(code, messageWarning));
            }
            if(response["sid"] != null)
            {
                string sid = (string)response["sid"];
                if (response["mot"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.MOTION_STREAM, response));
                }
                else if (response["eeg"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.EEG_STREAM, response));
                }
                else if(response["dev"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.DEVICE_STREAM, response));
                }
                else if (response["met"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.PERF_METRICS_STREAM, response));
                }
                else if (response["com"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.MENTAL_CMD_DATA_STREAM, response));
                }
                else if (response["fac"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.FACIAL_EXP_DATA_STREAM, response));
                }
                else if (response["sys"] != null)
                {
                    OnStreamDataReceived(this, new StreamDataEventArgs((int)StreamID.SYS_STREAM, response));
                }
                else
                {
                    Console.WriteLine("Can not detect stream type");
                }
               
            }
            else if(response["error"] != null)
            {
                JObject error = (JObject)response["error"];
                int code = (int)error["code"];
                string messageError = (string)error["message"];
                Console.WriteLine("Received: " + messageError);
                //Send Error message event
                OnMessageError(this, new MessageErrorEventArgs(code, messageError));
            }
            else if (response["id"] != null)
            {
                int id = (int)response["id"];

                // Get stream type from ID which requestID= streamType.ToString() + requestType.ToString() + epocTime.ToString();
                int streamType = Int32.Parse(id.ToString().Substring(0, 2));
                int requestType = Int32.Parse(id.ToString().Substring(2, 2));
                OnEventReceived(this, new StreamDataEventArgs(streamType, response, requestType));
            }
        }

        private void WebSocketClient_Closed(object sender, EventArgs e)
        {
            m_CloseEvent.Set();
        }

        private void WebSocketClient_Opened(object sender, EventArgs e)
        {
            m_OpenedEvent.Set();
        }

        private void WebSocketClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.GetType() + ":" + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);

            if (e.Exception.InnerException != null)
            {
                Console.WriteLine(e.Exception.InnerException.GetType());
            }
        }
       
    }
}
