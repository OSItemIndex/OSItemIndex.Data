using OSItemIndex.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSItemIndex.Observer.Services
{
    public interface IWikiRealtimePriceBackgroundService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> DeserializePricingModelAsync<T>(string endPoint) where T : IWikiRealtimePricingModel;
    }
}
