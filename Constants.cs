using System.Collections.Generic;

namespace OSItemIndex.Observer
{
    public class Constants
    {
        // Our user-agent all requests use so we can easily be identified and contacted
        public const string ObserverUserAgent = "OSItemIndex + OSItemIndex.Observer/1.00 + github.com/OSItemIndex";
    }

    public class Endpoints
    {
        public class OSRSBox
        {
            public const string Project = "https://release-monitoring.org/api/project/32210"; // pypi project id 32210
            public const string ItemsComplete = "https://www.osrsbox.com/osrsbox-db/items-complete.json"; // static json-api
        }

        public class WikiRealtime
        {
            public const string PriceLatest = "https://prices.runescape.wiki/api/v1/osrs/latest";
            public const string PriceFiveMin = "https://prices.runescape.wiki/api/v1/osrs/5m";
            public const string PriceOneHour = "https://prices.runescape.wiki/api/v1/osrs/1h";

            public static string[] OSItemIndexRequestEndpoints = new string[]
            {
                OSItemIndex.PricesRealtimeLatest,
                OSItemIndex.PricesRealtimeFiveMin,
                OSItemIndex.PricesRealtimeOneHour
            };
        }

        public class OSItemIndex
        {
            public const string BaseApiUri = "https://localhost:5001/api";
            public const string Items = BaseApiUri + "/items";
            public const string ItemsStatistics = BaseApiUri + "/items/stats";
            public const string Prices = BaseApiUri + "/prices";
            public const string PricesRealtimeLatest = BaseApiUri + "/prices/realtime/latest";
            public const string PricesRealtimeFiveMin = BaseApiUri + "/prices/realtime/5m";
            public const string PricesRealtimeOneHour = BaseApiUri + "/prices/realtime/1h";
        }
    }
}
