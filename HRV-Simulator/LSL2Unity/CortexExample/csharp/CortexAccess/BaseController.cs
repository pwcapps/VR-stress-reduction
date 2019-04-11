using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CortexAccess
{
    public abstract class BaseController
    {
        public BaseController()
        {

        }

        abstract public void ParseData(JObject data, int requestType = 0 );
    }
}
