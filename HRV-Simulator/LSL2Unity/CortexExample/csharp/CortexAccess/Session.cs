using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexAccess
{
    public class Session
    {
        //constructor
        public Session() { }
        
        public Session(JObject jSession)
        {
            //need check null
            if (jSession["headset"] != null)
            {
                Headset = new Headset((JObject)jSession["headset"]);
            }
            SessionID = (string)jSession["id"];
            Status = (string)jSession["status"];
            LicenseID = (string)jSession["license"];
            StartedTime = (string)jSession["started"];
            JToken stopVal = jSession.GetValue("stopped");
            if (stopVal.Type != JTokenType.Null)
            {
                StoppedTime = (string)jSession["stopped"];
            }
            JToken recordingVal = jSession.GetValue("recording");
            if(recordingVal.Type != JTokenType.Null)
            {
                IsRecording = (bool)jSession["recording"];
            }

        }

        //Field
        private Headset _headset;
        private string _sessionID;
        private string _licenseID;
        //private string _profileID;
        // Markers
        private bool _isRecording;
        private string _startedTime;
        private string _stoppedTime;

        private string _status;
        private string _owner;

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

        public string LicenseID
        {
            get
            {
                return _licenseID;
            }

            set
            {
                _licenseID = value;
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }

        public string Owner
        {
            get
            {
                return _owner;
            }

            set
            {
                _owner = value;
            }
        }
        public bool IsRecording
        {
            get
            {
                return _isRecording;
            }

            set
            {
                _isRecording = value;
            }
        }

        public string StartedTime
        {
            get
            {
                return _startedTime;
            }

            set
            {
                _startedTime = value;
            }
        }

        public string StoppedTime
        {
            get
            {
                return _stoppedTime;
            }

            set
            {
                _stoppedTime = value;
            }
        }

        public Headset Headset
        {
            get
            {
                return _headset;
            }

            set
            {
                _headset = value;
            }
        }
    }
}
