using OSItemIndex.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OSItemIndex.Observer.Services
{
    public class Realtime
    {
        public enum RequestType
        {
            Latest, FiveMinute, OneHour
        }

        public static Dictionary<RequestType, string> RequestEndpoints = new Dictionary<RequestType, string>
        {
            { RequestType.Latest, Endpoints.WikiRealtime.PriceLatest },
            { RequestType.FiveMinute, Endpoints.WikiRealtime.PriceFiveMin },
            { RequestType.OneHour, Endpoints.WikiRealtime.PriceOneHour },
        };

        public static Dictionary<RequestType, string> OSItemIndexRequestEndpoints = new Dictionary<RequestType, string>
        {
            { RequestType.Latest, Endpoints.OSItemIndex.PricesRealtimeLatest },
            { RequestType.FiveMinute, Endpoints.OSItemIndex.PricesRealtimeFiveMin },
            { RequestType.OneHour, Endpoints.OSItemIndex.PricesRealtimeOneHour },
        };
    }

    public interface IRealtimePriceBackgroundService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns></returns>
        Task<IEnumerable<RealtimePrice>> DeserializePricingModelAsync(Realtime.RequestType requestType);
    }
}
