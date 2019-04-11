using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexAccess
{
    public class Process
    {
        // Member variables
        private CortexClient _wSC;
        private AccessController _accessCtr;
        private HeadsetController _headsetCtr;
        private SessionController _sessionCtr;
        private TrainingController _trainingCtr;
        private int _experimentID;
        private Dictionary<int, BaseController> _mapControllers;
        private string _licenseId;

        // Event
        public event EventHandler<ArrayList> OnMotionDataReceived;
        public event EventHandler<ArrayList> OnEEGDataReceived;
        public event EventHandler<ArrayList> OnDevDataReceived;
        public event EventHandler<ArrayList> OnPerfDataReceived;

        // Constructor
        public Process()
        {
            _wSC = CortexClient.Instance;
            AccessCtr = AccessController.Instance;
            HeadsetCtr = HeadsetController.Instance;
            SessionCtr = SessionController.Instance;
            TrainingCtr = TrainingController.Instance;
            _mapControllers = new Dictionary<int, BaseController>();
            LicenseId = "";

            // Event register
            _wSC.OnConnected += Connected;
            _wSC.OnMessageError += MessageErrorRecieved;
            _wSC.OnStreamDataReceived += StreamDataReceived;
            _wSC.OnEventReceived += EventReceived;
            AccessCtr.OnLoginOK += LoginOK;
            AccessCtr.OnAuthorizedOK += AuthorizeOK;
            HeadsetCtr.OnQuerryHeadsetOK += QuerryHeadsetReceived;
            HeadsetCtr.OnDisconnectHeadset += DisconnectHeadsetReceived;

            _mapControllers.Add((int)StreamID.AUTHORIZE_STREAM, AccessCtr);
            _mapControllers.Add((int)StreamID.HEADSETS_STREAM, HeadsetCtr);
            _mapControllers.Add((int)StreamID.SESSION_STREAM, SessionCtr);
            _mapControllers.Add((int)StreamID.TRAINING_STREAM, TrainingCtr);
            _wSC.Open();

        } 

        // Properties
        public AccessController AccessCtr
        {
            get
            {
                return _accessCtr;
            }

            set
            {
                _accessCtr = value;
            }
        }

        public HeadsetController HeadsetCtr
        {
            get
            {
                return _headsetCtr;
            }

            set
            {
                _headsetCtr = value;
            }
        }

        public SessionController SessionCtr
        {
            get
            {
                return _sessionCtr;
            }

            set
            {
                _sessionCtr = value;
            }
        }

        public TrainingController TrainingCtr
        {
            get
            {
                return _trainingCtr;
            }

            set
            {
                _trainingCtr = value;
            }
        }
        public int ExperimentID
        {
            get
            {
                return _experimentID;
            }

            set
            {
                _experimentID = value;
            }
        }

        public string LicenseId
        {
            get
            {
                return _licenseId;
            }

            set
            {
                _licenseId = value;
            }
        }

        public bool IsCreateSession
        {
            get
            {
                return SessionCtr.IsCreateSession;
            }
        }


        // Method
        public void Login(string username, string password)
        {
            AccessCtr.Login(username, password);
        }
        // Querry User Login
        public void QueryUserLogin()
        {
            AccessCtr.QueryUserLogin();
        }

        // Get current userlogin
        public string  GetUserLogin()
        {
            return AccessCtr.CurrentUserLogin;
        }

        // Change user login
        public void SetUserLogin(string newUsername, string newPassword)
        {
            // Check username existed in userlists

            // Change to username

            // Re-login with new username and new password

        }
        // Logout
        public void Logout(string username)
        {
            AccessCtr.Logout();
        }
        // Authorize license
        public void Authorize(string license = "", int debitNumber = 0)
        {
            LicenseId = license;
            AccessCtr.Authorize(license, debitNumber);
        }

        // Get Access Token
        public string GetAccessToken()
        {
            return AccessCtr.CurrentAccessToken;
        }

        //Query Headset
        public void QueryHeadset()
        {
            HeadsetCtr.QueryHeadsets();
        }

        // Get Selected Headset ID
        public string GetSelectedHeadsetId()
        {
            return HeadsetCtr.SelectedHeadsetId;
        }

        // check headset connected 
        public bool IsHeadsetConnected()
        {
            return HeadsetCtr.IsConnected;
        }

        // Change Headset
        public void SetHeadsetID(string headsetId)
        {
            // check headsetId existed in lists

            // change to new headsetID

        }

        // Set status for session
        //public void SetNextStatus(string status)
        //{
        //    SessionCtr.NextStatus = status;
        //}

        // Create Session
        public void CreateSession()
        {
            if (string.IsNullOrEmpty(LicenseId))
                SessionCtr.NextStatus = "open";
            else
                SessionCtr.NextStatus = "active";
            if (IsHeadsetConnected() && !string.IsNullOrEmpty(GetAccessToken()))
                SessionCtr.CreateSession(GetSelectedHeadsetId(), GetAccessToken(), ExperimentID);
        }

        // Query Session
        public void QuerySession()
        {
            SessionCtr.QuerrySession(GetAccessToken());
        }
        // Close Session
        public void CloseSession()
        {
            SessionCtr.CloseSession(GetAccessToken());
        }

        // Get current Session
        public string GetCurrentSessionID()
        {
            return SessionCtr.SessionID;
        }

        // Subcribe data
        public void SubcribeData(string stream)
        {
            SessionCtr.RequestData(GetAccessToken(), stream, false, true);
            //SessionCtr.RequestAllData(GetAccessToken(), GetCurrentSessionID(), false, true);
        }
        // Unsubcribe data
        public void UnSubcribeData(string stream)
        {
            SessionCtr.RequestData(GetAccessToken(), stream, false, false);
        }

        // Start Record
        public void StartRecord(string recordName, string recordSubject, string recordNote)
        {
            if(SessionCtr.IsCreateSession)
            {
                SessionCtr.NextStatus = "startRecord";
                SessionCtr.RecordingName = recordName;
                SessionCtr.RecordingSubject = recordSubject;
                SessionCtr.RecordingNote = recordNote;
                SessionCtr.UpdateSession(GetAccessToken(), recordName, recordNote, recordSubject);
            }
        }
        // Stop Record
        public void StopRecord()
        {
            if (SessionCtr.IsRecording)
            {
                SessionCtr.NextStatus = "stopRecord";
                SessionCtr.UpdateSession(GetAccessToken(), SessionCtr.RecordingName, SessionCtr.RecordingNote, SessionCtr.RecordingSubject);
            }
        }

        // Update Notes
        public void UpdateNote(string note)
        {
            SessionCtr.UpdateNote(GetAccessToken(), SessionCtr.RecordID, note);
        }
        // Inject marker
        public bool InjectMarker(string label, int value, string port, Int64 timeStamp)
        {
            if (SessionCtr.IsCreateSession)
                return SessionCtr.InjectMarker(GetAccessToken(), port, label, value, timeStamp);
            else
                return false;
        }
        // Get Detection Information
        public void QuerryDetectionInfo(string detection)
        {
            TrainingCtr.QuerryDetectionInformation(detection);
        }
        // Profile
        // Querry profile of user
        public void QuerryProfiles()
        {
            TrainingCtr.QuerryProfile(GetAccessToken());
        }

        // Get ProfileLists
        public List<string> GetAllProfiles()
        {
            return TrainingCtr.ProfileLists;
        }

        // Check profileLists existed
        public bool IsProfilesExisted(string profileName)
        {
            if(TrainingCtr.ProfileLists.Count > 0)
            {
                if(TrainingCtr.ProfileLists.Contains(profileName))
                {
                    return true;
                }
            }
            return false;
        }

        // Load profile
        public void LoadProfile(string profileName)
        {
            TrainingCtr.CurrentProfileName = profileName;
            TrainingCtr.LoadProfile(GetAccessToken(), GetSelectedHeadsetId(), profileName);
        }
        // Create profile
        public void CreateProfile(string profileName)
        {
            TrainingCtr.CurrentProfileName = profileName;
            TrainingCtr.CreateProfile(GetAccessToken(), GetSelectedHeadsetId() , profileName);
        }
        // Save a profile
        public void SaveProfile()
        {
            TrainingCtr.SaveProfile(GetAccessToken(), GetSelectedHeadsetId(), TrainingCtr.CurrentProfileName);
        }
        // Delete a profile
        public void DeleteProfile(string profileName)
        {
            TrainingCtr.DeleteProfile(GetAccessToken(), profileName);
        }
        // Edit a profile
        public void EditProfile(string profileName, string newProfileName)
        {
            TrainingCtr.EditProfile(GetAccessToken(), profileName, newProfileName);
        }
        // Upload Profile
        public void UploadProfile()
        {
            TrainingCtr.UploadProfile(GetAccessToken(), TrainingCtr.CurrentProfileName);
        }
        // Training
        // Start Mental Command Training
        public void StartCmd(string action)
        {
            TrainingCtr.CmdTrainingAction = action;
            TrainingCtr.RequestMentalCmdTraining(GetAccessToken(), (int)TrainingControl.START_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void AcceptCmd()
        {
            TrainingCtr.RequestMentalCmdTraining(GetAccessToken(), (int)TrainingControl.ACCEPT_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void RejectCmd()
        {
            TrainingCtr.RequestMentalCmdTraining(GetAccessToken(), (int)TrainingControl.REJECT_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void ResetCmd(string action)
        {
            TrainingCtr.CmdTrainingAction = action;
            TrainingCtr.RequestMentalCmdTraining(GetAccessToken(), (int)TrainingControl.RESET_TRAINING, GetCurrentSessionID());
        }

        // Start Facial Expression Training
        public void StartFE(string action)
        {
            TrainingCtr.FETrainingAction = action;
            TrainingCtr.RequestFETraining(GetAccessToken(), (int)TrainingControl.START_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void AcceptFE()
        {
            TrainingCtr.RequestFETraining(GetAccessToken(), (int)TrainingControl.ACCEPT_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void RejectFE()
        {
            TrainingCtr.RequestFETraining(GetAccessToken(), (int)TrainingControl.REJECT_TRAINING, GetCurrentSessionID());
        }
        // Start Mental Command Training
        public void ResetFE(string action)
        {
            TrainingCtr.FETrainingAction = action;
            TrainingCtr.RequestFETraining(GetAccessToken(), (int)TrainingControl.RESET_TRAINING, GetCurrentSessionID());
        }

        // Handle Event Response
        private void EventReceived(object sender, StreamDataEventArgs evt)
        {
            // Send to corresponding Controller
            int streamType = evt.StreamType;
            int requestType = evt.RequestType;
            // if stream Type Exist
            if (_mapControllers.ContainsKey(streamType))
            {
                _mapControllers[streamType].ParseData(evt.Data, requestType);

            }
            else
            {
                Console.WriteLine("can not detect stream type" + streamType.ToString());
            }
        }

        private void StreamDataReceived(object sender, StreamDataEventArgs evt)
        {
            // Send to corresponding Controller
            int streamType = evt.StreamType;
            int requestType = evt.RequestType;

            switch (streamType)
            {
                case (int)StreamID.MOTION_STREAM:
                    Console.WriteLine("Motion data received");
                    ArrayList motionData = new ArrayList();
                    JArray jMotData = (JArray)evt.Data["mot"];
                    foreach(var item in jMotData)
                    {
                        motionData.Add((float)item);
                    }
                    if (motionData.Count > 0)
                    {
                        OnMotionDataReceived(this, new ArrayList(motionData));
                    }
                    break;
                case (int)StreamID.EEG_STREAM:
                    Console.WriteLine("EEG data received");
                    ArrayList eegData = new ArrayList();

                    JArray jEEGData = (JArray)evt.Data["eeg"];
                    foreach (var item in jEEGData)
                    {
                        eegData.Add((float)item);
                    }
                    if(eegData.Count > 0)
                    {
                        OnEEGDataReceived(this, new ArrayList(eegData));
                    }
                    break;
                case (int)StreamID.DEVICE_STREAM:
                    Console.WriteLine("Device data received");
                    ArrayList devData = new ArrayList();

                    JArray jDevData = (JArray)evt.Data["dev"];
                    foreach (var item in jDevData)
                    {
                        devData.Add((float)item);
                    }
                    if (devData.Count > 0)
                    {
                        OnDevDataReceived(this, new ArrayList(devData));
                    }
                    break;
                case (int)StreamID.PERF_METRICS_STREAM:
                    Console.WriteLine("Performance metrics data received");
                    ArrayList perfData = new ArrayList();

                    JArray jPerfData = (JArray)evt.Data["met"];
                    foreach (var item in jPerfData)
                    {
                        perfData.Add((float)item);
                    }
                    if (perfData.Count > 0)
                    {
                        OnPerfDataReceived(this, new ArrayList(perfData));
                    }
                    break;
                case (int)StreamID.SYS_STREAM:
                    JArray jSysEvent = (JArray)evt.Data["sys"];
                    Console.WriteLine("Sys Event received: " + jSysEvent[0] + " : " + jSysEvent[1]);
                    break;
                default:
                    break;
            }
        }

        private void Connected(object sender, bool isConnected)
        {
            if (isConnected)
            {
                Console.WriteLine("Websocket Client Connected");
                // Query user login
                if (!AccessCtr.IsLogin)
                    AccessCtr.QueryUserLogin();
                // Query Headset
                if(!HeadsetCtr.IsConnected)
                    HeadsetCtr.QueryHeadsets();
            }
            else
            {
                Console.WriteLine("Websocket Client disconnect");
                if (SessionCtr.IsCreateSession)
                    CloseSession();
                // Clear session
                SessionCtr.ClearSessionControllerData();
            }
        }

        //Recieved error message
        public void MessageErrorRecieved(object sender, MessageErrorEventArgs e)
        {
            Console.WriteLine("Message Error recieved from " + sender.ToString());
            Console.WriteLine("Recieved: " + e.Code + " : " + e.MessageError);
        }

        private void QuerryHeadsetReceived(object sender, List<Headset> headsets)
        {
            if(headsets.Count == 0)
            {
                Console.WriteLine("No headset connected.");
            }
        }

        private void AuthorizeOK(object sender, string token)
        {
            Console.WriteLine("Authorize successfully!!!. Access Token " + token);

        }
        private void LoginOK(object sender, bool isLogin)
        {
            Console.WriteLine("Login successfully !!!");
        }

        private void DisconnectHeadsetReceived(object sender, string headsetID)
        {
            Console.WriteLine("Disconnect headset " + headsetID);
            if (SessionCtr.IsCreateSession)
            {
                CloseSession();
            }
            // Clear session
            SessionCtr.ClearSessionControllerData();
            

        }


    }
}
