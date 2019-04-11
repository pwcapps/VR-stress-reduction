using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Timers;

namespace CortexAccess
{
    public sealed class HeadsetController : BaseController
    {
        public enum HeadsetRqType : int
        {
            QUERRY_HEADSET = 10,
            HEADSET_SETTING = 11
        }
        public const int QueryHeadsetTime = 10000; // duration to trigger query headset event

        // Member variable
        private static readonly HeadsetController _instance = new HeadsetController();
        private string _selectedHeadsetId; //selected headset
        private List<Headset> _headsetLists;
        private Timer _queryHeadsetTimer;
        private bool _isConnected;

        // Event
        public event EventHandler<List<Headset>> OnQuerryHeadsetOK;
        public event EventHandler<string> OnDisconnectHeadset;

        // Constructor
        static HeadsetController()
        {

        }
        private HeadsetController()
        {
            HeadsetLists = new List<Headset>();
            _selectedHeadsetId = "";
            _isConnected = false;

            // Set Timer
            _queryHeadsetTimer = new Timer(QueryHeadsetTime);
            _queryHeadsetTimer.Elapsed += OnTimeoutEvent;
            _queryHeadsetTimer.AutoReset = true;
            _queryHeadsetTimer.Enabled = true;

        }

        private void OnTimeoutEvent(object sender, ElapsedEventArgs e)
        {
            Instance.QueryHeadsets();
        }

        // Properties
        public static HeadsetController Instance
        {
            get
            {
                return _instance;
            }
        }

        public string SelectedHeadsetId
        {
            get
            {
                return _selectedHeadsetId;
            }

            set
            {
                _selectedHeadsetId = value;
            }
        }

        public List<Headset> HeadsetLists
        {
            get
            {
                return _headsetLists;
            }
            set
            {
                _headsetLists = value;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        // Methods
        // Send request to Websocket client
        public void QueryHeadsets()
        {
            JObject param = new JObject();
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.HEADSETS_STREAM, "queryHeadsets", true, (int)HeadsetRqType.QUERRY_HEADSET);
        }

        public override void ParseData(JObject data, int requestType)
        {

            if (data["result"] != null)
            {
                switch (requestType)
                {
                    case (int)HeadsetRqType.QUERRY_HEADSET:
                        //Console.WriteLine("Querry Headset successfully");
                        //send event queryHeadsets OK
                        JArray jHeadsetArr = (JArray)data["result"];

                        List<Headset> headsetLists = new List<Headset>();

                        foreach (JObject item in jHeadsetArr)
                        {
                            headsetLists.Add(new Headset(item));

                            //HeadsetLists.Add(new Headset(item));
                        }
                        if (headsetLists.Count > 0)
                        {
                            
                            HeadsetLists = headsetLists.ToList();
                            OnQuerryHeadsetOK(this, new List<Headset>(HeadsetLists));

                            if (HeadsetLists[0].Status == "paired")
                            {
                                OnDisconnectHeadset(this, HeadsetLists[0].HeadsetID);

                                // TODO: Switch to next available headset
                            }
                            else if(HeadsetLists[0].Status == "connected")
                            {
                                // Set element 0 as current headset
                                if(!SelectedHeadsetId.Contains(HeadsetLists[0].HeadsetID))
                                {
                                    Console.WriteLine("Headset Connected");
                                    _isConnected = true;
                                    SelectedHeadsetId = HeadsetLists[0].HeadsetID;
                                }
                                   
                                Console.WriteLine("Selected HeadsetID " + SelectedHeadsetId);
                            }
                        }
                        else
                        {
                            SelectedHeadsetId = "";
                            _isConnected = false;
                            HeadsetLists.Clear();
                            Console.WriteLine("No headset available");
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
