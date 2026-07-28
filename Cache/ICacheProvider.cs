using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthDataExport.Tools.Cache
{
    public class ICacheProvider
    {
        public async Task<object> GetAsync(object key)
        {
            // existing code
        }
    }
}