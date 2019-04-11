using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexAccess
{
    // Reponsible for login and manage access
    public sealed class AccessController : BaseController
    {
        public enum AccessState:int 
        {
            CHECK_LOGGED = 10,
            LOGIN = 11,
            AUTHEN = 12,
            REDEBIT = 13,
            GET_CLOUD_TOKEN = 14
        }
        const string ClientId = "4hsvfXAygLe5Asi4HOEhuDLtixJcaFvdTNSs2dfR";
        const string ClientSecret = "3ESVmzqpVf7bvwqjmSdrGTFDjseG4hoANyFGdCJZIpUmQAhKMYdjBlAsHamdoiUCDsN8Q32gVZRmfzVGEX8NMMujDsLvHbsbbYuNyqOsMBSV3qfVh2Fau5Szz2QnG2yO";

        // Member variable
        private static readonly AccessController _instance = new AccessController();
        private bool _isLogin;
        private string _currentAccessToken;
        private string _currentUserLogin;
        private List<string> _usernameList;

        // event
        public event EventHandler<bool> OnLoginOK;
        //public event EventHandler<bool> OnLogoutOK;
        public event EventHandler<string> OnAuthorizedOK;
        // Constructor
        static AccessController()
        {

        }
        private AccessController()
        {
            _isLogin = false;
            _currentAccessToken = "";
            _currentUserLogin = "";
            _usernameList = new List<string>();
        }

        // Properties
        public static AccessController Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool IsLogin
        {
            get
            {
                return _isLogin;
            }
        }

        public string CurrentAccessToken
        {
            get
            {
                return _currentAccessToken;
            }
        }

        public string CurrentUserLogin
        {
            get
            {
                return _currentUserLogin;
            }
        }

        public List<string> UsernameList
        {
            get
            {
                return _usernameList;
            }
        }

        // Method
        // Clear Token
        public void ClearAccessToken()
        {
            _currentAccessToken = "";
        }

        // Login
        public void Login(string username, string password)
        {
            JObject param = new JObject(
                    new JProperty("username", username),
                    new JProperty("password", password),
                    new JProperty("client_id", ClientId),
                    new JProperty("client_secret", ClientSecret)
                );
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.AUTHORIZE_STREAM, "login", true, (int)AccessState.LOGIN);
        }

        // Authorize
        public void Authorize(string licenseID, int debitNumber)
        {
            JObject param = new JObject();
            param.Add("client_id", ClientId);
            param.Add("client_secret", ClientSecret);
            if (String.IsNullOrEmpty(licenseID))
            {
                param.Add("debit", 0);
            }
            else
            {
                param.Add("license", licenseID);
                param.Add("debit", debitNumber);
            }
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.AUTHORIZE_STREAM, "authorize", true, (int)AccessState.AUTHEN);
        }

        // Query user login
        public void QueryUserLogin()
        {
            JObject param = new JObject();
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.AUTHORIZE_STREAM, "getUserLogin", false, (int)AccessState.CHECK_LOGGED);
        }
        // RefreshToken
        public void GenerateNewToken()
        {
            JObject param = new JObject(
                    new JProperty("token", CurrentAccessToken));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.AUTHORIZE_STREAM, "generateNewToken", true, (int)AccessState.AUTHEN);
        }
        public void Debit(int debitNumber, string licenseID)
        {
            JObject param = new JObject(
                    new JProperty("client_id", ClientId),
                    new JProperty("client_secret", ClientSecret),
                    new JProperty("license", licenseID),
                    new JProperty("debit", debitNumber)
                );
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.AUTHORIZE_STREAM, "authorize", true, (int)AccessState.REDEBIT);
        }

        // Logout
        public void Logout()
        {
            if (!IsLogin)
                return;

            JObject param = new JObject(
                   new JProperty("username", CurrentUserLogin));
            CortexClient.Instance.SendTextMessage(param, (int)StreamID.AUTHORIZE_STREAM, "logout", true, (int)AccessState.LOGIN);
        }

        public override void ParseData(JObject data, int requestType)
        {
            if (data["result"] != null)
            {
                switch (requestType)
                {
                    case (int)AccessState.CHECK_LOGGED:
                        JArray jUsernameArr = (JArray)data["result"];
                        foreach (var item in jUsernameArr)
                        {
                            string itemObject = (string)item;
                            if(!String.IsNullOrEmpty(itemObject))
                                UsernameList.Add(itemObject);
                        }
                        if (UsernameList.Count > 0)
                        {
                            _currentUserLogin = UsernameList[0];
                            _isLogin = true;
                        }
                        else
                        {
                            _currentUserLogin = "";//reset
                            _isLogin = false;
                        }
                        // Set element 0 as current user
                        break;
                    case (int)AccessState.LOGIN:

                        Console.WriteLine("Login successfully");
                        if(String.IsNullOrEmpty(_currentUserLogin))
                        {
                            QueryUserLogin();
                        }
                        else
                        {
                            OnLoginOK(this, true);
                        }
                        break;

                    case (int)AccessState.AUTHEN:

                        string token = (string)data["result"]["_auth"];
                        _currentAccessToken = token;
                        // send Authorize successfully
                        OnAuthorizedOK(this, token);

                        break;
                    case (int)AccessState.REDEBIT:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
