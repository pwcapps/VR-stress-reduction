using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace CortexAccess
{
    public class SessionController : BaseController
    {
        private static readonly SessionController _instance = new SessionController();

        public enum SessionReqType : int
        {
            CREATE_SESSION = 10,
            UPDATE_SESSION = 11,
            QUERRY_SESSION = 12,
            CLOSE_SESSION = 13,
            START_RECORD = 20,
            STOP_RECORD = 21,
            INJECT_MARKER = 22,
            UPDATE_NOTE = 23,
            SUBCRIBE_DATA = 30,
            UNSUBCRIBE_DATA = 31
        }

        // Member variables
        private List<Session> _sessionLists;

        private string _sessionID;
        //private Record _currentRecord;
        private string  _nextStatus;
        private bool _isCreateSession;
        private bool _isRecording;
        private string _recordID;

        private string _recordingName;
        private string _recordingNote;
        private string _recordingSubject;

        // Event
        //public event EventHandler<ArrayList> OnSubcribeOK;
        public event EventHandler<ArrayList> OnSubcribeEEGOK;
        public event EventHandler<ArrayList> OnSubcribeMotionOK;
        public event EventHandler<ArrayList> OnSubcribeDevOK;
        public event EventHandler<ArrayList> OnSubcribeMetOK;

        // Constructor
        // Constructor
        static SessionController()
        {

        }
        private SessionController()
        {
            NextStatus = "open";
            _isCreateSession = false;
        }

        // Properties
        public static SessionController Instance
        {
            get
            {
                return _instance;
            }
        }

        public string NextStatus
        {
            get
            {
                return _nextStatus;
            }

            set
            {
                _nextStatus = value;
            }
        }

        public bool IsCreateSession
        {
            get
            {
                return _isCreateSession;
            }
        }

        public string SessionID
        {
            get
            {
                return _sessionID;
            }

            set
            {
                _sessionID = value;
            }
        }

        public bool IsRecording
        {
            get
            {
                return _isRecording;
            }
        }

        public string RecordingName
        {
            get
            {
                return _recordingName;
            }

            set
            {
                _recordingName = value;
            }
        }

        public string RecordingNote
        {
            get
            {
                return _recordingNote;
            }

            set
            {
                _recordingNote = value;
            }
        }

        public string RecordingSubject
        {
            get
            {
                return _recordingSubject;
            }

            set
            {
                _recordingSubject = value;
            }
        }

        public List<Session> SessionLists
        {
            get
            {
                return _sessionLists;
            }

            set
            {
                _sessionLists = value;
            }
        }

        public string RecordID
        {
            get
            {
                return _recordID;
            }

            set
            {
                _recordID = value;
            }
        }

        // Method
        // Request 
        // Create Session
        public void CreateSession(string headsetID, string token, int experimentID = 0)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("headset", headsetID),
                    new JProperty("status", NextStatus));

            if (experimentID > 0)
            {
                param.Add("experimentID", experimentID);
            }

            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "createSession", true, (int)SessionReqType.CREATE_SESSION);
        }

        // Update Session
        // start Record
        // Stop Record
        public void UpdateSession(string token, string recordingName = "", string recordingNote= "", string recordingSubject="")
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", SessionID),
                    new JProperty("status", NextStatus));

            int sessionReqType = (int)SessionReqType.UPDATE_SESSION;

            if (NextStatus == "startRecord" || NextStatus == "stopRecord")
            {
                if (!string.IsNullOrEmpty(recordingName))
                    param.Add("recordingName", recordingName);
                if (!string.IsNullOrEmpty(recordingName))
                    param.Add("recordingSubject", recordingSubject);
                if (!string.IsNullOrEmpty(recordingName))
                    param.Add("recordingNote", recordingNote);
            }
            if (NextStatus == "startRecord")
                sessionReqType = (int)SessionReqType.START_RECORD;
            else if (NextStatus == "stopRecord")
                sessionReqType = (int)SessionReqType.STOP_RECORD;

            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "updateSession", true, sessionReqType);
        }
        // Query Sessions
        public void QuerrySession(string token, string queryCondition = "")
        {
            JObject param = new JObject(
                    new JProperty("_auth", token));

            if(!string.IsNullOrEmpty(queryCondition))
            {
                param.Add("query", new JObject(new JProperty("status", queryCondition)));
            }

            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "querySessions", true, (int)SessionReqType.QUERRY_SESSION);
        }

        // Close Session
        public void CloseSession(string token)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", SessionID),
                    new JProperty("status", "close"));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "updateSession", true, (int)SessionReqType.CLOSE_SESSION);
        }
        // Update Note
        public void UpdateNote(string token, string recordID, string note)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", SessionID),
                    new JProperty("record", recordID),
                    new JProperty("note", note));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "updateNote", true, (int)SessionReqType.UPDATE_NOTE);
        }

        // Inject markers
        public bool InjectMarker(string token, string port, string label, int value, Int64 epocTime)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("port", port),
                    new JProperty("label", label),
                    new JProperty("value", value),
                    new JProperty("time", epocTime));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "injectMarker", true, (int)SessionReqType.INJECT_MARKER);
            return true;
        }
        // Request Data
        public void RequestData(string token, string stream, bool isReplay, bool isSubcribe)
        {
            JArray jStreamArr = new JArray();
            jStreamArr.Add(stream);

            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", SessionID),
                    new JProperty("streams", jStreamArr));
                    //new JProperty("replay", isReplay));

            if (isSubcribe)
            {
                CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "subscribe", true, (int)SessionReqType.SUBCRIBE_DATA);
            }
            else
            {
                CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "unsubscribe", true, (int)SessionReqType.UNSUBCRIBE_DATA);
            }
        }
        // Request Data
        public void RequestAllData(string token, string sessionId, bool isReplay, bool isSubcribe)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("session", sessionId),
                    new JProperty("streams", new JArray("eeg", "mot", "dev", "met")),
                    new JProperty("replay", isReplay));

            if (isSubcribe)
            {
                CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "subscribe", true, (int)SessionReqType.SUBCRIBE_DATA);
            }
            else
            {
                CortexClient.Instance.SendTextMessage(param, (int)StreamID.SESSION_STREAM, "unsubscribe", true, (int)SessionReqType.UNSUBCRIBE_DATA);
            }
        }

        // Handle Event Reponse
        public override void ParseData(JObject data, int requestType)
        {
            if (data["result"] != null)
            {
                JToken result = (JToken)data["result"];
                switch (requestType)
                {
                    case (int)SessionReqType.CREATE_SESSION:
                        Console.WriteLine("Create SESSION successfully");
                        // TODO: Store headset setting

                        // TODO: Store markers
                        string status = (string)result["status"];
                        if (status == "activated" || status == "opened")
                        {
                            _isCreateSession = true;
                            SessionID = (string)result["id"];
                        }
                        else
                        {
                            _isCreateSession = false;
                            _sessionID = "";
                        }
                        // TODO: Send create session successfully
                        break;
                    case (int)SessionReqType.QUERRY_SESSION:
                        Console.WriteLine("\nQuery SESSION successfully");
                        //send event queryHeadsets OK
                        JArray jSessions = (JArray)data["result"];

                        List<Session> sessionLists = new List<Session>();

                        foreach (JObject item in jSessions)
                        {
                            sessionLists.Add(new Session(item));
                        }
                        if(sessionLists.Count > 0)
                        {
                            SessionLists = sessionLists.ToList();
                        }
                        break;
                    case (int)SessionReqType.SUBCRIBE_DATA:
                        JArray jArrResult = (JArray)result;
                        foreach (var item in jArrResult)
                        {
                            if (item["eeg"] != null) // EEG
                            {
                                ArrayList eegChannelLists = new ArrayList();
                                JArray cols = (JArray)item["eeg"]["cols"];
                                foreach (var chanItem in cols)
                                {
                                    eegChannelLists.Add((string)chanItem);
                                }
                                if(eegChannelLists.Count > 0)
                                {
                                    OnSubcribeEEGOK(this, eegChannelLists);
                                }
                            }
                            else if (item["mot"] != null) // Motion
                            {
                                ArrayList motChannelLists = new ArrayList();
                                JArray cols = (JArray)item["mot"]["cols"];
                                foreach (var chanItem in cols)
                                {
                                    motChannelLists.Add((string)chanItem);
                                }
                                if (motChannelLists.Count > 0)
                                {
                                    OnSubcribeMotionOK(this, motChannelLists);
                                }
                            }
                            else if (item["dev"] != null)
                            {
                                ArrayList devChannelLists = new ArrayList();
                                JArray cols = (JArray)item["dev"]["cols"];
                                devChannelLists.Add((string)cols[0]); // Batery
                                devChannelLists.Add((string)cols[1]); // Signal Strength 

                                foreach (var chanItem in cols[2]) // Channel headset
                                {
                                    devChannelLists.Add((string)chanItem);
                                }
                                if (devChannelLists.Count > 0)
                                {
                                    OnSubcribeDevOK(this, devChannelLists);
                                }
                            }
                            else if (item["met"] != null) // Performance Metrics
                            {
                                ArrayList metChannelLists = new ArrayList();
                                JArray cols = (JArray)item["met"]["cols"];
                                foreach (var chanItem in cols)
                                {
                                    metChannelLists.Add((string)chanItem);
                                }
                                if (metChannelLists.Count > 0)
                                {
                                    OnSubcribeMetOK(this, metChannelLists);
                                }
                            }
                        }
                        break;
                    case (int)SessionReqType.UNSUBCRIBE_DATA:
                        JArray jResultUnSubcribe = (JArray)result;
                        foreach(var item in jResultUnSubcribe)
                        {
                            string message = (string)item["message"];
                            Console.WriteLine(message);
                        }
                        break;
                    case (int)SessionReqType.START_RECORD:
                        // TODO: Store record information
                        Console.WriteLine("\nStart RECORD successfully");
                        _isRecording = (bool)result["recording"];
                        break;
                    case (int)SessionReqType.STOP_RECORD:
                        Console.WriteLine("\nStop RECORD successfully");
                        JToken recordInfo = data["result"];
                        _isRecording = (bool)result["recording"];
                        // TODO: Send stop event
                        break;
                    case (int)SessionReqType.INJECT_MARKER:
                        // TODO: Store marker
                        JArray markers = (JArray)result["markers"];
                        Console.WriteLine("\nInject MARKERS successfully");
                        Console.WriteLine("\n##########List MARKERS#######");
                        foreach(var item in markers)
                        {
                            int code = (int)item["code"];
                            string label = (string)item["label"];
                            string port = (string)item["port"];
                            JArray jEvent = (JArray)item["events"];
                            foreach(var evtItem in jEvent)
                            {
                                string time = (string)evtItem[0];
                                string value = (string)evtItem[1]; // start and stop marker have value reversered (Eg: 1 vs -1)
                                Console.WriteLine("code:" + code.ToString() + " label: " + label + " port: " + port + " time: " + time + " value: " + value);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        // Clear Session Info
        public void ClearSessionControllerData()
        {
            SessionLists = null;
            SessionID = "";
            NextStatus = "close";
            _isCreateSession = false;
            _isRecording = false;
            RecordingName = "";
            RecordingNote = "";
            RecordingSubject = "";
        }
    }
}
