using System;
using System.Collections.Generic;
using System.Text;

namespace OSItemIndex.Observer
{
    public class Constants
    {
        public class Endpoints
        {
            public const string OSRSBoxVersion = "https://release-monitoring.org/api/project/32210"; // pypi project id 32210
            public const string OSRSBoxItems = "https://www.osrsbox.com/osrsbox-db/items-complete.json"; // static json-api

            public const string OSItemIndexAPIPost = "https://localhost:5001/api/items";
        }
    }
}
