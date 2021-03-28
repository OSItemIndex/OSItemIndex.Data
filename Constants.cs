namespace OSItemIndex.Observer
{
    public class Constants
    {
        public class Endpoints
        {
            public const string OSRSBoxVersion = "https://release-monitoring.org/api/project/32210"; // pypi project id 32210
            public const string OSRSBoxItems = "https://www.osrsbox.com/osrsbox-db/items-complete.json"; // static json-api

            public const string WikiRealtimePriceLatest = "https://prices.runescape.wiki/api/v1/osrs/latest";
            public const string WikiRealtimePriceFiveMin = "https://prices.runescape.wiki/api/v1/osrs/5m";
            public const string WikiRealtimePriceOneHour = "https://prices.runescape.wiki/api/v1/osrs/1h";

            public const string OSItemIndexAPIPost = "https://localhost:5001/api/items";
            public const string OSItemIndexAPIStats = "https://localhost:5001/api/items/stats";
        }

        // Our user-agent all requests use so we can easily be identified and contacted
        public const string OSItemIndexObserverUserAgent = "OSItemIndex + OSItemIndex.Observer/1.00 + github.com/OSItemIndex";
    }
}
