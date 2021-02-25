using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OSItemIndex.Observer.Services
{
    // Use Tasks - https://www.c-sharpcorner.com/article/task-and-thread-in-c-sharp/

    public interface IObserverService : IHostedService, IDisposable
    {

    }
}
