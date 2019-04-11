using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace CortexAccess
{
    public sealed class TrainingController : BaseController
    {
        public enum TrainingReqType : int
        {
            CREATE_NEW_PROFILE = 10,
            SAVE_PROFILE = 11,
            LOAD_PROFILE = 12,
            QUERY_PROFILE_LIST = 13,
            DELETE_PROFILE = 14,
            EDIT_PROFILE = 15,
            UPLOAD_PROFILE = 16,
            GET_DETECTION_INFO =20,
            MCC_START = 30,
            MCC_RESET = 31,
            MCC_REJECT = 32,
            MCC_ERASE = 33,
            MCC_ACCEPT = 34,
            FE_START = 40,
            FE_ACCEPT = 41,
            FE_REJECT = 42,
            FE_ERASE = 43,
            FE_RESET = 44

        }

        // Member variable
        private static readonly TrainingController _instance = new TrainingController();

        private List<string> _profileLists;
        private string _currentProfileName;

        private string _cmdTrainingAction;
        private string _feTrainingAction;
        private ArrayList _actionLists;
        private ArrayList _controlLists;
        private ArrayList _eventLists;
        private bool _isLoaded;

        // event
        //public event EventHandler<bool> OnLogoutOK;
        // Constructor
        static TrainingController()
        {

        }
        private TrainingController()
        {
            CmdTrainingAction = "";
            ActionLists = new ArrayList();
            ControlLists = new ArrayList();
            EventLists = new ArrayList();
            ProfileLists = new List<string>();
            IsLoaded = false;
        }

        // Properties
        public static TrainingController Instance
        {
            get
            {
                return _instance;
            }
        }

        public List<string> ProfileLists
        {
            get
            {
                return _profileLists;
            }

            set
            {
                _profileLists = value;
            }
        }

        public string CurrentProfileName
        {
            get
            {
                return _currentProfileName;
            }

            set
            {
                _currentProfileName = value;
            }
        }

        public string CmdTrainingAction
        {
            get
            {
                return _cmdTrainingAction;
            }

            set
            {
                _cmdTrainingAction = value;
            }
        }

        public string FETrainingAction
        {
            get
            {
                return _feTrainingAction;
            }

            set
            {
                _feTrainingAction = value;
            }
        }

        public ArrayList ActionLists
        {
            get
            {
                return _actionLists;
            }

            set
            {
                _actionLists = value;
            }
        }

        public ArrayList ControlLists
        {
            get
            {
                return _controlLists;
            }

            set
            {
                _controlLists = value;
            }
        }

        public ArrayList EventLists
        {
            get
            {
                return _eventLists;
            }

            set
            {
                _eventLists = value;
            }
        }

        public bool IsLoaded
        {
            get
            {
                return _isLoaded;
            }

            private set
            {
                _isLoaded = value;
            }
        }

        // Method
        // Send request to Client
        //public void GetCurrentProfile()
        //{

        //}

        // Querry profile of user
        public void QuerryProfile(string token)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.TRAINING_STREAM, "queryProfile", true, (int)TrainingReqType.QUERY_PROFILE_LIST);
        }

        // Load profile
        public void LoadProfile(string token, string headsetID, string profileName)
        {
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("headset", headsetID),
                    new JProperty("profile", profileName),
                    new JProperty("status", "load"));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.TRAINING_STREAM, "setupProfile", true, (int)TrainingReqType.LOAD_PROFILE);
        }
        // Create profile
        public void CreateProfile(string token, string headsetID, string profileName)
        {
            // Check profile existed
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("headset", headsetID),
                    new JProperty("profile", profileName),
                    new JProperty("status", "create"));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.TRAINING_STREAM, "setupProfile", true, (int)TrainingReqType.CREATE_NEW_PROFILE);
        }
        // Save a profile
        public void SaveProfile(string token, string headsetID, string profileName)
        {
            // Check current profile loaded
            if (string.IsNullOrEmpty(profileName) || !IsLoaded)
                return;

            // Save Profile
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("headset", headsetID),
                    new JProperty("profile", profileName),
                    new JProperty("status", "save"));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.TRAINING_STREAM, "setupProfile", true, (int)TrainingReqType.SAVE_PROFILE);
        }

        // upload a profile
        public void UploadProfile(string token, string profileName)
        {
            // Save Profile
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("profile", profileName),
                    new JProperty("status", "upload"));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.TRAINING_STREAM, "setupProfile", true, (int)TrainingReqType.UPLOAD_PROFILE);
        }
        // Delete a profile
        public void DeleteProfile(string token, string profileName)
        {
            // Delete profile from Profile lists

            // Send delete profile request
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("profile", profileName),
                    new JProperty("status", "delete"));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.TRAINING_STREAM, "setupProfile", true, (int)TrainingReqType.DELETE_PROFILE);
        }
        // Edit a profile
        public void EditProfile(string token, string profileName, string newProfileName)
        {
            // Rename profile in Profile lists

            // Send edit profile request
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("profile", profileName),
                    new JProperty("newProfileName", newProfileName),
                    new JProperty("status", "rename"));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.TRAINING_STREAM, "setupProfile", true, (int)TrainingReqType.EDIT_PROFILE);
        }
        // Get Detection Information
        public void QuerryDetectionInformation(string detection)
        {
            JObject param = new JObject(
                    new JProperty("detection", detection));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.TRAINING_STREAM, "getDetectionInfo", true, (int)TrainingReqType.GET_DETECTION_INFO);
        }
        // Request Mental Command Training
        public void RequestMentalCmdTraining(string token, int trainingControl, string sessionID)
        {
            string trainingCtr = "";
            int trainingReqType = 0;

            switch(trainingControl)
            {
                case (int)TrainingControl.START_TRAINING:
                    trainingCtr = "start";
                    trainingReqType = (int)TrainingReqType.MCC_START;
                    break;
                case (int)TrainingControl.ACCEPT_TRAINING:
                    trainingCtr = "accept";
                    trainingReqType = (int)TrainingReqType.MCC_ACCEPT;
                    break;
                case (int)TrainingControl.REJECT_TRAINING:
                    trainingCtr = "reject";
                    trainingReqType = (int)TrainingReqType.MCC_REJECT;
                    break;
                case (int)TrainingControl.RESET_TRAINING:
                    trainingCtr = "reset";
                    trainingReqType = (int)TrainingReqType.MCC_RESET;
                    break;
                default:
                    Console.WriteLine("Unknown mental command training control");
                    break;
            }
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("detection", "mentalCommand"),
                    new JProperty("action", CmdTrainingAction),
                    new JProperty("session", sessionID),
                    new JProperty("status", trainingCtr));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.TRAINING_STREAM, "training", true, trainingReqType);
        }
        // Request Facial Expression Training
        public void RequestFETraining(string token, int trainingControl, string sessionID)
        {
            string trainingCtr = "";
            int trainingReqType = 0;

            switch (trainingControl)
            {
                case (int)TrainingControl.START_TRAINING:
                    trainingCtr = "start";
                    trainingReqType = (int)TrainingReqType.FE_START;
                    break;
                case (int)TrainingControl.ACCEPT_TRAINING:
                    trainingCtr = "accept";
                    trainingReqType = (int)TrainingReqType.FE_ACCEPT;
                    break;
                case (int)TrainingControl.REJECT_TRAINING:
                    trainingCtr = "reject";
                    trainingReqType = (int)TrainingReqType.FE_REJECT;
                    break;
                case (int)TrainingControl.RESET_TRAINING:
                    trainingCtr = "reset";
                    trainingReqType = (int)TrainingReqType.FE_RESET;
                    break;
                default:
                    Console.WriteLine("Unknown facial expression training control");
                    break;
            }
            JObject param = new JObject(
                    new JProperty("_auth", token),
                    new JProperty("detection", "facialExpression"),
                    new JProperty("action", FETrainingAction),
                    new JProperty("session", sessionID),
                    new JProperty("status", trainingCtr));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.TRAINING_STREAM, "training", true, trainingReqType);
        }



        public override void ParseData(JObject data, int requestType = 0)
        {
            if (data["result"] != null)
            {
                JToken result = (JToken)data["result"];
                switch (requestType)
                {
                    case (int)TrainingReqType.GET_DETECTION_INFO:
                        // Store actions lists
                        // Training Action
                        JArray jActionsArr = (JArray)result["actions"];
                        foreach (var item in jActionsArr)
                        {
                            ActionLists.Add((string)item);
                        }
                        // Training Control
                        JArray jControlArr = (JArray)result["controls"];
                        foreach (var item in jControlArr)
                        {
                            ControlLists.Add((string)item);
                        }
                        // Training Events
                        JArray jEventArr = (JArray)result["events"];
                        foreach (var item in jEventArr)
                        {
                            EventLists.Add((string)item);
                        }
                        break;
                    case (int)TrainingReqType.CREATE_NEW_PROFILE:
                        Console.WriteLine("Create Profile Successfully");
                        break;
                    case (int)TrainingReqType.LOAD_PROFILE:
                        IsLoaded = true;
                        Console.WriteLine("Load Profile Successfully");
                        break;
                    case (int)TrainingReqType.SAVE_PROFILE:
                        Console.WriteLine("Save Profile Successfully");
                        break;
                    case (int)TrainingReqType.UPLOAD_PROFILE:
                        Console.WriteLine("Upload Profile Successfully");
                        break;
                    case (int)TrainingReqType.QUERY_PROFILE_LIST:
                        Console.WriteLine("Querry Profile Successfully");
                        JArray jProfileArr = (JArray)result;
                        List<string> profileNameLists = new List<string>();
                        foreach (var item in jProfileArr)
                        {
                            profileNameLists.Add((string)item["name"]);
                        }
                        if(profileNameLists.Count > 0)
                        {
                            ProfileLists = profileNameLists.ToList();

                        }
                        else
                        {
                            ProfileLists.Clear();
                        }
                        break;
                    case (int)TrainingReqType.MCC_START:
                    case (int)TrainingReqType.MCC_ACCEPT:
                    case (int)TrainingReqType.MCC_REJECT:
                    case (int)TrainingReqType.MCC_RESET:
                        Console.WriteLine("Mental Command Training: " + (string)result);
                        break;
                    case (int)TrainingReqType.FE_START:
                    case (int)TrainingReqType.FE_ACCEPT:
                    case (int)TrainingReqType.FE_REJECT:
                    case (int)TrainingReqType.FE_RESET:
                        Console.WriteLine("Facial Expression Training: " + (string)result);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
