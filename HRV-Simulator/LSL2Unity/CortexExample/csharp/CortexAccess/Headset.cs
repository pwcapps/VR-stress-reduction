using Newtonsoft.Json.Linq;
using System;

namespace CortexAccess
{
    //enum HEADSET_TYPE {
    //    TYPE_UNKNOWN = 0,
    //    TYPE_EPOC,
    //    TYPE_EPOC_PLUS,
    //    TYPE_INSIGHT
    //}

    public class Headset
    {
        private string _headsetID;
        private string _status;
        private string _type;
        private string _serialId;
        private string _firmwareVersion;
        private string _dongleSerial;
        private string _connectionType;

        // Contructor
        public Headset()
        {
            _headsetID = "";
            _status = "";
            _type = "";
            _serialId = "";
            _firmwareVersion = "";
            _dongleSerial = "";
            _connectionType = "";
        }
        public Headset (JObject jHeadset)
        {
            if(jHeadset["id"] != null)
            {
                HeadsetID = (string)jHeadset["id"];
                string id = (string)jHeadset.GetValue("id");
                string[] strSeperator = { "-" };
                string[] headsetIdInfo = id.Split(strSeperator, StringSplitOptions.RemoveEmptyEntries);
                
                HeadsetID = id;
                Type = headsetIdInfo[0];
                SerialId = headsetIdInfo[1];

                Status = (string)jHeadset["status"];
                FirmwareVersion = (string)jHeadset["firmware"];
                DongleSerial = (string)jHeadset["dongle"];
                ConnectionType = (string)jHeadset["connectedBy"];
            }
        }

        // Properties
        public string HeadsetID
        {
            get
            {
                return _headsetID;
            }

            set
            {
                _headsetID = value;
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

        public string Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

        public string SerialId
        {
            get
            {
                return _serialId;
            }

            set
            {
                _serialId = value;
            }
        }

        public string FirmwareVersion
        {
            get
            {
                return _firmwareVersion;
            }

            set
            {
                _firmwareVersion = value;
            }
        }

        public string DongleSerial
        {
            get
            {
                return _dongleSerial;
            }

            set
            {
                _dongleSerial = value;
            }
        }

        public string ConnectionType
        {
            get
            {
                return _connectionType;
            }

            set
            {
                _connectionType = value;
            }
        }

    }
}
